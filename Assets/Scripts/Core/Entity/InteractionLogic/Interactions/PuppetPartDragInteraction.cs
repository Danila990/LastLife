using Core.Entity.Characters;
using UnityEngine;

namespace Core.Entity.InteractionLogic.Interactions
{
    public class PuppetPartDragInteraction : DragInteraction, ICharacterContextAcceptor
    {
        public CharacterContext CharacterContext;

        public override void OnBeginDrag(Vector3 refPoint)
        {
            CharacterContext.RagDollManager.EnableRagDoll();
            base.OnBeginDrag(refPoint);
        }

        public override void OnEndDrag()
        {
            CharacterContext.RagDollManager.DisableRagDoll();
            base.OnEndDrag();
        }

        public override void DisableOutline()
        {
            CharacterContext.CharacterOutline.Disable();
        }

        public override void EnableOutline()
        {
            CharacterContext.CharacterOutline.Enable();
        }

        protected override void InitOutline()
        {
        }

        public void SetContext(CharacterContext context)
        {
            CharacterContext = context;
        }
    }
}