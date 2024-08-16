using Core.Entity.Characters.Adapters;
using Core.Entity.InteractionLogic.Interactions;
using Core.Services;
using Installer;
using UnityEngine;
using VContainer;

namespace Common
{
    public class ReturnTeleportCollision : MonoBehaviour, IInjectableTag
    {
        [Inject] private readonly ISpawnPointProvider _spawnPointProvider;
        
        private void OnTriggerEnter(Collider col)
        {
            if (col.TryGetComponent(out CharacterPartDamagable part))
            {
                var point = _spawnPointProvider.GetSafeSpawnPoint().position + Vector3.up * 2;
                if (!part.IsConnected)
                {
                    part.transform.position = point;
                    return;
                }
                if (part.CharacterContext.CurrentAdapter is PlayerCharacterAdapter playerCharacterAdapter)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        
                        playerCharacterAdapter.Rigidbody.velocity = Vector3.zero;
                        playerCharacterAdapter.Rigidbody.position = point;
                        playerCharacterAdapter.CurrentContext.MainTransform.position = point;
                    }
                    return;
                }
                else if (part.CharacterContext.CurrentAdapter is AiCharacterAdapter aiCharacterAdapter)
                {
                    aiCharacterAdapter.NavMeshAgent.transform.position = point;
                    foreach (var puppetMasterMuscle in part.CharacterContext.PuppetMaster.muscles)
                    {
                        puppetMasterMuscle.rigidbody.position = point;
                        puppetMasterMuscle.rigidbody.velocity = Vector3.zero;
                    }
                }
                return;
            }
            
            if (col.transform.root.TryGetComponent(out BaseHeadAdapter _))
            {
                return;
            }

            if (col.transform.TryGetComponent(out Rigidbody rb))
            {
                rb.position = Vector3.up;
                rb.velocity = Vector3.zero;
                return;
            }
            
            if (col.TryGetComponent(out MonoInteractProvider interactProvider) && interactProvider.Context)
            {
                interactProvider.Context.MainTransform.position = Vector3.up;
                
                if (interactProvider.Context.MainTransform.TryGetComponent(out rb))
                {
                    rb.position = Vector3.up;
                    rb.velocity = Vector3.zero;
                }
                return;
            }
        }
        
#if UNITY_EDITOR
        private Collider _collider;
        
        private void OnDrawGizmosSelected()
        {
            if (!_collider)
                _collider = GetComponent<BoxCollider>();
            
            Gizmos.color = Color.blue;
            var bounds = _collider.bounds;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }
#endif
    }
}
