﻿using UnityEngine;
using UIText = UnityEngine.UI.Text;

namespace FiringSquad.Gameplay.UI
{
	public class BoundIntField : BoundUIElement<int>
	{
		[SerializeField] private string mDisplayFormat;
		private UIText mTextElement;

		protected override void Awake()
		{
			base.Awake();
			mTextElement = GetComponent<UIText>();
		}

		protected override void HandlePropertyChanged()
		{
			mTextElement.text = property.value.ToString(mDisplayFormat);
		}
	}
}
