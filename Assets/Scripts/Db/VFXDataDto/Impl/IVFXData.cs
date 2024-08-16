using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace Db.VFXDataDto.Impl
{
	public interface IVFXData
	{
		IList<ParticleData> ParticleData { get; }
	}

	[Serializable]
	public struct ParticleData
	{
		public string Name;
		[VerticalGroup("Settings"), LabelText("Max"), LabelWidth(30), PropertyRange(0, 100)] public int MaxCount;
		[VerticalGroup("Settings"), LabelText("Preload"), LabelWidth(30), PropertyRange(0, 100)] public int PreloadCount;
		[OnValueChanged("OnSetParticle")]public VFXContext Particle;
		
		public override string ToString()
		{
			return Name;
		}

		public void OnSetParticle()
		{
			if (Particle != null && string.IsNullOrEmpty(Name))
			{
				Name = Particle.name;
			}
		}
	}
}