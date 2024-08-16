using Core.Entity.Characters;
using Core.HealthSystem;
using UnityEngine;
using Utils;

namespace Core.Entity.InteractionLogic.Interactions
{
    [CreateAssetMenu(menuName = SoNames.INTERACTION_DATA + nameof(GlobalCharacterDamageInteraction),fileName = nameof(GlobalCharacterDamageInteraction))]
    public class GlobalCharacterDamageInteraction : AbstractSoInteraction
    {
        private CharacterContext _characterContext;

        public override void SetCharContext(CharacterContext context)
        {
            _characterContext = context;
        }
        
        public override InteractionResultMeta Visit(IInteractorVisiter visiter, ref InteractionCallMeta meta)
        {
            return visiter.Accept(this, ref meta);
        }

        // public void Hit(ref DamageArgs args)
        // {
        //     _characterContext.DoDamage(ref args, );
        // }
        
        public void HandleExplosion(ref DamageArgs args, Vector3 metaPoint, Vector3 normal, Vector3 direction)
        {
            _characterContext.DoMassDamage(ref args);
        }
    }
}