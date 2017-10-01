using UnityEngine;

public class RotatorUtilityScript : MonoBehaviour
{
	[SerializeField] private float mRotationRate = 180.0f;

	private void Update()
	{
		transform.Rotate(Vector3.up, mRotationRate * Time.deltaTime);
	}
}
