
namespace FiringSquad.Gameplay
{
	public interface IInteractable
	{
		void Interact();
		void Interact(ICharacter source);
	}
}
