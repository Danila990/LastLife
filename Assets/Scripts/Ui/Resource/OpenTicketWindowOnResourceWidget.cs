using Core.ResourcesSystem;
using Ui.Widget;
using UnityEngine;
using UnityEngine.UI;

namespace Ui.Resource
{
	public class OpenTicketWindowOnResourceWidget : MonoBehaviour
	{
		public Image Plus;
		private ResourceWidget _widget;

		private void Start() 
		{
			_widget = GetComponent<ResourceWidget>();
			if (_widget.ResourceType == ResourceType.Ticket)
			{
				Instantiate(Plus, _widget.ResourceIcon.transform);
			}
		}
	}
}