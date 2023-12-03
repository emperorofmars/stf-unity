using System;
using System.Collections.Generic;
using UnityEngine;
using STF_Util;
using UnityExtensions;

namespace MTF
{
	[CreateAssetMenu(fileName = "MTF Material", menuName = "MTF/Material", order = 1)]
	public class Material : ScriptableObject
	{
		[Serializable] public class ShaderTarget { public string Platform; public List<string> Shaders = new List<string>(); }
		[Serializable] public class StyleHint { public string Name; public string Value; }
		[Serializable] public class Property { public string Type; [ReorderableList(elementsAreSubassets = true)] public List<IPropertyValue> Values = new List<IPropertyValue>(); }

		[Id]
		public string Id = Guid.NewGuid().ToString();
		public string MaterialName;
		public UnityEngine.Material ConvertedMaterial;
		[ReorderableList] public List<ShaderTarget> PreferedShaderPerTarget = new List<ShaderTarget>();
		[ReorderableList] public List<StyleHint> StyleHints = new List<StyleHint>();
		[ReorderableList] public List<Property> Properties = new List<Property>();

		public static Material CreateDefaultMaterial()
		{
			var ret = CreateInstance<Material>();
			ret.PreferedShaderPerTarget.Add(new ShaderTarget{ Platform = "unity3d", Shaders = new List<string>{ "Standard" } });
			ret.Properties.Add(new Property { Type = "Albedo", Values = new List<IPropertyValue> { new ColorPropertyValue{ Color = Color.white } } } );
			return ret;
		}
	}
}
