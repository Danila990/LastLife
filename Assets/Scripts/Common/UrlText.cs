using UnityEngine;
using UnityEngine.EventSystems;

namespace Common
{
	public class UrlText : MonoBehaviour, IPointerClickHandler
	{
		[SerializeField] private string _url;
		
		public void OnPointerClick(PointerEventData eventData)
		{
			Application.OpenURL(_url);
		}
	}
}