
using System.Drawing;
using UnityEngine;

namespace MTF
{
	public interface IMaterialConvertState
	{
		Bitmap LoadTextureAsBitmap(Texture2D Texture);
		Texture2D SaveBitmapTexture(Bitmap Bitmap, string Name);
	}
	public interface IMaterialParseState
	{
		
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
