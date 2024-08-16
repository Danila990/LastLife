using System;

namespace Editor.Windows
{
	public abstract class BasePageEditor
	{
		protected bool DataExist { get; set; } = false;
		public event Action OnValueSaved;
		
		public virtual void Init()
		{

		}

		public virtual void OnDispose()
		{
			
		}
		
		public virtual void Save()
		{
			OnValueSaved?.Invoke();
		}
	}
}