using System;
using UniRx;
using UniRx.Triggers;
using UnityEngine.EventSystems;

namespace Utils
{
	public class ObservableDragTriggers : ObservableTriggerBase, IBeginDragHandler, IDragHandler, IEndDragHandler
	{
		private Subject<PointerEventData> _onDrag;
		private Subject<PointerEventData> _onEndDrag;
		private Subject<PointerEventData> _onBeginDrag;

		
		void IBeginDragHandler.OnBeginDrag(PointerEventData eventData) 
			=> _onBeginDrag?.OnNext(eventData);

		void IDragHandler.OnDrag(PointerEventData eventData) 
			=> _onDrag?.OnNext(eventData);

		void IEndDragHandler.OnEndDrag(PointerEventData eventData) 
			=> _onEndDrag?.OnNext(eventData);
		

		public IObservable<PointerEventData> OnDragAsObservable() 
			=> _onDrag ??= new Subject<PointerEventData>();

		public IObservable<PointerEventData> OnEndDragAsObservable() 
			=> _onEndDrag ??= new Subject<PointerEventData>();
        
		public IObservable<PointerEventData> OnBeginDragAsObservable() 
			=> _onBeginDrag ??= new Subject<PointerEventData>();

		protected override void RaiseOnCompletedOnDestroy()
		{
			_onBeginDrag?.OnCompleted();
			_onDrag?.OnCompleted();
			_onEndDrag?.OnCompleted();
		}
	}
}