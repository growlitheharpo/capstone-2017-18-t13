namespace FiringSquad.Core.UI
{
	public interface IGameplayUIManager
	{
		BoundProperty<T> GetProperty<T>(int hash);

		void BindProperty(int hash, BoundProperty prop);
		void UnbindProperty(BoundProperty prop);
	}
}
