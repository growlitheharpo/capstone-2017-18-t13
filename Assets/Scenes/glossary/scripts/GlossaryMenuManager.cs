using FiringSquad.Core;
using FiringSquad.Core.State;
using FiringSquad.Core.Weapons;
using FiringSquad.Gameplay.Weapons;
using FiringSquad.Data;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FiringSquad.Gameplay.UI
{
	/// <summary>
	/// Gun Glossary UI manager.
	/// </summary>
	public class GlossaryMenuManager : MonoBehaviour
	{
		/// Inspector variables
		[SerializeField] private GameObject mMainElementHolder;
		[SerializeField] private GameObject mScrollRectContent;
		[SerializeField] private ActionProvider mReturnToMainButton;
		[SerializeField] private GameObject mButtonPrefab;
		[SerializeField] private UnityEngine.UI.Text mDescriptionText;
		[SerializeField] private GameObject mPartShowcase;

		/// Var for the number of parts
		private int mPartCount;

		/// <summary>
		/// Unity's Start function
		/// </summary>
		void Start()
		{
			mReturnToMainButton.OnClick += ReturnToMenu;
			// Get the number of weapon parts for list purposes
			mPartCount = ServiceLocator.Get<IWeaponPartManager>()
				.GetAllPrefabs(false).Count;

			// Set the content rectangle to be the size of the list
			mScrollRectContent.GetComponent<RectTransform>().sizeDelta 
				= new Vector2(mScrollRectContent.GetComponent<RectTransform>().sizeDelta.x, 
				mScrollRectContent.GetComponent<RectTransform>().sizeDelta.y + (100 * (mPartCount - 7)));

			// Populate the list and hope for the best
			PopulateList();

			mPartShowcase.SetActive(true);
		}

		/// <summary>
		/// Unity's update function
		/// </summary>
		private void Update()
		{
			// Slowly rotate the weapon around
			mPartShowcase.transform.Rotate(0, Time.deltaTime * 35, 0, Space.World);
		}

		/// <summary>
		/// Return to the main menu
		/// </summary>
		private void ReturnToMenu()
		{
			mMainElementHolder.SetActive(false);

			ServiceLocator.Get<IGamestateManager>()
				.RequestSceneChange(GamestateManager.MENU_SCENE);
		}

		/// <summary>
		/// Populates the list with weapon part buttons
		/// </summary>
		private void PopulateList()
		{
			// Iterator for moving the buttons down
			int i = 1;

			foreach(System.Collections.Generic.KeyValuePair<byte, GameObject> item in ServiceLocator.Get<IWeaponPartManager>().GetAllPrefabs(false))
			{
				// Instantiate the button for the weapon part
				GameObject tmpButton = Instantiate(mButtonPrefab, mScrollRectContent.transform);

				// Set the text of the button
				tmpButton.GetComponentInChildren<UnityEngine.UI.Text>().text
					= item.Value.GetComponent<WeaponPartScript>().prettyName;

				// Get the description of the item
				string itemDesc = item.Value.GetComponent<WeaponPartScript>().description;

				// Set local position to a new location
				tmpButton.transform.localPosition = new Vector3(tmpButton.transform.localPosition.x, -(100 * i), tmpButton.transform.localPosition.z);

				// Add a the changetext function to the onclick
				tmpButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate { ChangeText(itemDesc);});
				tmpButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate { ChangePart(item.Value); });

				++i;
			}
		}

		/// <summary>
		/// Changes the text of the description field
		/// </summary>
		private void ChangeText(string newText)
		{
			mDescriptionText.text = newText;
		}

		/// <summary>
		/// Changes the current attached part to the model gun
		/// </summary>
		private void ChangePart(GameObject item)
		{
			mPartShowcase.GetComponent<BaseWeaponScript>().AttachNewPart(item.GetComponent<WeaponPartScript>().partId);
		}
	}
}
