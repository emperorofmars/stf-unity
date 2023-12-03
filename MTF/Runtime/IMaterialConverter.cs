
using System.Drawing;
using UnityEngine;

namespace MTF
{
	public interface IMaterialConvertState
	{
		void SaveResource(Object Resource, string Name);
		Texture2D SaveImageResource(byte[] Bytes, string Name, string Extension);
	}
	public interface IMaterialParseState
	{
		void SavePropertyValue(IPropertyValue PropertyValue, Material.Property Property, MTF.Material Material);
	}

	public interface IMaterialConverter
	{
		string ShaderName {get;}
		UnityEngine.Material ConvertToUnityMaterial(IMaterialConvertState State, Material MTFMaterial, UnityEngine.Material ExistingUnityMaterial = null);
	}

	public interface IMaterialParser
	{
		string ShaderName {get;}
		Material ParseFromUnityMaterial(IMaterialParseState State, UnityEngine.Material UnityMaterial, Material ExistingMTFMaterial = null);
	}
}
