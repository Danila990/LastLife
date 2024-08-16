using System;
using System.Globalization;
using AnnulusGames.LucidTools.Audio;
using Core.Entity.Characters;
using Core.Equipment;
using Core.Equipment.Data;
using Core.Equipment.Impl;
using Core.Equipment.Impl.JetPack;
using Core.Fuel;
using Core.InputSystem;
using MessagePipe;
using Sirenix.OdinInspector;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using VContainer.Unity;
using VContainerUi.Abstraction;


namespace Ui.Sandbox.JetPack
{
	public class JetPackView : UiView
	{
		public Image FilledImage;
		public TextMeshProUGUI Fuel;
		public GameObject Content;
		public Button RefuelButton;
		public string Format;
	}

	public class JetPackController : UiController<JetPackView>, IStartable, IDisposable
	{
		private const float PERCENT_TO_SHOW = 0.95f;
		
		private readonly ISubscriber<PlayerContextChangedMessage> _subscriber;
		private readonly IFuelService _fuelService;

		private CharacterContext _playerContext;
		
		private CompositeDisposable _jetPackObserving;
		private CompositeDisposable _fuelObserving;
		private AudioPlayer _audioPlayer;

		private float _currentFuel;
		private float _maxFuel;
		
		public JetPackController(
			ISubscriber<PlayerContextChangedMessage> subscriber,
			IFuelService fuelService
		)
		{
			_subscriber = subscriber;
			_fuelService = fuelService;
		}

		public void Start()
		{
			_subscriber.Subscribe(OnPlayerChanged).AddTo(View);
			View.RefuelButton.onClick.AddListener(Refuel);
		}
		
		private void OnPlayerChanged(PlayerContextChangedMessage context)
		{
			_jetPackObserving?.Dispose();
			_fuelObserving?.Dispose();

			_currentFuel = 0;
			_maxFuel = 0;
			
			_playerContext = context.CharacterContext;
			
			if(!context.Created)
				return;

			_jetPackObserving = new CompositeDisposable();
			var activeEquipment = context.CharacterContext.EquipmentInventory.Controller.ActiveEquipment;
			activeEquipment.OnEquip.SubscribeWithState(context, (entityContext, _) => OnJetPackChanged(entityContext, true)).AddTo(_jetPackObserving);
			activeEquipment.OnUnequip.SubscribeWithState(context, (entityContext, _) => OnJetPackChanged(entityContext.Context, false)).AddTo(_jetPackObserving);

			activeEquipment.TryGetActivePart(EquipmentPartType.JetPack, out JetPackContext jetPack);
			OnJetPackChanged(jetPack, jetPack != null);
		}

		private void OnJetPackChanged(EquipmentEntityContext context, bool isPutOn)
		{
			if(context == null)
			{
				Hide();
				return;
			}
			
			if (context is JetPackContext jetPack)
			{
				if (!isPutOn)
				{
					Hide();
					return;
				}
				
				Show();
				_fuelObserving?.Dispose();
				_fuelObserving = new CompositeDisposable();
				jetPack.Args.CurrentFuel.SubscribeWithState(jetPack.Args.MaxFuel, OnFuelChanged).AddTo(_fuelObserving);
				_fuelService.TanksCount.Subscribe(OnTankChanged).AddTo(_fuelObserving);
			}
		}

		private void Show()
		{
			View.Content.SetActive(true);
			SetButtonState(false);

		}

		private void Hide()
		{
			View.Content.SetActive(false);
			SetButtonState(false);
			_fuelObserving?.Dispose();
		}

		private void OnTankChanged(int tanks)
		{
			if (_maxFuel > 0 && _currentFuel <= _maxFuel * PERCENT_TO_SHOW && _fuelService.HasFuel())
			{
				SetButtonState(true);
				return;
			}
			SetButtonState(false);
		}
		
		private void OnFuelChanged(float fuel, float maxFuel)
		{
			_currentFuel = fuel;
			_maxFuel = maxFuel;
			var fuelToShow = Mathf.Clamp(Mathf.CeilToInt(_currentFuel), 0, _maxFuel);
			
			View.Fuel.text = string.Format(View.Format, fuelToShow, maxFuel);
			View.FilledImage.fillAmount = fuel / maxFuel;

			var threshold = maxFuel * PERCENT_TO_SHOW;
			if (fuel <= threshold && _fuelService.HasFuel())
				SetButtonState(true);
			else if(fuel > threshold)
				SetButtonState(false);
		}

		private void SetButtonState(bool state)
		{
			if (state && !View.RefuelButton.gameObject.activeInHierarchy)
				View.RefuelButton.gameObject.SetActive(true);
			else if(!state && View.RefuelButton.gameObject.activeInHierarchy)
				View.RefuelButton.gameObject.SetActive(false);
		}
		
		private void Refuel()
		{
			if (_playerContext.EquipmentInventory.Controller.ActiveEquipment.TryGetActivePart<JetPackContext>(EquipmentPartType.JetPack, out var jetPack))
			{
				var tank = _fuelService.GetTank();
				jetPack.AddFuel(tank.Fuel);
			}
		}
		
		public void Dispose()
		{
			_jetPackObserving?.Dispose();
			_fuelObserving?.Dispose();
		}
	}
}
