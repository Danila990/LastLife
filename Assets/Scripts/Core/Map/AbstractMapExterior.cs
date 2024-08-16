using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.Map
{
	public abstract class AbstractMapExterior : MonoBehaviour
	{
		public abstract void CreateObjects();
		public abstract UniTask CreateObjectsAsync();
	}
}