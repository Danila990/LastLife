using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MiniGames.Impl.SpinnerMiniGame
{
	public class SpinnerUi : MonoBehaviour, IEndDragHandler, IDragHandler
	{
		[SerializeField] private float ANGLE_STEP = 30f;
		private RectTransform _rectTransform;
		private int _targetValue;
		private Vector2 _lastAngle = Vector2.up;
		private readonly ReactiveProperty<int> _value = new ReactiveProperty<int>();
		public IReactiveProperty<int> Value => _value;
		public Vector3 Center => _rectTransform.position;
		
		private void Start()
		{
			_rectTransform = GetComponent<RectTransform>();
			_value.AddTo(this);
		}
		
		public void ResetSpinner()
		{
			_value.Value = 0;
			_lastAngle = Vector2.up;
			transform.rotation = Quaternion.identity;
		}

		public void OnDrag(PointerEventData eventData)
		{
			var delta = new Vector2(eventData.delta.x / Screen.width, eventData.delta.y / Screen.width);
			var pos = new Vector2(eventData.position.x , eventData.position.y);
			OnDragInput(delta, pos);
		}

		public void OnDrag(Vector2 pos)
		{
			var position = new Vector2(pos.x , pos.y);
			OnDragInput(Vector2.zero, position);
		}

		private void OnDragInput(Vector2 delta, Vector2 pos)
		{
			var posInScreen = _rectTransform.position;
			var currentMousePosition = pos;
			var directionToCurrentMousePos = new Vector2(posInScreen.x,posInScreen.y) - currentMousePosition;
			var currentAngle = Vector2.SignedAngle(directionToCurrentMousePos.normalized, _lastAngle);
			
			if (currentAngle > ANGLE_STEP)
			{
				_value.Value++;
				_lastAngle = directionToCurrentMousePos.normalized;
				transform.Rotate(0,0,-1f);
			}
		}
		
		public void OnEndDrag(PointerEventData eventData)
		{
		}
	}
}