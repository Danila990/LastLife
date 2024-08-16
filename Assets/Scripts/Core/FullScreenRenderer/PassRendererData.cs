using UnityEngine;
using UnityEngine.Rendering.Universal;
using Utils;

namespace Core.FullScreenRenderer
{
	[CreateAssetMenu(menuName = SoNames.FULLSCREEN_MATERIALS + nameof(PassRendererData), fileName = nameof(PassRendererData))]
	public class PassRendererData : ScriptableObject, IPassRendererData
	{
		[SerializeField] private UniversalRendererData _rendererData;
		
		public UniversalRendererData Data => _rendererData;
	}

	public interface IPassRendererData
	{
		public UniversalRendererData Data { get; }
	}
}
