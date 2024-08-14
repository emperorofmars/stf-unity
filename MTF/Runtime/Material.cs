using System;
using System.Collections.Generic;
using UnityEngine;
using STF_Util;
using MTF.PropertyValues;

namespace MTF
{
	[CreateAssetMenu(fileName = "MTF Material", menuName = "MTF/Material", order = 1)]
	public class Material : ScriptableObject
	{
		[Serializable] public class ShaderTarget { public string Platform; public List<string> Shaders = new(); }
		[Serializable] public class StyleHint { public string Name; public string Value; }
		[Serializable] public class Property { public string Type; public List<IPropertyValue> Values = new(); }

		[Id]
		public string Id = Guid.NewGuid().ToString();
		public string MaterialName;
		public UnityEngine.Material ConvertedMaterial;
		public List<ShaderTarget> PreferedShaderPerTarget = new();
		public List<StyleHint> StyleHints = new();
		public List<Property> Properties = new();

		public static Material CreateDefaultMaterial()
		{
			var ret = CreateInstance<Material>();
			ret.PreferedShaderPerTarget.Add(new ShaderTarget{ Platform = "unity3d", Shaders = new List<string>{ "Standard" } });
			var defaultColor = ScriptableObject.CreateInstance<ColorPropertyValue>();
			defaultColor.Color = Color.white;
			ret.Properties.Add(new Property { Type = "Albedo", Values = new List<IPropertyValue> { defaultColor } } );
			return ret;
		}
	}
}
