
#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.IO;
using STF.Util;
using STF.Types;

namespace STF.Serialisation
{
	public class EditorUnityImportContext : IUnityImportContext
	{
		public bool IsDegraded => false;
		string TargetLocation;

		public EditorUnityImportContext(string TargetLocation)
		{
			this.TargetLocation = TargetLocation;
			EnsureFolderStructure(TargetLocation);
		}
		
		private void EnsureFolderStructure(string TargetLocation)
		{
			var existingEntries = Directory.EnumerateFileSystemEntries(TargetLocation); foreach(var entry in existingEntries)
			{
				if(File.Exists(entry)) File.Delete(entry);
				else Directory.Delete(entry, true);
			}
			AssetDatabase.Refresh();
			AssetDatabase.CreateFolder(TargetLocation, STFConstants.ResourceDirectoryName);
			AssetDatabase.CreateFolder(TargetLocation, STFConstants.PreservedBuffersDirectoryName);
			AssetDatabase.Refresh();
		}

		public Object SaveResource(ISTFResource Resource)
		{
			var location = Path.Combine(TargetLocation, STFConstants.ResourceDirectoryName, Resource.name + ".Asset");
			AssetDatabase.CreateAsset(Resource, location);
			AssetDatabase.Refresh();
			return Resource;
		}
		public Object SaveSubResource(Object SubResource, Object Resource)
		{
			var assetPath = AssetDatabase.GetAssetPath(Resource);
			Debug.Assert(assetPath != null);
			AssetDatabase.AddObjectToAsset(SubResource, assetPath);
			AssetDatabase.ImportAsset(assetPath);
			AssetDatabase.Refresh();
			return SubResource;
		}
		public Object SaveGeneratedResource(GameObject Resource)
		{
			var location = Path.Combine(TargetLocation, STFConstants.ResourceDirectoryName, Resource.name + ".Prefab");
			var prefabGo =  PrefabUtility.SaveAsPrefabAsset(Resource, location);
			return prefabGo;
		}
		public Object SaveGeneratedResource(Object Resource, string FileExtension)
		{
			if(!FileExtension.StartsWith(".")) FileExtension = "." + FileExtension;
			var location = Path.Combine(TargetLocation, STFConstants.ResourceDirectoryName, Resource.name + FileExtension);
			AssetDatabase.CreateAsset(Resource, location);
			AssetDatabase.Refresh();
			return Resource;
		}
		public Object SaveGeneratedResource(byte[] Resource, string Name, string FileExtension)
		{
			if(!FileExtension.StartsWith(".")) FileExtension = "." + FileExtension;
			var location = Path.Combine(TargetLocation, STFConstants.ResourceDirectoryName, Name + FileExtension);
			File.WriteAllBytes(location, Resource);
			AssetDatabase.Refresh();
			return AssetDatabase.LoadAssetAtPath<Object>(location);
		}
		public Object Instantiate(Object Resource)
		{
			if(Resource is GameObject) return PrefabUtility.InstantiatePrefab(Resource);
			else return Object.Instantiate(Resource);
		}
	}
}

#endif
