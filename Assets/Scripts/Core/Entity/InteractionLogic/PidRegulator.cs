using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Entity.InteractionLogic
{
    [Serializable]
    public class PidRegulator
    {
        [SerializeField] private float kP;
        [SerializeField] private float kI;
        [SerializeField] private float kD;

        [ShowInInspector] private float I;
        [ShowInInspector] private float prevErr;
		
		
        public PidRegulator(float kP, float kI, float kD)
        {
            this.kP = kP;
            this.kI = kI;
            this.kD = kD;
        }

        public float Regulate(float err, float deltaTime)
        {
            I += err * deltaTime;
            var D = (err - prevErr) / deltaTime;
            prevErr = err;
            return err * kP + I * kI + D * kD;
        }
    }
}