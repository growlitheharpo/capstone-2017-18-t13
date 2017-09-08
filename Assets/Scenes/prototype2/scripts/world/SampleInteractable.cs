using UnityEngine;

namespace Prototype2
{
	public class SampleInteractable : MonoBehaviour, IInteractable
	{
		public void Interact()
		{
			Debug.Log("You clicked on the box!!");
		}
	}
}
