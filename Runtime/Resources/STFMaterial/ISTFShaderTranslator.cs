
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

	public interface ISTFShaderTranslator
	{
		Material TranslateSTFToUnity(ISTFImporter state, STFMaterial stfMaterial);
		STFMaterial TranslateUnityToSTF(ISTFExporter state, Material material);
	}
}