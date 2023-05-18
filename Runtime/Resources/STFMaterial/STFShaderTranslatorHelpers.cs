
using System.Linq;
using System.Threading.Tasks;
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
	}
}