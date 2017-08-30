using System.Collections.Generic;
using UnityEngine;

namespace KeatsLib.Persistence
{
	public partial class Persistence
	{
		[SerializeField] private Dictionary<string, IOptionsData> mIdOptionsMap = new Dictionary<string, IOptionsData>();

		public IOptionsData CreateOptionsData(string id)
		{
			OptionsDataImpl optionsData = new OptionsDataImpl { mPersistence = this, mId = id };
			mIdOptionsMap[id] = optionsData;

			return optionsData;
		}

		public IOptionsData GetOptionsData(string id, bool doNotCreate = true)
		{
			IOptionsData result;
			if (mIdOptionsMap.TryGetValue(id, out result))
				return result;

			return doNotCreate ? null : CreateOptionsData(id);
		}

		public void DeleteOptionsData(string id)
		{
			mIdOptionsMap.Remove(id);
		}
	}
}
