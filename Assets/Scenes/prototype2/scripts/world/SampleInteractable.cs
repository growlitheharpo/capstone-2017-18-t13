using UnityEngine;

namespace FiringSquad.Gameplay
{
	public class SampleInteractable : MonoBehaviour, IInteractable
	{
		public void Interact()
		{
			Logger.Info("You clicked on the box!");
		}

		public void Interact(ICharacter source)
		{
			Logger.Info("You clicked on the box and gave a source!");
		}
	}
}
