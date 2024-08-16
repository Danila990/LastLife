using System;
using System.Collections.Generic;
using Sirenix.Serialization;
using UnityEngine;

namespace Core.AnimationRigging
{
	public interface IRigProvider
	{
		//Transform AimTarget { get; }
		IReadOnlyDictionary<string, RigElementController> Rigs { get; }
	}
	
	[Serializable]
	public class RigProvider : IRigProvider, IDisposable
	{
		[OdinSerialize] private Dictionary<string, RigElementController> _rigs = new Dictionary<string, RigElementController>();

		public Transform AimTarget { get; }
		public IReadOnlyDictionary<string, RigElementController> Rigs => _rigs;
		
		public void Dispose()
		{
			foreach (var rg in _rigs.Values)
			{
				rg.Dispose();
			}
		}
	}

}