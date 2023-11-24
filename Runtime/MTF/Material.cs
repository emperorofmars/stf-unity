using System;
using System.Collections.Generic;
using UnityEngine;

namespace MTF
{
	[CreateAssetMenu(fileName = "MTF Material", menuName = "MTF/Material", order = 1)]
	public class Material : ScriptableObject
	{
		[Serializable] public class ShaderTarget { public string Platform; public List<string> Shaders = new List<string>(); }
		[Serializable] public class Property { public string Type; public List<IPropertyValue> Values = new List<IPropertyValue>(); }

		public string Id = Guid.NewGuid().ToString();
		public UnityEngine.Material ConvertedMaterial;
		public List<ShaderTarget> PreferedShaderPerTarget = new List<ShaderTarget>();
		public List<string> StyleHints = new List<string>();
		public List<Property> Properties = new List<Property>();

		public static Material CreateDefaultMaterial()
		{
			var ret = CreateInstance<Material>();
			ret.PreferedShaderPerTarget.Add(new ShaderTarget{ Platform = "unity3d", Shaders = new List<string>{ "Standard" } });
			ret.Properties.Add(new Property { Type = "Albedo", Values = new List<IPropertyValue> { new ColorPropertyValue{ Color = Color.white } } } );
			return ret;
		}
	}
}
