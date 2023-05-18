
using System;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System.Collections.Generic;

namespace stf.serialisation
{
	public static class STFShaderRegistry
	{
		public static Dictionary<string, ISTFShaderTranslator> Converters = new Dictionary<string, ISTFShaderTranslator> {
			{STFShaderTranslatorStandard._SHADER_NAME, new STFShaderTranslatorStandard()}
		};
	}

	public class STFMaterial : ScriptableObject
	{
		
		[Serializable]
		public class ShaderTarget
		{
			public string target;
			public List<string> shaders;
		}

		[Serializable]
		public class ShaderProperty
		{
			public string Name;
			public string Type;
			public List<ShaderTarget> Targets;
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
		public List<ShaderTarget> ShaderTargets = new List<ShaderTarget>();
		public List<ShaderProperty> Properties = new List<ShaderProperty>();
	}

	public interface ISTFShaderTranslator
	{
		Material TranslateSTFToUnity(ISTFImporter state, STFMaterial stfMaterial);
		STFMaterial TranslateUnityToSTF(ISTFExporter state, Material material);
	}
}