using System;
using Core.Entity.Ai.Merchant;
using Dialogue.Services.Interfaces;
using UniRx;
using UnityEngine;

namespace Common.SpawnPoint
{
	public class MerchantShopItemPointManager : MonoBehaviour
	{
		[SerializeField] private MerchantSpawnPoint _merchantSpawnPoint;
		[SerializeField] private MerchantPlaceInfo _merchantPlaceInfo;
		[SerializeField] private Transform[] _spawnPoints;
		private IDisposable _disposable;
		
		private int _index;
		
		private void Awake()
		{
			_disposable = _merchantSpawnPoint.MerchantCreated.Subscribe(OnObjectCreated);
		}

		private void OnDrawGizmos()
		{
			foreach (var spawnPoint in _spawnPoints)
			{
				Gizmos.matrix = spawnPoint.localToWorldMatrix;
				Gizmos.DrawCube(Vector3.zero, (Vector3.one - spawnPoint.localScale) + Vector3.one / 2);
			}
		}

		private void OnObjectCreated(MerchantEntityContext context)
		{
			_disposable?.Dispose();
			_disposable = null;
			
			if (context.TryGetComponent(out ShopDialogueModuleArgs args))
			{
				args.ShopItemPointManager = this;
				args.MerchantPlaceInfo = _merchantPlaceInfo;
				if (_merchantPlaceInfo)
				{
					foreach (var obj in _merchantPlaceInfo.Objects)
					{
						obj.ConnectedContext(context);
					}
				}
			}
		}
		
		public Vector3 GetSpawnedItemPosition()
		{
			if (++_index >= _spawnPoints.Length)
				_index = 0;
			
			return _spawnPoints[_index].position;
		}
	}
}