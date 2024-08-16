using System;
using Adv.Messages;
using Core.ResourcesSystem;
using Core.ResourcesSystem.Interfaces;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Ui.Widget;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;
using VContainer.Unity;
using VContainerUi.Abstraction;
using Object = UnityEngine.Object;

namespace Ui.Resource
{
	public class ResourceView : UiView
	{
		public Transform Content;
		public ResourceWidget ResourceWidgetPrefab;
	}

	public class ResourceUiController : UiController<ResourceView>, IStartable, IDisposable
	{
		private readonly IResourcesService _resourceService;
		private readonly IPublisher<ShowShopMenu> _openShopTickets;
		private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();
		
		public ResourceUiController(
			IResourcesService resourceService,
			IPublisher<ShowShopMenu> openShopTickets
			)
		{
			_resourceService = resourceService;
			_openShopTickets = openShopTickets;
		}
		
		public void Start()
		{
			View.GetComponentInChildren<Image>().OnPointerClickAsObservable().Subscribe(OnClick).AddTo(View.destroyCancellationToken);
			CreateResourceWidgetFor(ResourceType.Ticket);
			CreateResourceWidgetFor(ResourceType.GoldTicket);
			CreateResourceWidgetFor(ResourceType.Fuel);
		}
		
		private void OnClick(PointerEventData obj)
		{
			_openShopTickets.Publish(new ShowShopMenu("tickets", forceUseMenuPanel:false, canClose: false));
		}

		private ResourceWidget CreateResourceWidgetFor(ResourceType resourceType)
		{
			var observable = _resourceService.GetResourceObservable(resourceType);
			var widget = Object.Instantiate(View.ResourceWidgetPrefab, View.Content);
			observable.SubscribeToText(widget.CountText);
			widget.SetResource(resourceType);
			widget.SetCount(_resourceService.GetCurrentResourceCount(resourceType));
			widget.ReLayout(ResourceWidget.CountTextPosition.Left);
			return widget;
		}
		
		public void Dispose()
		{
			_compositeDisposable?.Dispose();
		}
	}
}