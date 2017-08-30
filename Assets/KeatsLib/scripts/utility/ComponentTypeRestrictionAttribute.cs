using UnityEngine;

[System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false)]
public class ComponentTypeRestrictionAttribute : PropertyAttribute
{
	public System.Type mInheritsFromType;
	public bool mHideTypeDropDown;

	public ComponentTypeRestrictionAttribute(System.Type inheritsFromType)
	{
		this.mInheritsFromType = inheritsFromType;
	}
}
