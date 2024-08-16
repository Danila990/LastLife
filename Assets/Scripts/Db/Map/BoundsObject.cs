using SharedUtils;
using UnityEngine;

namespace Db.Map
{
	public abstract class BoundsObject : MonoBehaviour
	{
		[SerializeField] private Vector3 _size;
		[SerializeField] private Vector3 _offset;
		
		private Vector3 _checkPosition;
		public Vector3 Size => _size;
		public Vector3 Offset => _offset;
		
		public bool InBounds(Vector3 position)
		{
			_checkPosition = position;
			var halfSize = _size / 2f;
			var minBounds = transform.position + _offset - halfSize;
			var maxBounds = transform.position + _offset + halfSize;

			return position.x >= minBounds.x && position.x <= maxBounds.x &&
			       position.y >= minBounds.y && position.y <= maxBounds.y &&
			       position.z >= minBounds.z && position.z <= maxBounds.z;
		}

		public Vector3 GetRandomPointInside()
		{
			var halfSize = _size / 2f;
			var minBounds = transform.position + _offset - halfSize;
			var maxBounds = transform.position + _offset + halfSize;
			var x = Random.Range(minBounds.x, maxBounds.x);
			var y = Random.Range(minBounds.y, maxBounds.y);
			var z = Random.Range(minBounds.z, maxBounds.z);
			return new Vector3(x, y, z);
		}
		
#if UNITY_EDITOR
			private void OnDrawGizmos()
			{
				/*var halfSize = _size / 2f;
				var minBounds = transform.position + _offset - halfSize;
				var maxBounds = transform.position + _offset + halfSize;
				Util.DrawSphere(minBounds, Quaternion.identity, 0.2f, Color.green);
				Util.DrawSphere(maxBounds, Quaternion.identity, 0.2f, Color.green);
				if(_checkPosition != Vector3.zero)
					Util.DrawSphere(_checkPosition, Quaternion.identity, 0.2f, Color.green);*/
				Gizmos.color = Color.green;
				Gizmos.DrawWireCube(transform.position + _offset, _size);
			}

			private void OnValidate()
			{
				_size.x = Mathf.Abs(_size.x);
				_size.y = Mathf.Abs(_size.y);
				_size.z = Mathf.Abs(_size.z);
			}
#endif
	}

}
