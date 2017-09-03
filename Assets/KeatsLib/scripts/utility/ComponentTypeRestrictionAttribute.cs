using UnityEngine;

/// <summary>
/// Utility Attribute. Shows an error in the inspector if a component is not an instance of a given type.
/// </summary>
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
