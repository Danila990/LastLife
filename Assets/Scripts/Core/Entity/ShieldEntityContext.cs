using System;
using LitMotion;
using UnityEngine;
using Utils.Constants;

namespace Core.Entity
{
	public class ShieldEntityContext : PhysicEntityContext
	{

		[SerializeField] private MeshRenderer _renderer;
		[SerializeField] private AnimationCurve _blinkCurve;
		private Func<(Vector3 Pos, Vector3 Rot)> _calc;
        
		public void SetCalc(Func<(Vector3 Pos, Vector3 Rot)> calcPosition)
		{
			_calc = calcPosition;
			
			if(_calc != null)
				ForcePlace();
		}

		public void PlayBlinking(float duration)
		{
			if(!_renderer)
				return;
			
			LMotion
				.Create(0f, 1f, duration)
				//.WithEase(_blinkCurve)
				.Bind(val => _renderer.material.SetFloat(ShHash.MaskPower, val))
				.ToUniTask(destroyCancellationToken);
		}
        
		public void Update()
		{
			if (_calc == null)
				return;
            
			Place();
		}
        
		private void Place()
		{
			var args = _calc();

			transform.position = Vector3.Lerp(transform.position, args.Pos, 200f * Time.deltaTime);
			transform.eulerAngles = args.Rot;
		}
        
		private void ForcePlace()
		{
			var args = _calc();

			transform.position = args.Pos;
			transform.eulerAngles = args.Rot;
		}
	}
}
