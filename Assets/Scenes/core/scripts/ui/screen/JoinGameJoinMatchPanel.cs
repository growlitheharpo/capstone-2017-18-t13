using System.Collections;
using System.Collections.Generic;
using FiringSquad.Core;
using UnityEngine;
using UnityEngine.UI;

public class JoinGameJoinMatchPanel : MonoBehaviour {

	/// Inspector variables
	[SerializeField] private Button mCancelButton01;
	[SerializeField] private Button mCancelButton02;

	/// Private variables
	private bool panelActive = false;

	private void OnEnable()
	{
		mCancelButton01.onClick.AddListener(OnClickCancelButton);
		mCancelButton02.onClick.AddListener(OnClickCancelButton);
		panelActive = true;
	}

	private void OnDisable()
	{
		mCancelButton01.onClick.RemoveListener(OnClickCancelButton);
		mCancelButton02.onClick.RemoveListener(OnClickCancelButton);
		panelActive = false;
	}

	private void OnClickCancelButton()
	{
		if (panelActive == true)
		{
			gameObject.SetActive(false);
		}
	}
}
