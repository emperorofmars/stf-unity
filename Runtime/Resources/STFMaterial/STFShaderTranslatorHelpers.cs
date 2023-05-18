
using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace stf.serialisation
{

	public class STFShaderTranslatorHelpers
	{
		public static void ExportTexture(ISTFExporter state, Material material, STFMaterial stfMaterial, string unityName, string stfName)
		{
			if(material.GetTexture(unityName) != null)
			{
				Texture texture = material.GetTexture(unityName);
				state.RegisterResource(texture);
				stfMaterial.Properties.Add(new STFMaterial.ShaderProperty {
					Name = stfName,
					Type = "texture",
					Value = state.GetResourceId(texture)
				});
			}
		}

		public static void ExportTextureChannel(ISTFExporter state, Material material, STFMaterial stfMaterial, string unityName, int channel, string stfName, bool invert = false)
		{
			if(material.GetTexture(unityName) != null)
			{
				Texture texture = material.GetTexture(unityName);
				state.RegisterResource(texture);
				var textureView = new JObject();
				textureView.Add("type", "STF.texture_view");
				textureView.Add("texture", state.GetResourceId(texture));
				textureView.Add("channel", channel);
				textureView.Add("invert", invert);
				var textureViewId = Guid.NewGuid().ToString();
				state.RegisterResource(textureViewId, textureView);
				stfMaterial.Properties.Add(new STFMaterial.ShaderProperty {
					Name = stfName,
					Type = "texture",
					Value = textureViewId
				});
			}
		}

		public static void ImportTexture(ISTFImporter state, Material material, STFMaterial stfMaterial, string unityName, string stfName)
		{
			var property = stfMaterial.Properties.Find(p => p.Name == stfName);
			if(property != null && material.GetTexturePropertyNames().Contains(unityName))
			{
				state.AddTask(new Task(() => {
					material.SetTexture(unityName, state.GetResource(property.Value));
				}));
			}
		}

		public static void ExportStandardProperties(ISTFExporter state, Material material, STFMaterial stfMaterial)
		{
			ExportTexture(state, material, stfMaterial, "_MainTex", "albedo");
			ExportTexture(state, material, stfMaterial, "_BumpMap", "normal");
		}
	}
}