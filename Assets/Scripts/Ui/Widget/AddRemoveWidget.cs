using System;
using UnityEngine;
using UnityEngine.UI;

namespace Ui.Widget
{
	public class AddRemoveWidget : MonoBehaviour
	{
		public event Action<bool> AddOrRemoveEvent;
		public Button AddBTN;
		public Button RemoveBTN;

		public void Start()
		{
			AddBTN.onClick.AddListener(() => AddOrRemoveEvent?.Invoke(true));
			RemoveBTN.onClick.AddListener(() => AddOrRemoveEvent?.Invoke(false));
		}
	}
}