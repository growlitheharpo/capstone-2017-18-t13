using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

namespace KeatsLib.Unity
{
	/// <summary>
	/// A powerful utility script that binds private members in external classes to a value in an XML file.
	/// </summary>
	public class XmlBinder : MonoBehaviour
	{
		/// <summary>
		/// A helper class that binds a property of any type (including value types) so they can be updated.
		/// Includes implicit conversion operators (in both directions) with type T for ease-of-use.
		/// </summary>
		/// <typeparam name="T">The type of the variable. Generally float.</typeparam>
		public class Field<T>
		{
			/// <summary>The value stored in this reference.</summary>
			public T v { get; set; }

			/// <summary>A constructor for a reference given a base starting value.</summary>
			/// <param name="a">The value to initialize this Field with.</param>
			public Field(T a)
			{
				v = a;
			}

			/// <summary>Implicitly convert a Field to its base type for calculations.</summary>
			/// <param name="a"></param>
			public static implicit operator T(Field<T> a)
			{
				return a.v;
			}

			/// <summary>Implicitly convert an item of our base type into a Field.</summary>
			/// <param name="a"></param>
			public static implicit operator Field<T>(T a)
			{
				return new Field<T>(a);
			}
		}

		private static XmlBinder kInstance;

		///<summary>Our collection of data fields taken from the XML file.</summary>
		private Dictionary<string, float> mData;

		/// <summary>A collection of Field listeners stored by the key they are listening for.</summary>
		private Dictionary<string, HashSet<Field<float>>> mListeningFields;

		[SerializeField] private string mXmlFilename;

		public Dictionary<string, float> dataFields { get { return mData; } }

		/// <summary>Initialize our collections and attempt our first XML read.</summary>
		private void Awake()
		{
			if (kInstance == null)
				kInstance = this;
			else
				Destroy(this);

			mListeningFields = new Dictionary<string, HashSet<Field<float>>>();
			mData = new Dictionary<string, float>();

			RefreshData();
		}

		private void Start()
		{
			EventManager.OnInitiateXmlRefresh += RefreshData;
		}

		private void OnDestroy()
		{
			EventManager.OnInitiateXmlRefresh -= RefreshData;
		}

		/// <summary>Initailizes a Field so that it can be dynamically updated at runtime.</summary>
		/// <param name="token">The string value of the XML token that the value of this Field will be found to.</param>
		/// <param name="field">The Field that will be bound to this field. Should be removed when the field is no longer relevant.</param>
		/// <returns>Returns the Field that was passed in, updated to contain an initial value and bound to its token.</returns>
		public static Field<float> Bind(string token, ref Field<float> field)
		{
			return kInstance.BindInternal(token, ref field);
		}

		/// <summary>Initailizes a Field so that it can be dynamically updated at runtime.</summary>
		/// <param name="token">The string value of the XML token that the value of this Field will be found to.</param>
		/// <param name="field">The Field that will be bound to this field. Should be removed when the field is no longer relevant.</param>
		/// <returns>Returns the Field that was passed in, updated to contain an initial value and bound to its token.</returns>
		private Field<float> BindInternal(string token, ref Field<float> field)
		{
			if (!mListeningFields.ContainsKey(token))
				mListeningFields.Add(token, new HashSet<Field<float>>());

			if (!mData.ContainsKey(token))
				Logger.Warn("Could not find token: " + token);

			if (mListeningFields[token].Contains(field))
				return field;

			field = new Field<float>(mData[token]);
			mListeningFields[token].Add(field);

			return field;
		}

		/// <summary>
		/// Remove a Field field so that we do not store more than necessary in memory.
		/// <para>Refs are stored in a hash set, so removal time is essentially trivial.</para>
		/// </summary>
		/// <param name="field">The Field to be removed from its token.</param>
		public static void RemoveListenerField(ref Field<float> field)
		{
			foreach (var hashSet in kInstance.mListeningFields.Values)
				hashSet.Remove(field);

			field = null;
		}

		/// <summary>
		/// Perform an XML refresh.
		/// <para>This function loads the XML document from disk and loops through it, storing all of the data that it finds.</para>
		/// <para>After completing the load, it calls updateLinkedFields().</para>
		/// <para>
		/// If DEVELOPMENT_BUILD or UNITY_EDITOR are defined, this function will also display a message on screen
		/// indicating a successful reload.
		/// </para>
		/// </summary>
		private void RefreshData()
		{
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.Load(Application.streamingAssetsPath + "/" + mXmlFilename);

			XmlNode root = xmlDoc.FirstChild;
			XmlNode child = root.FirstChild;

			while (child != null)
			{
				if (child.Attributes != null)
				{
					foreach (XmlAttribute attrib in child.Attributes)
						mData[child.Name + "/" + attrib.Name] = float.Parse(attrib.Value);
				}

				child = child.NextSibling;
			}

			UpdateLinkedFields();
			EventManager.Notify(EventManager.XmlSuccesfullyRefreshed);

#if DEVELOPMENT_BUILD || UNITY_EDITOR
			StartCoroutine(ShowSuccessText());
#endif
		}

		/// <summary>Loop through all of our Field values that we maintain and update them to the new value found in the XML file.</summary>
		private void UpdateLinkedFields()
		{
			foreach (var pair in mListeningFields)
			{
				foreach (var field in pair.Value)
					field.v = mData[pair.Key];
			}
		}

#if DEVELOPMENT_BUILD || UNITY_EDITOR
		private bool mShowSuccess;

		/// <summary>Show a small sucess message on screen using the old OnGUI method.</summary>
		private IEnumerator ShowSuccessText()
		{
			float timer = 0.0f;
			mShowSuccess = true;

			while (timer < 2.0f)
			{
				timer += Time.deltaTime;
				yield return new WaitForEndOfFrame();
			}

			mShowSuccess = false;
		}

		private void OnGUI()
		{
			if (mShowSuccess)
			{
				GUI.Label(
					new Rect(0.05f * Screen.width, 0.45f * Screen.width, 0.25f * Screen.height, 0.45f * Screen.height),
					new GUIContent("XML Refresh successful!"));
			}
		}
#endif
	}
}
