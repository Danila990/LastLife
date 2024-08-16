using System;
using System.Collections.Generic;
using Core.Carry;
using Core.Entity.Characters;
using Core.InputSystem;
using Core.ResourcesSystem;
using MessagePipe;
using UniRx;
using VContainer.Unity;

namespace Market.OilRig
{
	public interface IRefineProvider
	{
		void AddEntity(RefinerFactoryContext factoryContext);
		void RemoveEntity(RefinerFactoryContext factoryContext);
		bool GetResourceType(ResourceType resourceType, out List<RefinerFactoryContext> refiners);
	}
	
	public class RefineProvider : IRefineProvider, IStartable, IDisposable
	{
		private readonly ISubscriber<PlayerContextChangedMessage> _contextSub;
		private readonly Dictionary<ResourceType, List<RefinerFactoryContext>> _refiners;

		private readonly CompositeDisposable _disposable;
		private CompositeDisposable _carryingObserving;
		private CharacterContext _currentContext;
		private CarriedContext _currentCarriedContext;

		public RefineProvider(ISubscriber<PlayerContextChangedMessage> contextSub)
		{
			_contextSub = contextSub;
			
			_disposable = new CompositeDisposable();
			_refiners = new Dictionary<ResourceType, List<RefinerFactoryContext>>();
		}
		
		public bool GetResourceType(ResourceType resourceType, out List<RefinerFactoryContext> refiners)
		{
			return _refiners.TryGetValue(resourceType, out refiners);
		}
		
		public void Start()
		{
			_contextSub.Subscribe(OnContextChanged).AddTo(_disposable);
		}

		private void OnContextChanged(PlayerContextChangedMessage msg)
		{
			_currentContext = msg.CharacterContext;
			_carryingObserving?.Dispose();
			NotifyAllRefiners(_currentContext.CarryInventory.CurrentCarried.Value, false);

			if (msg.Created)
			{
				_carryingObserving = new CompositeDisposable();
				_currentContext.CarryInventory.OnDrop.Subscribe(OnDrop).AddTo(_carryingObserving);
				_currentContext.CarryInventory.OnPickUp.Subscribe(OnPick).AddTo(_carryingObserving);
			}
		}

		private void OnDrop(CarriedContext context)
		{
			NotifyAllRefiners(context, false);
			_currentCarriedContext = null;
		}
		
		private void OnPick(CarriedContext context)
		{
			NotifyAllRefiners(context, true);
			_currentCarriedContext = context;
		}

		private void NotifyAllRefiners(CarriedContext context, bool isPickedUp)
		{
			if(_refiners == null || _refiners.Count == 0)
				return;

			if (context == null)
			{
				foreach (var refiners in _refiners.Values)
					foreach (var refiner in refiners)
						refiner.OnDrop();

				return;
			}
			
			if (_refiners.TryGetValue(context.ResourceType, out var collection))
			{
				foreach (var refiner in collection)
				{
					if (isPickedUp)
						refiner.OnPickUp();
					else
						refiner.OnDrop();
				}
			}
		}
		
		public void AddEntity(RefinerFactoryContext factoryContext)
		{
			var type = factoryContext.ResourceType;
			
			if (!_refiners.ContainsKey(type))
				_refiners[type] = new List<RefinerFactoryContext>();

			var collection = _refiners[type];

			if (!collection.Contains(factoryContext))
			{
				collection.Add(factoryContext);
				factoryContext.OnEndRefine.Subscribe(OnEndRefine).AddTo(_disposable);
			}
		}

		public void RemoveEntity(RefinerFactoryContext factoryContext)
		{
			if (_refiners.TryGetValue(factoryContext.ResourceType, out var contexts))
			{
				contexts.Remove(factoryContext);
			}
		}

		private void OnEndRefine(RefinerFactoryContext factoryContext)
		{
			if(!_currentCarriedContext || _currentCarriedContext.ResourceType != factoryContext.ResourceType)
				return;
			
			NotifyAllRefiners(_currentCarriedContext, true);
		}
		
		public void Dispose()
		{
			_disposable?.Dispose();
		}
	}



}
