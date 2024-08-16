using System.Collections.Generic;
using Core.Quests.Tips;
using Core.Quests.Tips.Impl;
using Core.Services;
using Core.Services.Experience;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using SharedUtils;
using TMPro;
using Ui.Sandbox.CharacterMenu;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VContainer.Unity;
using VContainerUi.Abstraction;

namespace Ui.Sandbox.Experience
{
	public class ExperienceView : UiView
	{
		public TextMeshProUGUI CurrentLevelTxt;
		public string LvlFormat;
		
		public TextMeshProUGUI CurrentExperienceTxt;
		public string ExperienceFormat;

		public TextMeshProUGUI AvailablePoints;
		public GameObject AvailableGO;
		
		public Image ProgressBarImg;
		public Image FullBarImg;
		public GameObject Content;
		public Image ClickImage;
		public QuestTipContext[] Tips;
	}

	public class ExperienceUiController : UiController<ExperienceView>, IStartable
	{
		private readonly IExperienceService _experienceService;
		private readonly IMenuPanelService _menuPanelService;
		private readonly IQuestTipService _questTipService;
		private readonly CharacterMenuController _characterMenuController;
		private readonly Queue<ExperienceLvlEvent> _experienceEvents = new Queue<ExperienceLvlEvent>();
		private bool _isAnimating;
		private int _currentLevel;
		private float _experience;
		private float _expToLevelUp;

		public ExperienceUiController(
			IExperienceService experienceService, 
			IMenuPanelService menuPanelService,
			IQuestTipService questTipService,
			CharacterMenuController characterMenuController)
		{
			_experienceService = experienceService;
			_menuPanelService = menuPanelService;
			_questTipService = questTipService;
			_characterMenuController = characterMenuController;
		}
		
		public void Start()
		{
			_experienceService.OnExperienceChanged.Subscribe(OnExperienceEvent).AddTo(View);
			_experienceService.AvailablePoints.Subscribe(OnAvailablePoints).AddTo(View);
			View.ClickImage.OnPointerClickAsObservable().Subscribe(OnClickExp).AddTo(View);
			SetLevel(_experienceService.CurrentLevel.Value);
			View.ProgressBarImg.fillAmount = _experienceService.CurrentExperience.Value / _experienceService.GetExperienceToLevel(_experienceService.CurrentLevel.Value);
			SetExperience(_experienceService.CurrentExperience.Value);
			
			foreach (var tip in View.Tips)
				_questTipService.AddTip(tip);
		}
		
		private void OnAvailablePoints(int obj)
		{
			if (obj <= 0)
			{
				View.AvailableGO.SetActive(false);
			}
			else
			{
				View.AvailableGO.SetActive(true);
				View.AvailablePoints.text = $"+{Mathf.Min(obj, 9)}";
			}
		}

		private void OnClickExp(PointerEventData obj)
		{
			_menuPanelService.SelectMenu("CharacterMenuWindow");
			_characterMenuController.SelectSkillsScreen();
		}

		private void OnExperienceEvent(ExperienceLvlEvent obj)
		{
			if (!obj.IsAvailable)
			{
				View.Content.SetActive(false);
				return;
			}
			View.Content.SetActive(true);

			CheckOnMaxLevel();

			if (obj.Force)
			{
				View.ProgressBarImg.fillAmount = obj.Experience / obj.ExpToLevelUp;
				SetLevel(obj.NewLevel);
				SetExperience(obj.Experience);
			}
			else
			{
				_experienceEvents.Enqueue(obj);
				if (_isAnimating) 
					return;
				Animate().Forget();
			}
		}

		private void CheckOnMaxLevel()
		{
			View.FullBarImg.enabled = _experienceService.IsMaxLevel();
		}
		
		private async UniTaskVoid Animate()
		{
			_isAnimating = true;
			await UniTask.NextFrame(cancellationToken: View.destroyCancellationToken);
			while (_experienceEvents.TryDequeue(out var evt))
			{
				var to = Mathf.Clamp01(evt.Experience / evt.ExpToLevelUp);
				
				SetExperienceAnimation(evt.Experience);
				
				await LMotion
					.Create(View.ProgressBarImg.fillAmount, to, 1f)
					.BindToFillAmount(View.ProgressBarImg)
					.ToUniTask(View.destroyCancellationToken);
				
				if (_currentLevel != evt.NewLevel)
				{
					await LevelUp(evt.NewLevel);
					await UniTask.Delay(0.75f.ToSec(), cancellationToken: View.destroyCancellationToken);


					View.ProgressBarImg.fillAmount = 0f;
				}
				await UniTask.NextFrame(cancellationToken: View.destroyCancellationToken);
			}
			
			_isAnimating = false;
		}

		private async UniTask LevelUp(int resultNewLevel)
		{
			await LMotion
				.Create(Vector3.one, Vector3.one * 1.5f, 0.35f)
				.WithEase(Ease.OutQuad)
				.BindToTMPCharScale(View.CurrentLevelTxt, View.CurrentLevelTxt.textInfo.characterCount - 1)
				.ToUniTask(View.destroyCancellationToken);
			
			await LMotion
				.Create(Vector3.one * 1.5f, Vector3.one, 0.45f)
				.WithEase(Ease.InQuad)
				.BindToTMPCharScale(View.CurrentLevelTxt, View.CurrentLevelTxt.textInfo.characterCount - 1)
				.ToUniTask(View.destroyCancellationToken);

			SetLevel(resultNewLevel);
		}

		private void SetLevel(int level)
		{
			_currentLevel = level;
			View.CurrentLevelTxt.text = string.Format(View.LvlFormat, _currentLevel);
		}

		public void SetExperience(float experience)
		{
			_experience = (int)GetExperienceToLevel(experience);
			View.CurrentExperienceTxt.text = string.Format(View.ExperienceFormat, _experience);
		}
		
		public void SetExperienceAnimation(float experience)
		{
			var expToLevel = (int)GetExperienceToLevel(experience);
			
			LMotion
				.Create((int) _experience, expToLevel, 1f)
				.WithEase(Ease.OutQuad)
				.WithOnComplete(CheckOnMaxLevel)
				.BindWithState(View.CurrentExperienceTxt, ApplyExp)
				.AddTo(View);
			
			_experience = expToLevel;
		}
		
		private float GetExperienceToLevel(float experience)
		{
			return experience + _currentLevel * _experienceService.GetExperienceToLevel(_currentLevel);
		}

		private void ApplyExp(int arg1, TextMeshProUGUI arg2)
		{
			arg2.text = string.Format(View.ExperienceFormat, arg1);
		}
	}
}