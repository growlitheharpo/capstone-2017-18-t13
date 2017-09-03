using System.Collections.Generic;
using UnityEngine;

namespace KeatsLib.Persistence
{
	/// <inheritdoc />
	public partial class Persistence
	{
		[SerializeField] private Dictionary<string, IOptionsData> mIdOptionsMap = new Dictionary<string, IOptionsData>();

		/// <summary>
		/// Create an instance of the OptionsData.
		/// </summary>
		public IOptionsData CreateOptionsData(string id)
		{
			OptionsDataImpl optionsData = new OptionsDataImpl { mPersistence = this, mId = id };
			mIdOptionsMap[id] = optionsData;

			return optionsData;
		}

		/// <summary>
		/// Get an instance of the OptionsData. Optionally create it if it does not exist yet.
		/// </summary>
		public IOptionsData GetOptionsData(string id, bool doNotCreate = true)
		{
			IOptionsData result;
			if (mIdOptionsMap.TryGetValue(id, out result))
				return result;

			return doNotCreate ? null : CreateOptionsData(id);
		}

		/// <summary>
		/// Delete an instance of the OptionsData.
		/// </summary>
		public void DeleteOptionsData(string id)
		{
			mIdOptionsMap.Remove(id);
		}
	}
}
