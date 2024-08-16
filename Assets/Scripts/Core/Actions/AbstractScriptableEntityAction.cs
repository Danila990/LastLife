using System;
using System.Collections.Generic;
using Core.Entity;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Actions
{
	public abstract class AbstractScriptableEntityAction : ScriptableObject, IEntityAction
	{
		[field:NonSerialized] public EntityContext CurrentContext { get; private set; }
		protected readonly Dictionary<uint, InnerSettings> _settings = new Dictionary<uint, InnerSettings>();
		protected IReadOnlyDictionary<uint, InnerSettings> Settings => _settings;
		[field:SerializeField] public string Id { get; set; }
		
		public abstract void OnDeselect();
		public abstract void OnInput(bool state);
		public abstract void OnInputUp();
		public abstract void OnInputDown();
		
		public virtual void SetContext(EntityContext context)
		{
			_settings.TryAdd(context.Uid, new InnerSettings());
			CurrentContext = context;
		}
		
		/// <summary>
		/// Called before awake
		/// </summary>
		public virtual void Initialize() { }
		public virtual void Dispose() { }
		
#if UNITY_EDITOR
		[Button]
		[DisableIf("@ScriptableActionsData.EditorInstance.Contains(this)")]
		public void AddSelf()
		{
			ScriptableActionsData.EditorInstance.AddActionEditor(this);
		}
		
		[Button]
		public void BindId()
		{
			Id = GetType().Name;
		}
#endif		
		
		protected class InnerSettings
		{
			public bool LastInput;
		}
	}
}