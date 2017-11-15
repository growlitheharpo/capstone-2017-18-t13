using UnityEngine;

namespace KeatsLib.Unity
{
	/// <summary>
	/// Component to rotate an object at a constant rate.
	/// </summary>
	public class RotatorUtilityScript : MonoBehaviour
	{
		[SerializeField] private float mRotationRate = 180.0f;

		/// <summary>
		/// Unity's Update function
		/// Rotate the object.
		/// </summary>
		private void Update()
		{
			transform.Rotate(Vector3.up, mRotationRate * Time.deltaTime);
		}
	}
}
