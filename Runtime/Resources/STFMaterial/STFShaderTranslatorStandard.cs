
using System;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace stf.serialisation
{

	public class STFShaderTranslatorStandard : ISTFShaderTranslator
	{
		public static readonly string _SHADER_NAME = "Standard";

		public Material TranslateSTFToUnity(ISTFImporter state, STFMaterial stfMaterial)
		{
			var ret = new Material(Shader.Find(_SHADER_NAME));
			foreach(var property in stfMaterial.Properties)
			{
				if(property.Name == "Albedo")
				{
					if(property.Type == "Texture")
					{
						state.AddTask(new Task(() => {
							ret.SetTexture("_MainTex", state.GetResource(property.Value));
						}));
					}
				}
			}
			return ret;
		}

		public STFMaterial TranslateUnityToSTF(ISTFExporter state, Material material)
		{
			// handle existing stf material
			var ret = ScriptableObject.CreateInstance<STFMaterial>();
			ret.name = material.name;
			ret.ShaderTargets.Add(new STFMaterial.ShaderTarget {target = "Unity", shaders = new List<string> {_SHADER_NAME}});

			/*for(int i = 0; i < material.shader.GetPropertyCount(); i++)
			{
				foreach(var attribute in material.shader.GetPropertyAttributes(i))
				{
					Debug.Log($"Property Index: {i}, Attribute: {attribute}");
				}
			}
			foreach(var name in material.GetTexturePropertyNames())
			{
				Debug.Log($"GetTexturePropertyNames: {name}");
			}*/



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