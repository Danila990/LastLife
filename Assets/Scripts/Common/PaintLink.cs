using DG.Tweening;
using PaintIn3D;
using UnityEngine;
using Utils;
namespace Common
{
	public class PaintLink : Singleton<PaintLink>
	{
		[SerializeField] private P3dPaintDecal _decal;
		[SerializeField] private float _delay = 1f;
		//public PaintData PaintData;
		private bool _isActive;
		private Tween _tween;
		private Color _defaultColor;
		private bool _isEnabled;

		private void OnEnable()
		{
			//_isEnabled = _settingsManager.GetValue<bool>(SettingsConsts.MAIN_SETTING, SettingsConsts.PAINT_3D_ENABLED, GameSetting.ParameterType.Bool);
			_isEnabled = true;
			if (!_isEnabled)
			{
				return;
			}
			
			_tween = DOVirtual.DelayedCall(_delay, Callback).SetAutoKill(false);
			_tween.Pause();
			_defaultColor = _decal.Color;
			// if (PaintData.Enabled)
			// {
			// 	ChangeColor(true);
			// }
		}
		
		public void ChangeColor(bool isGreen)
		{
			if (isGreen)
			{
				//_decal.Color = PaintData.Color;
				_decal.Modifiers.Instances.RemoveAll(modifier => modifier is P3dModifyColorRandom);
			}
			else
			{
				_decal.Color = _defaultColor;
			}
		}

        public void ChangeColor(Color color)
        {
            _decal.Color = color;
        }

        private void Callback()
		{
			_isActive = false;
		}

		private void OnDisable()
		{
			_tween.Kill();
		}

		public void HandleDecal(Vector3 position, Quaternion rotation)
		{
			if (_isActive || !_isEnabled)
				return;
			HandleInternal(ref position, ref rotation);
		}

		private void HandleInternal(ref Vector3 position, ref Quaternion rotation)
		{
			_isActive = true;
			const int priority = 0;
			const float pressure = 1.0f;
			const int seed = 0;
			_decal.HandleHitPoint(false, priority, pressure, seed, position, rotation);
			_tween.Restart();
		}
	}
}