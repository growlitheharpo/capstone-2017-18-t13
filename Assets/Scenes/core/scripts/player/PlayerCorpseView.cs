using KeatsLib.Unity;
using UnityEngine;

namespace FiringSquad.Gameplay
{
	/// <summary>
	/// Component for handling the visualization of each player's corpse
	/// </summary>
	public class PlayerCorpseView : MonoBehaviour
	{
		/// Inspector variables
		[SerializeField] private float mLingerTime = 5.0f;

		/// <summary>
		/// Unity's start function
		/// </summary>
		private void Start()
		{
			StartCoroutine(Coroutines.InvokeAfterSeconds(mLingerTime, Cleanup));
		}

		/// <summary>
		/// Cleanup this object after our specified amount of time has passed
		/// </summary>
		private void Cleanup()
		{
			Destroy(gameObject);
		}

		/// <summary>
		/// Update the color used by this corpse to match the team of its owner.
		/// </summary>
		public void UpdateColor(Color c)
		{
			var updaters = GetComponentsInChildren<ColormaskUpdateUtility>();
			foreach (ColormaskUpdateUtility u in updaters)
				u.UpdateDisplayedColor(c);
		}
	}
}
