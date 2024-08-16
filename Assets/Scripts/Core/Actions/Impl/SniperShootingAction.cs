using System;
using System.Threading;
using Core.Entity.Characters.Adapters;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Utils;

namespace Core.Actions.Impl
{
    [CreateAssetMenu(menuName = SoNames.ACTION_DATA + nameof(SniperShootingAction), fileName =  nameof(SniperShootingAction))]
    public class SniperShootingAction : ShootingAction
    {
        protected override UniTaskVoid WaitAiming(Action callback, CancellationToken token)
        {
            if (CharacterContext.CurrentAdapter is PlayerCharacterAdapter adapter)
            {
                if (adapter.AimController.CurrState == AimState.Default)
                {
                    adapter.SetAimState(AimState.Aim);
                } 
            }
            return base.WaitAiming(callback, token);
        }
    }
}