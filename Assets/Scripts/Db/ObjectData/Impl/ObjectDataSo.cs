using Sirenix.OdinInspector;
using UnityEngine;

namespace Db.ObjectData.Impl
{

	public abstract class ObjectDataSo<T> : ScriptableObject, IObjectDataProvider
		where T : ObjectData
	{
		[field:SerializeField, InlineProperty, HideLabel] public T Model { get; private set; }
		public ObjectData ObjectData => Model;

		public virtual void EditorSet(T model)
		{
			Model.Ico = model.Ico;
			Model.Id = model.Id;
			Model.Name = model.Name;
			Model.IsUnlocked = model.IsUnlocked;
			Model.UnlockKey = model.UnlockKey;
		}
	}

	public interface IObjectDataProvider
	{
		ObjectData ObjectData { get; }
	}
}