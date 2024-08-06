
#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using UnityEditor;
using System.Threading.Tasks;
using System.IO;

namespace STF.Serialisation
{
	public class EditorUnityImportContext : IUnityImportContext
	{

		public EditorUnityImportContext(string TargetLocation)
		{
			this.TargetLocation = TargetLocation;
		}

		string TargetLocation;
		STFImportState _State;
		public STFImportState State { get => _State; set => _State = value; }

		public void SaveSubResource(Object Component, Object Resource)
		{
			var assetPath = AssetDatabase.GetAssetPath(Resource);
			Debug.Assert(assetPath != null);
			AssetDatabase.AddObjectToAsset(Component, assetPath);
			AssetDatabase.ImportAsset(assetPath);
			AssetDatabase.Refresh();
		}

		public void SaveResource(Object Resource, string FileExtension, string Id)
		{
			if(!FileExtension.StartsWith(".")) FileExtension = "." + FileExtension;
			var location = Path.Combine(TargetLocation, STFConstants.ResourceDirectoryName, Resource.name + "_" + Id + FileExtension);
			AssetDatabase.CreateAsset(Resource, location);
			State.AddResource(Resource, Id);
			AssetDatabase.Refresh();
		}
		public void SaveResource<T>(Object Resource, string FileExtension, T Meta, string Id) where T: ISTFResource
		{
			if(!FileExtension.StartsWith(".")) FileExtension = "." + FileExtension;
			var location = Path.Combine(TargetLocation, STFConstants.ResourceDirectoryName, Meta.Name + "_" + Id + FileExtension);
			AssetDatabase.CreateAsset(Resource, location);
			Meta.Resource = Resource;
			AssetDatabase.CreateAsset(Meta, Path.ChangeExtension(location, "Asset"));
			State.AddResource(Meta, Id);
			AssetDatabase.Refresh();
		}
		public void SaveResource<T>(GameObject Resource, T Meta, string Id) where T: ISTFResource
		{
			var location = Path.Combine(TargetLocation, STFConstants.ResourceDirectoryName, Meta.Name + "_" + Id + ".Prefab");
			var saved = PrefabUtility.SaveAsPrefabAsset(Resource, location);
			Meta.Resource = saved;
			AssetDatabase.CreateAsset(Meta, Path.ChangeExtension(location, "Asset"));
			State.AddResource(Meta, Id);
			AssetDatabase.Refresh();
		}
		public void SaveResource<M, R>(byte[] Resource, string FileExtension, M Meta, string Id) where M: ISTFResource where R: Object
		{
			if(!FileExtension.StartsWith(".")) FileExtension = "." + FileExtension;
			var location = Path.Combine(TargetLocation, STFConstants.ResourceDirectoryName, Meta.Name + "_" + Id + FileExtension);
			
			AssetDatabase.CreateAsset(Meta, Path.ChangeExtension(location, "Asset"));
			File.WriteAllBytes(location, Resource);
			AssetDatabase.Refresh();
			
			Meta.Resource = AssetDatabase.LoadAssetAtPath<R>(location);
			State.AddResource(Meta, Id);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}
		public Object SaveAndLoadResource(byte[] Resource, string Name, string FileExtension)
		{
			if(!FileExtension.StartsWith(".")) FileExtension = "." + FileExtension;
			var location = Path.Combine(TargetLocation, STFConstants.ResourceDirectoryName, Name + FileExtension);
			File.WriteAllBytes(location, Resource);
			AssetDatabase.Refresh();
			return AssetDatabase.LoadAssetAtPath<Object>(location);
		}
		public void SaveResourceBelongingToId(Object Resource, string FileExtension, string OwnerId)
		{
			if(!FileExtension.StartsWith(".")) FileExtension = "." + FileExtension;
			var location = Path.Combine(TargetLocation, STFConstants.ResourceDirectoryName, Resource.name + "_" + OwnerId + FileExtension);
			AssetDatabase.CreateAsset(Resource, location);
			AssetDatabase.Refresh();
		}
		public void SaveGeneratedResource(Object Resource, string FileExtension)
		{
			if(!FileExtension.StartsWith(".")) FileExtension = "." + FileExtension;
			var location = Path.Combine(TargetLocation, STFConstants.ResourceDirectoryName, Resource.name + FileExtension);
			AssetDatabase.CreateAsset(Resource, location);
			AssetDatabase.Refresh();
		}
		
		public Object Instantiate(Object Resource)
		{
			if(Resource is GameObject) return PrefabUtility.InstantiatePrefab(Resource);
			else return Object.Instantiate(Resource);
		}
	}
}

#endif
