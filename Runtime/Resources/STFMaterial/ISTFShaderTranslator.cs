
using System;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System.Collections.Generic;

namespace stf.serialisation
{
	public static class STFShaderRegistry
	{
		public static Dictionary<string, ISTFShaderTranslator> Converters = new Dictionary<string, ISTFShaderTranslator> {
			{"Standard", new STFShaderTranslatorStandard()}
		};
	}

	public class STFMaterial : ScriptableObject
	{
		[Serializable]
		public class ShaderProperty
		{
			public string Name;
			public string Type;
			public List<string> Targets;
			public dynamic Value;

			public string SerializeValue()
			{
				if(Value.GetType() == typeof(String))
				{
					return Value;
				}
				throw new Exception($"Unknown ShaderProperty Value: {Value.GetType()}");
			}

			public void ParseJsonValue(JToken json)
			{
				if(Type == "Texture")
				{
					Value = json.ToString();
				}
			}
		}
		public List<string> ShaderTargets = new List<string>();
		public List<ShaderProperty> Properties = new List<ShaderProperty>();
	}

	public interface ISTFShaderTranslator
	{
		Material TranslateSTFToUnity(ISTFImporter state, STFMaterial stfMaterial);
		STFMaterial TranslateUnityToSTF(ISTFExporter state, Material material);
	}
}