using LitMotion;
using LitMotion.Extensions;
using UnityEngine;
using Utils.Constants;
using VContainer;

namespace Core.Inventory.Items.Weapon
{
	public class SpiderGranadaEntity : ExplosionEntity
	{
		[SerializeField] private Renderer _renderer;
		[SerializeField, ColorUsage(false, true)] private Color end;
		[SerializeField] private AnimationCurve _ease;
		
		private MotionHandle _handle;

		protected override void OnCreated(IObjectResolver resolver)
		{
			base.OnCreated(resolver);

			Material material;
			var color = (material = _renderer.material).GetColor(ShHash.EmissionColor);
			_handle = LMotion.Create(color, end, _explosionDelay)
			//.WithEase(_ease)
			.BindToMaterialColor(material, ShHash.EmissionColor);
		}

		private void OnDisable()
		{
			if(_handle.IsActive())
				_handle.Cancel();
		}
	}
}
