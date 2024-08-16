using Core.Actions;
using Core.Entity.Characters;
using UnityEngine;

namespace Ui.Sandbox.PlayerInput
{
	public abstract class InputButtonElement : MonoBehaviour
	{
		public ActionKey ButtonKey;
		public abstract void OnContextChanged(CharacterContext characterContext);
		public abstract void SetUp(IEntityActionController entityActionData);
		public abstract void Hide();
		public abstract void Show();
		public virtual void Init() { }
		public abstract void Disable();
	}
}