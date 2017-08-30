using System;
using UnityEngine;

public class ActionProvider : MonoBehaviour
{
	public event Action OnClick = () => { };

	public void Click()
	{
		OnClick();
	}
}
