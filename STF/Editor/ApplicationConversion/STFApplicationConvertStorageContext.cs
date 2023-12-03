using System.IO;
using UnityEditor;
using UnityEngine;

namespace STF.ApplicationConversion
{
	public class STFApplicationConvertStorageContext : ISTFApplicationConvertStorageContext
	{
		public string _TargetPath;
		public string TargetPath => _TargetPath;

		public STFApplicationConvertStorageContext(string TargetPath)
		{
			_TargetPath = TargetPath;
		}

		public void SaveGeneratedResource(Object Resource, string FileExtension)
		{
			if(!FileExtension.StartsWith(".")) FileExtension = "." + FileExtension;
			AssetDatabase.CreateAsset(Resource, Path.Combine(TargetPath, Resource.name + FileExtension));
		}
	}
}