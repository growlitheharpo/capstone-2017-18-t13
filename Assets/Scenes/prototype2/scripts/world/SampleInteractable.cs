using UnityEngine;

namespace FiringSquad.Gameplay
{
	public class SampleInteractable : MonoBehaviour, IInteractable
	{
		public void Interact()
		{
			Logger.Info("You clicked on the box!");
		}
	}
}
