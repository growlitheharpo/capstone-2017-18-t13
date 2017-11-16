namespace FiringSquad.Gameplay
{
	/// <summary>
	/// Interface for any object in the world that the player can interact with directly.
	/// </summary>
	public interface IInteractable
	{
		/// <summary>
		/// Interact with this object.
		/// </summary>
		/// <param name="source">The character doing the interaction.</param>
		void Interact(ICharacter source);
	}
}
