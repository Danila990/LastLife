using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Ui.Sandbox.CharacterMenu
{
	public class MainCharacterPreview : MonoBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler
	{
		[SerializeField] private RawImage _rawImage;
		
		private ReactiveProperty<Vector2> _dragPosition;
		private ReactiveCommand<Vector2> _onBeginDrag;
		private ReactiveCommand<Vector2> _onEndDrag;
		
		public IReactiveProperty<Vector2> DragPosition => _dragPosition;
		public ReactiveCommand<Vector2> OnDragBegin => _onBeginDrag;
		public ReactiveCommand<Vector2> OnDragEnd => _onEndDrag;
		public RawImage RawImage => _rawImage;

		public void Init()
		{
			_dragPosition = new ReactiveProperty<Vector2>().AddTo(this);
			_onBeginDrag = new ReactiveCommand<Vector2>().AddTo(this);
			_onEndDrag = new ReactiveCommand<Vector2>().AddTo(this);
		}
		
		void IDragHandler.OnDrag(PointerEventData eventData)
		{
			_dragPosition.Value = eventData.position;
		}
		void IEndDragHandler.OnEndDrag(PointerEventData eventData)
		{
			_onEndDrag?.Execute(eventData.position);
		}
		void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
		{
			_onBeginDrag?.Execute(eventData.position);

		}
	}
}
