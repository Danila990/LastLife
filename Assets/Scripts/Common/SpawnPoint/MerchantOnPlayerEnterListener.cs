using System;
using Core.Entity.Ai.Merchant;
using Core.Entity.InteractionLogic.Interactions;
using NodeCanvas.Framework;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;

namespace Common.SpawnPoint
{
	public class MerchantOnPlayerEnterListener : MonoBehaviour
	{
		[SerializeField, HorizontalGroup("shopPoint")] private string _shopPointKey; 
		[SerializeField, HorizontalGroup("shopPoint")] private GameObject _shopPoint;

		[SerializeField] private Transform _cameraPoint;
		[SerializeField] private Vector3 _position;
		[SerializeField] private Vector3 _rotation;
		
		[SerializeField] private PlayerTriggerVisiter _triggerVisiter;
		[SerializeField] private MerchantSpawnPoint _merchantSpawnPoint;
		[ShowInInspector, ReadOnly] public MerchantEntityContext MerchantEntity { get; set; }
		[SerializeField] private string _setKey;
		private Variable<bool> _variable;
		private IDisposable _disposable;

		private void Awake()
		{
			_disposable = _merchantSpawnPoint.MerchantCreated.Subscribe(OnObjectCreated);
		}
		private void Start()
		{
			_triggerVisiter.CurrentPlayer.Subscribe(OnPlayerMoved).AddTo(this);
		}
		
		private void OnPlayerMoved(PlayerTriggerVisiter.PlayerTriggerInteractionData obj)
		{
			MerchantEntity.Blackboard.SetVariableValue("PlayerLookAtTarget", obj.PlayerCharacterAdapter.CurrentContext.LookAtTransform.gameObject);
			MerchantEntity.Blackboard.SetVariableValue(_shopPointKey, _shopPoint);

			if (obj.IsEnter)
			{
				MerchantEntity.NpcAnimator.TrackObject(obj.PlayerCharacterAdapter.CurrentContext.LookAtTransform);
				if (_cameraPoint)
				{
					_cameraPoint.localPosition = _position;
					_cameraPoint.localEulerAngles = _rotation;
				}
			}
			else
			{
				MerchantEntity.NpcAnimator.StopTrack();
				
			}

			MerchantEntity.Blackboard.SetVariableValue(_setKey, obj.IsEnter);
		}

		public void OnObjectCreated(MerchantEntityContext entityContext)
		{
			_disposable?.Dispose();
			_disposable = null;
			MerchantEntity = entityContext;
		}
	}
}