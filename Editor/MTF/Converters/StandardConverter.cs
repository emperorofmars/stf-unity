using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MTF
{
	public class StandardConverter : IMaterialConverter
	{
		public static string _SHADER_NAME = "Standard";
		public string ShaderName {get => _SHADER_NAME;}
		public UnityEngine.Material ConvertToUnityMaterial(Material MTFMaterial, UnityEngine.Material ExistingUnityMaterial = null)
		{
			var ret = ExistingUnityMaterial != null ? ExistingUnityMaterial : new UnityEngine.Material(Shader.Find(_SHADER_NAME));
			foreach(var property in MTFMaterial.Properties)
			{
				if(property.Type == "albedo")
				{
					var parsed = false;
					foreach(var value in property.Values)
					{
						if(value.Type == "MTF.texture")
						{
							ret.SetTexture("_MainTex", ((TexturePropertyValue)value).Texture);
							parsed = true;
							//break;
						}
						else if(value.Type == "MTF.color")
						{
							ret.SetColor("_Color", ((ColorPropertyValue)value).Color);
							parsed = true;
							//break;
						}
					}
					if(parsed == false) Debug.LogWarning("Unknown Material Property Skipped: " + property.Type);
				}
			}
			return ret;
		}
	}

	public class MaterialParser : IMaterialParser
	{
		public string ShaderName {get => StandardConverter._SHADER_NAME;}
		public Material ParseFromUnityMaterial(UnityEngine.Material UnityMaterial, Material ExistingMTFMaterial = null)
		{
			var ret = ExistingMTFMaterial != null ? ExistingMTFMaterial : ScriptableObject.CreateInstance<Material>();
			if(UnityMaterial.GetTexture("_MainTex") != null)
			{
				var property = ret.Properties.FirstOrDefault(p => p.Type == "albedo");
				if(property == null) property = new Material.Property();
				property.Values.Add(new TexturePropertyValue {Texture = (Texture2D)UnityMaterial.GetTexture("_MainTex")});
			}
			if(UnityMaterial.GetTexture("_Color") != null)
			{
				var property = ret.Properties.FirstOrDefault(p => p.Type == "albedo");
				if(property == null) property = new Material.Property();
				property.Values.Add(new TexturePropertyValue {Texture = (Texture2D)UnityMaterial.GetTexture("_Color")});
			}
			return ret;
		}
	}
}