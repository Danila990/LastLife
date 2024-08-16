using Core.AnimationRigging;
using UniRx;
using UnityEngine;

namespace Core.Entity.Characters.Adapters
{
	public struct AimStatus
	{
		public AimState State;

		public AimStatus(AimState state)
		{
			State = state;
		}
	}

	public enum AimState
	{
		Default,
		Aim,
		Sniper
	}
	
	public class AimController : MonoBehaviour
	{
		private BaseCharacterAdapter _adapter;
		private RigElementController _aimRig;
		public IReadOnlyReactiveProperty<bool> IsAiming => _isAiming;
		public IReadOnlyReactiveProperty<AimStatus> TargetAim => _targetAiming;
		private BoolReactiveProperty _isAiming;
		private ReactiveProperty<AimStatus> _targetAiming;
		private AimState _currState;
		public AimState CurrState => _currState;
		
		public void Init(BaseCharacterAdapter baseCharacterAdapter)
		{
			_adapter = baseCharacterAdapter;
			_isAiming = new BoolReactiveProperty(false).AddTo(this);
			_targetAiming = new ReactiveProperty<AimStatus>(new AimStatus(AimState.Default)).AddTo(this);
			_adapter.ContextChanged.Subscribe(OnChange).AddTo(this);
		}
		
		private void OnChange(CharacterContext obj)
		{
			if (!obj)
			{
				return;
			}
			
			if(obj.RigProvider.Rigs is null) return;
			_aimRig = obj.RigProvider.Rigs["aim"];
			_aimRig.OnComplete += OnCompleteAim;
		}

		private void OnCompleteAim(bool obj)
		{
			_isAiming.Value = obj;
		}

		public void SetAimState(AimState state)
		{
			if(_aimRig is null) return; //TODO CHANGE
			_currState = state;
			_targetAiming.Value = new AimStatus(state);
			if (state != AimState.Default)
			{ 
				_aimRig.EnableRig();
			}
			else
			{
				_aimRig.DisableRig();
			}
		}
	}
}