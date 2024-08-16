using Core.Equipment.Inventory;
using Core.Services;
using Db.Render;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Core.Render
{
	public interface IRendererFactory
	{
		public CharacterRenderer CreateRenderer(string characterId);
		RendererHolder GetRendererHolder();
	}
	
	public class RendererFactory : IRendererFactory, IInitializable
	{
		private readonly IRendererStorage _rendererStorage;
		private readonly IObjectResolver _resolver;
		private readonly IRendererData _rendererData;
		private readonly IItemUnlockService _itemUnlockService;
		private readonly IItemStorage _itemStorage;

		private RendererHolder _holder;
		private Vector3 _offset;

		public RendererFactory(
			IRendererStorage rendererStorage, 
			IObjectResolver resolver,
			IRendererData rendererData,
			IItemUnlockService itemUnlockService,
			IItemStorage itemStorage)
		{
			_rendererStorage = rendererStorage;
			_resolver = resolver;
			_rendererData = rendererData;
			_itemUnlockService = itemUnlockService;
			_itemStorage = itemStorage;
		}

		public CharacterRenderer CreateRenderer(string characterId)
		{
			if (!_rendererStorage.CharacterRenderers.TryGetValue(characterId, out var prefab))
			{
				Debug.LogError($"The renderers storage doesn't contain rendered with id ({characterId})");
				return null;
			}

			var instance = Object.Instantiate(prefab, _holder.CharactersHolder);
			_resolver.Inject(instance);
			if (_itemStorage.All.TryGetValue(characterId, out var value))
			{
				instance.SetIsUnlocked(_itemUnlockService.IsUnlocked(value));
			}
			
			
			instance.Init(_resolver);
			return instance;
		}
		
		public RendererHolder GetRendererHolder()
		{
			return _holder;
		}

		public void Initialize()
		{
			_holder = Object.Instantiate(_rendererData.RendererPrefab);
			
		}
	}
}
