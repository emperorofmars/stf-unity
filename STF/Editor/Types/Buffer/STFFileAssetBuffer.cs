
#if UNITY_EDITOR

using System.IO;
using UnityEditor;
using UnityEngine;

namespace STF.Types
{
	[System.Serializable]
	public class STFFileAssetBuffer : ISTFBuffer
	{
		public Object FileAsset;

		public override byte[] GetData()
		{
			return File.ReadAllBytes(AssetDatabase.GetAssetPath(FileAsset));
		}
	}
}

#endif
