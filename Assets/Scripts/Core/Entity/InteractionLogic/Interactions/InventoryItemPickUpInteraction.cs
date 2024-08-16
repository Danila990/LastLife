using Core.Entity.Characters;
using Core.Inventory.Items;
using Db.ObjectData.Impl;
using MessagePipe;
using UniRx;
using UnityEngine;

namespace Core.Entity.InteractionLogic.Interactions
{
    public class InventoryItemPickUpInteraction : ItemSupplyInteraction
    {
        [SerializeField] protected int _quantity;

        [SerializeField] private ThrowableEntity _entity;
        public InventoryObjectDataSo ObjectToGive;

        protected override void OnStart()
        {
            base.OnStart();
            _entity.OnDestroyCommand.Subscribe(_ => OnEntityDestroyed()).AddTo(_entity);
        }

        private void OnEntityDestroyed()
        {
            DisableUI();
        }
        
        public override void Use(EntityContext user)
        {
            if (user is CharacterContext characterContext)
            {
                Select(characterContext);
                PlaySound();

                if (_entity != null)
                {
                    _entity.OnDestroyed(_entity.EntityRepository);
                    Destroy(_entity.gameObject);
                }
            }
        }
        
        protected override void Select(CharacterContext characterContext)
        {
            if(_entity == null)
                return;
            
            var data = ObjectToGive.Model;
            characterContext.Inventory.AddItem(data, _quantity);
            characterContext.Inventory.TrySelectItem(data.Id);
        }
    }

}