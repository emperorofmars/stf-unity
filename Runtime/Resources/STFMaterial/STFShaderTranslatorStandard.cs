
using System;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace stf.serialisation
{

	public class STFShaderTranslatorStandard : ISTFShaderTranslator
	{
		public Material TranslateSTFToUnity(ISTFImporter state, STFMaterial stfMaterial)
		{
			var ret = new Material(Shader.Find("Standard"));
			foreach(var property in stfMaterial.Properties)
			{
				if(property.Name == "Albedo")
				{
					if(property.Type == "Texture")
					{
						state.AddTask(new Task(() => {
							Debug.Log($"state.GetResource {property.Value}");
							ret.SetTexture("_MainTex", state.GetResource(property.Value));
						}));
					}
				}
			}
			return ret;
		}

		public STFMaterial TranslateUnityToSTF(ISTFExporter state, Material material)
		{
			var ret = ScriptableObject.CreateInstance<STFMaterial>();
			ret.name = material.name;
			ret.ShaderTargets.Add(new STFMaterial.ShaderTarget {target = "Unity", shaders = new List<string> {"Standard"}});
			for(int i = 0; i < material.shader.GetPropertyCount(); i++)
			{
				foreach(var attribute in material.shader.GetPropertyAttributes(i))
				{
					Debug.Log($"Property Index: {i}, Attribute: {attribute}");
				}
			}
			foreach(var name in material.GetTexturePropertyNames())
			{
				Debug.Log($"GetTexturePropertyNames: {name}");
			}



			if(material.GetTexture("_MainTex") != null)
			{
				Texture mainTex = material.GetTexture("_MainTex");
				Debug.Log($"Main Tex Type: {mainTex.GetType()}");
				state.RegisterResource(mainTex);
				ret.Properties.Add(new STFMaterial.ShaderProperty {
					Name = "Albedo",
					Type = "Texture",
					Value = state.GetResourceId(mainTex)
				});
			}
			return ret;
		}
	}
}