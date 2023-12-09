#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace STF_Util
{
	[CustomPropertyDrawer(typeof(AssetListAttribute))]
	public class AssetListAttributeDrawer : PropertyDrawer
	{
		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			var assetListAttribute = attribute as AssetListAttribute;
			var container = new VisualElement();

			var list = new ListView();
			container.Add(list);
			list.BindProperty(property);
			list.showAddRemoveFooter = false;

			return container;
		}
	}
}

#endif
