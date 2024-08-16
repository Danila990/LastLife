using Core.Inventory.Items.Weapon;
using UnityEngine;

namespace Core.Entity.InteractionLogic.Interactions
{
    public class SnapDragInteraction : DragInteraction
    {
        [SerializeField] private SnapObject _snap;
        
        public override void OnBeginDrag(Vector3 refPoint)
        {
            base.OnBeginDrag(refPoint);
            _snap.Detach();
        }

        public override void OnEndDrag()
        {
            base.OnEndDrag();
            _snap.AllowAttach();
        }
    }
}