using System;
using Core.Services;
using Db.Quests;
using UniRx;
using UnityEngine;
using Utils;
using VContainer;

namespace Core.Actions.Impl
{
	[CreateAssetMenu(menuName = SoNames.ACTION_DATA + nameof(DeleteItemAction), fileName = nameof(DeleteItemAction))]
	public class DeleteItemAction : ItemEntityAction
	{
		[Inject] private readonly IDeleteItemService _deleteItemService;

		private IDisposable _disposable;
		
		public override void OnDeselect()
		{
			Dispose();
		}

		public override void OnInput(bool state)
		{
			if (!state)
				Dispose();
		}

		public override void OnInputUp()
		{
			Dispose();
			_deleteItemService.DeleteStatus.Value = false;	
		}
		
		public override void OnInputDown()
		{
			_deleteItemService.DeleteStatus.Value = true;
			DoProxyInput();
		}

		private void DoProxyInput()
		{
			_deleteItemService.RayCastDestroy();
			_disposable = Observable
				.IntervalFrame(10)
				.Subscribe(_ => _deleteItemService.RayCastDestroy());
		}

		public override void Dispose()
		{
			_disposable?.Dispose();
		}
	}
}