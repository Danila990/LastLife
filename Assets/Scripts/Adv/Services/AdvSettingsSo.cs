using UnityEngine;

namespace Adv.Services
{
	public class AdvSettingsSo : ScriptableObject, IAdvSettings
	{
		[field:SerializeField] public float InterTimer { get; set; }
	}

	public interface IAdvSettings
	{
		float InterTimer { get; }
	}
}