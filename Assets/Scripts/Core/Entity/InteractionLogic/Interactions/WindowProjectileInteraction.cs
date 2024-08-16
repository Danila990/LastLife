using Core.HealthSystem;
using UnityEngine;

namespace Core.Entity.InteractionLogic.Interactions
{
    public class WindowProjectileInteraction : EnviromentProjectileInteraction
    {
        [SerializeField] private ParticleSystem _onDestroy;
        [SerializeField] private GameObject _onDestroyDisable;
        public override bool AcceptMelee => true;

        public override void OnHit(ref DamageArgs args, ref InteractionCallMeta meta)
        {
            _onDestroy.Play();
            _onDestroyDisable.SetActive(false);
        }
    }
}