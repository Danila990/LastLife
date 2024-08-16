using Core.Quests.Save;
using Core.Quests.Tree;
using Cysharp.Threading.Tasks;
using RemoteConfig;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using VContainer;

namespace Core.Quests.Misc
{
	public class QuestCompleteObserver : MonoBehaviour
	{
		[ValueDropdown("@QuestsUtils.GetAllQuests()")]
		[SerializeField] private string TreeId;
		[TitleGroup("Event")]
		[SerializeField] private UnityEvent _unityEvent;
		
		private readonly BoolReactiveProperty _onComplete = new ();
		public IReactiveProperty<bool> OnComplete => _onComplete;

		private IQuestSaveAdapter _questSaveAdapter;
		private IQuestService _questService;
		private IRemoteConfigAdapter _remoteConfigAdapter;

		[Inject]
		private void Construct(IObjectResolver resolver)
		{
			_questService = resolver.Resolve<IQuestService>();
			_questSaveAdapter = resolver.Resolve<IQuestSaveAdapter>();
			_remoteConfigAdapter = resolver.Resolve<IRemoteConfigAdapter>();
			
			Init();
		}
		
		private void Init()
		{
			_onComplete.AddTo(destroyCancellationToken);
			_questSaveAdapter.OnLoaded.Subscribe(OnLoaded).AddTo(destroyCancellationToken);
			
		}

		private void OnLoaded(bool isLoaded)
		{
			if (_questSaveAdapter.IsCompleteTree(TreeId))
			{
				Complete();
				return;
			}

			_questService.OnTreeComplete.Subscribe(OnTreeComplete).AddTo(destroyCancellationToken);
		}

		private void OnTreeComplete(QuestsTree tree)
		{
			if (tree.Id == TreeId)
				Complete();
		}
		
		private void Complete()
		{
			_onComplete.Value = true;
			_unityEvent?.Invoke();
			Destroy(this);
		}
	}
}
