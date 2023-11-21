using System;
using System.Collections.Generic;
using UnityEngine;

namespace MTF
{
	public class StandardConverter : IMaterialConverter
	{
		public static string _SHADER_NAME = "Standard";
		public string ShaderName {get => _SHADER_NAME;}
		public UnityEngine.Material ConvertToUnityMaterial(Material MTFMaterial, UnityEngine.Material ExistingUnityMaterial)
		{
			return null;
		}
	}

	public class MaterialParser : IMaterialParser
	{
		public string ShaderName {get => StandardConverter._SHADER_NAME;}
		public Material ParseFromUnityMaterial(UnityEngine.Material UnityMaterial, Material ExistingMTFMaterial)
		{
			return null;
		}
	}
}