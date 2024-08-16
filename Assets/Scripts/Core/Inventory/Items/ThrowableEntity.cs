using Core.Entity;
using UnityEngine;

namespace Core.Inventory.Items
{
    public class ThrowableEntity : EntityContext
    {
        [SerializeField] private Rigidbody _targetRigidBody;
        public EntityContext Owner;
        public Rigidbody TargetRigidbody => _targetRigidBody;
    }
}