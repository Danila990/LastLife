using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Db.ObjectData
{
	[Serializable]
	public abstract class ObjectData : IDataModel
	{
		[field:SerializeField, VerticalGroup("Id")] public string Id { get; set; }
		[VerticalGroup("Id")] public string Name;
		
		[VerticalGroup("Id")] public string UnlockKey;
		[VerticalGroup("Id")] public bool IsUnlocked;
		
		[TableColumnWidth(100), PreviewField(Alignment = ObjectFieldAlignment.Center), VerticalGroup("Ico"), HideLabel, PropertyOrder(20)] 
		public Sprite Ico;
	}
}