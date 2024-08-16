using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;
using VContainer.Unity;
using VContainerUi.Abstraction;

namespace Ui.Sandbox.Conversation
{
	public class ConversationView : UiView
	{
		public TextMeshProUGUI DialogText;
		public Button Close;
		public Button Submit;
	}

	public interface IConversationController
	{
		void Hide();
		void Show(string text, 
			UnityAction action, 
			UnityAction fallbackAction = null,
			bool closeOnSubmit = false);
	}
	
	public class ConversationController : UiController<ConversationView>, IStartable, IConversationController
	{
		private UnityAction _action;
		private UnityAction _fallbackAction;
		private bool _closeOnSubmit;

		public void Start()
		{
			View.Submit.onClick.AddListener(OnSubmit);
			View.Close.onClick.AddListener(Close);
		}
		
		protected virtual void Close()
		{
			View.gameObject.SetActive(false);
			_action = null;
            if (_fallbackAction == null) return;
            _fallbackAction();
            _fallbackAction = null;
            _closeOnSubmit = false;
		}

		private void OnSubmit()
		{
			_action();
			if (_closeOnSubmit)
			{
				Close();
			}
		}

        public void Hide()
        {
			View.gameObject.SetActive(false);
        }
        
        public override void OnShow()
        {
	        View.gameObject.SetActive(false);
        }
        
        public virtual void Show(string text, 
	        UnityAction action, 
	        UnityAction fallbackAction = null,
	        bool closeOnSubmit = false)
        {
	        OnTop();
			_closeOnSubmit = closeOnSubmit;
			if (!string.IsNullOrEmpty(text))
			{
				View.DialogText.text = text;
			}
			_action = action;
            if (fallbackAction != null)
            {
				_fallbackAction = fallbackAction;
            }
			View.gameObject.SetActive(true); 
        }

        public void OnTop()
        {
			View.transform.SetSiblingIndex(1000);
        }
	}
}