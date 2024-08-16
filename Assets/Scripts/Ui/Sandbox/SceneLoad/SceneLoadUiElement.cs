using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ui.Sandbox.SceneLoad
{
	public class SceneLoadUiElement : MonoBehaviour
	{
		public TextMeshProUGUI SceneTxt;
		public Image Image;
		public Button LoadButton;
		public Button TicketButton;
		
		public string SceneId { get; private set; } 
		
		public void Init(SceneData viewSceneLoadVariant)
		{
			SceneTxt.text = viewSceneLoadVariant.SceneTextName;
			SceneId = viewSceneLoadVariant.SceneId;
			Image.sprite = viewSceneLoadVariant.SceneIcon;
		}
	}
}