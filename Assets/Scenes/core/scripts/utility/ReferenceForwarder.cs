using System;
using UnityEngine;

public class ReferenceForwarder : MonoBehaviour
{
	[Serializable]
	public class References
	{
		[SerializeField] private GameObject mPlayerRef;
		public GameObject player { get { return mPlayerRef; } }
	}

	[SerializeField] private References mReferences;
	public static References get { get { return instance.mReferences; } }

	private static ReferenceForwarder instance { get; set; }

	private void Awake()
	{
		if (instance == null)
			instance = this;
		else
			Destroy(this);
	}
}
