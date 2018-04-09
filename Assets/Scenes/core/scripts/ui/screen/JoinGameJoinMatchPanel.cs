using UnityEngine;
using UnityEngine.UI;

public class JoinGameJoinMatchPanel : MonoBehaviour {

	/// Inspector variables
	[SerializeField] private Button mCancelButton01;
	[SerializeField] private Button mCancelButton02;

	/// Private variables
	private bool mPanelActive;

	private void OnEnable()
	{
		mCancelButton01.onClick.AddListener(OnClickCancelButton);
		mCancelButton02.onClick.AddListener(OnClickCancelButton);
		mPanelActive = true;
	}

	private void OnDisable()
	{
		mCancelButton01.onClick.RemoveListener(OnClickCancelButton);
		mCancelButton02.onClick.RemoveListener(OnClickCancelButton);
		mPanelActive = false;
	}

	private void OnClickCancelButton()
	{
		if (mPanelActive)
			gameObject.SetActive(false);
	}
}
