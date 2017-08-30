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
		protected class BasePersisting : IBasePersisting
		{
			internal string mId;
			internal Persistence mPersistence;

			Persistence IBasePersisting.persistence { get { return mPersistence; } }
			string IBasePersisting.id { get { return mId; } }

			protected void SetDirty()
			{
				mPersistence.isDirty = true;
			}
		}

		private string mFilepath;
		public bool isDirty { get; private set; }

		public static Persistence Load(string filepath)
		{
			Persistence obj = new Persistence { mFilepath = filepath };
			obj.DoInitialLoad();

			return obj;
		}

		public static Persistence Create(string filepath)
		{
			return new Persistence { mFilepath = filepath };
		}

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
