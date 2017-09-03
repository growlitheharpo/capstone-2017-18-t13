using System.IO;
using UnityEngine;

namespace KeatsLib.Persistence
{
	/// <summary>
	/// Main section of the Persistence class.
	/// Handles saving and loading data between gameplay sessions.
	/// </summary>
	public partial class Persistence
	{
		/// <summary>
		/// Base implementation of the IBasePersisting interface.
		/// </summary>
		protected class BasePersisting : IBasePersisting
		{
			internal string mId;
			internal Persistence mPersistence;

			Persistence IBasePersisting.persistence { get { return mPersistence; } }
			string IBasePersisting.id { get { return mId; } }

			/// <summary>
			/// Notifies the Persistence instance that it needs to be saved.
			/// </summary>
			protected void SetDirty()
			{
				mPersistence.isDirty = true;
			}
		}

		private string mFilepath;

		/// <summary>
		/// Does this Persistence instance have data that needs to be saved?
		/// </summary>
		public bool isDirty { get; private set; }

		/// <summary>
		/// Load the Persistence from a file.
		/// </summary>
		/// <param name="filepath">The file to use. This is where we will also save to.</param>
		/// <returns>The loaded data instance.</returns>
		public static Persistence Load(string filepath)
		{
			Persistence obj = new Persistence { mFilepath = filepath };
			obj.DoInitialLoad();

			return obj;
		}

		/// <summary>
		/// Create a new instance of the persistent data.
		/// </summary>
		/// <param name="filepath">The file to save to later.</param>
		/// <returns>The created data instance.</returns>
		public static Persistence Create(string filepath)
		{
			return new Persistence { mFilepath = filepath };
		}

		/// <summary>
		/// If we're dirty, save to our file.
		/// </summary>
		public void Save()
		{
			//check if filepath is ""!!
		} // serialize this entire class.

		private void DoInitialLoad()
		{
			//Check if filepath is ""!!
			using (FileStream fileStream = File.OpenRead(Application.persistentDataPath + mFilepath)) { }
			//load the file from mFilepath
		}
	}
}
