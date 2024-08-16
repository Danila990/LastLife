using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils;

namespace Core.Entity.EntityUpgrade
{

	public interface ICharacterUpgradesData
	{
		IReadOnlyCollection<CharacterUpgrades> Upgrades { get; }
		IReadOnlyCollection<CharacterUpgradeUIData> UiData { get; }
		bool TryGetUpgrade(string playerId, out CharacterUpgrades upgrade);
		CharacterUpgradeUIData GetUI(int level);
	}
	
	[CreateAssetMenu(menuName = SoNames.UPGRADES + "Repository", fileName = nameof(CharacterUpgradesData))]
	public class CharacterUpgradesData : ScriptableObject, ICharacterUpgradesData
	{
		[SerializeField] private List<CharacterUpgrades> _upgrades;
		[SerializeField] private List<CharacterUpgradeUIData> _uiData;
		[SerializeField] private CharacterUpgradeUIData _fallback;

		public IReadOnlyCollection<CharacterUpgrades> Upgrades => _upgrades;
		public IReadOnlyCollection<CharacterUpgradeUIData> UiData => _uiData;

		public bool TryGetUpgrade(string playerId, out CharacterUpgrades upgrade)
		{
			foreach (var x in _upgrades)
			{
				if (x.PlayerId.Equals(playerId, StringComparison.InvariantCultureIgnoreCase))
				{
					upgrade = x;
					return true;
				}
			}
			
			upgrade = null;
			return false;
		}
		
		public CharacterUpgradeUIData GetUI(int level)
		{
			for (var i = 0; i < _uiData.Count - 1; i++)
			{
				if (level >= _uiData[i].Level && level < _uiData[i+1].Level)
					return _uiData[i];
			}

			return _uiData.Last();
		}
	}
	
	[Serializable]
	public class CharacterUpgradeUIData
	{
		public int Level;
		public Color TextColor;
		[PreviewField]
		public Sprite Sprite;
	}
}
