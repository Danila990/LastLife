using System;
using Core.Entity;
using Core.Entity.Characters;

namespace Core.HealthSystem
{
    public struct DamageArgs
    {
        public EntityContext DamageSource;
        public float Damage;
        public float BloodLossAmount;
        public float BloodLossTime;
        public float KnockOut;
        public float HitForce;
        public float Unpin;
        public float DismemberDamage;
        public DamageType DamageType;
        public IDamageSource MetaDamageSource;

        public DamageArgs(EntityContext damageSource, float damage = 0f, float bloodLossAmount = 0f,
            float bloodLossTime = 0f, float knockOut = 0f, float hitForce = 0f, float unpin = 0f,float dismemberDamage = 0f,
            DamageType damageType = DamageType.Generic)
        {
            DamageSource = damageSource;
            BloodLossTime = bloodLossTime;
            Damage = damage;
            BloodLossAmount = bloodLossAmount;
            KnockOut = knockOut;
            HitForce = hitForce;
            Unpin = unpin;
            DismemberDamage = dismemberDamage;
            DamageType = damageType;
            MetaDamageSource = null;
        }
    }

    [Serializable]
    public struct SerializedDamageArgs
    {
        public float Damage;
        public float BloodLossAmount;
        public float BloodLossTime;
        public float KnockOut;
        public float HitForce;
        public float Unpin;
        public float DismemberDamage;
        public DamageType DamageType;
        public DamageArgs GetArgs(EntityContext context)
        {
            return new DamageArgs(context, Damage, BloodLossAmount, BloodLossTime, KnockOut, HitForce, Unpin, DismemberDamage, DamageType);
        }
    }
}