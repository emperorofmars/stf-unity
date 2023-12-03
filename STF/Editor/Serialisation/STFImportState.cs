
#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using UnityEditor;
using System.Threading.Tasks;
using System.IO;

namespace STF.Serialisation
{
	public class STFImportState : ISTFImportState
	{
		STFImportContext _Context;
		public STFImportContext Context {get =>_Context;}
		string _TargetLocation;
		public string TargetLocation {get =>_TargetLocation;}
		string _MainAssetId;
		public string MainAssetId {get => _MainAssetId;}
		JObject _JsonRoot;
		public JObject JsonRoot {get => _JsonRoot;}
		//Dictionary<string, STFAsset> _Assets = new Dictionary<string, STFAsset>();
		//public Dictionary<string, STFAsset> Assets {get => _Assets;}
		Dictionary<string, UnityEngine.Object> _Resources = new Dictionary<string, UnityEngine.Object>();
		public Dictionary<string, UnityEngine.Object> Resources {get => _Resources;}
		Dictionary<string, byte[]> _Buffers = new Dictionary<string, byte[]>();
		public Dictionary<string, byte[]> Buffers {get => _Buffers;}

		// stuff to delete before the import finishes
		public List<UnityEngine.Object> Trash = new List<UnityEngine.Object>();
		public List<Task> Tasks = new List<Task>();

		public List<(GameObject Asset, string Name, bool Main)> AssetsToSave = new List<(GameObject Asset, string Name, bool Main)>();

		public STFImportState(STFImportContext Context, string TargetLocation, JObject JsonRoot)
		{
			this._Context = Context;
			this._TargetLocation = TargetLocation;
			this._JsonRoot = JsonRoot;
			this._MainAssetId = (string)JsonRoot["main"];
		}

		public void AddTask(Task task)
		{
			Tasks.Add(task);
		}

		public void AddResource(UnityEngine.Object Resource, string Id)
		{
			Resources.Add(Id, Resource);
		}

		public void AddResourceComponent(ISTFResourceComponent Component, ISTFResource Resource, string Id)
		{
			Component.Resource = Resource;
			Resource.Components.Add(Component);
			AssetDatabase.AddObjectToAsset(Component, AssetDatabase.GetAssetPath(Resource));
			//AssetDatabase.AddObjectToAsset(Component, Resource);
			AssetDatabase.Refresh();
		}

		public void AddTrash(UnityEngine.Object Trash)
		{
			this.Trash.Add(Trash);
		}

		public void SaveResource(UnityEngine.Object Resource, string FileExtension, string Id)
		{
			if(!FileExtension.StartsWith(".")) FileExtension = "." + FileExtension;
			var location = Path.Combine(TargetLocation, STFConstants.ResourceDirectoryName, Resource.name + "_" + Id + FileExtension);
			AssetDatabase.CreateAsset(Resource, location);
			AddResource(Resource, Id);
			AssetDatabase.Refresh();
		}
		public void SaveResource<T>(UnityEngine.Object Resource, string FileExtension, T Meta, string Id) where T: ISTFResource
		{
			if(!FileExtension.StartsWith(".")) FileExtension = "." + FileExtension;
			var location = Path.Combine(TargetLocation, STFConstants.ResourceDirectoryName, Meta.Name + "_" + Id + FileExtension);
			AssetDatabase.CreateAsset(Resource, location);
			Meta.Resource = Resource;
			Meta.ResourceLocation = location;
			AssetDatabase.CreateAsset(Meta, Path.ChangeExtension(location, "Asset"));
			AddResource(Meta, Id);
			AssetDatabase.Refresh();
		}
		public void SaveResource<T>(GameObject Resource, T Meta, string Id) where T: ISTFResource
		{
			var location = Path.Combine(TargetLocation, STFConstants.ResourceDirectoryName, Meta.Name + "_" + Id + ".Prefab");
			var saved = PrefabUtility.SaveAsPrefabAsset(Resource, location);
			Meta.Resource = saved;
			Meta.ResourceLocation = location;
			AssetDatabase.CreateAsset(Meta, Path.ChangeExtension(location, "Asset"));
			AddResource(Meta, Id);
			AssetDatabase.Refresh();
		}
		public void SaveResource<M, R>(byte[] Resource, string FileExtension, M Meta, string Id) where M: ISTFResource where R: UnityEngine.Object
		{
			if(!FileExtension.StartsWith(".")) FileExtension = "." + FileExtension;
			var location = Path.Combine(TargetLocation, STFConstants.ResourceDirectoryName, Meta.Name + "_" + Id + FileExtension);
			File.WriteAllBytes(location, Resource);
			Meta.ResourceLocation = location;
			AssetDatabase.Refresh();
			Meta.Resource = AssetDatabase.LoadAssetAtPath<R>(location);
			AssetDatabase.CreateAsset(Meta, Path.ChangeExtension(location, "Asset"));
			AddResource(Meta, Id);
			AssetDatabase.Refresh();
		}
		public T SaveAndLoadResource<T>(byte[] Resource, string Name, string FileExtension) where T: UnityEngine.Object
		{
			if(!FileExtension.StartsWith(".")) FileExtension = "." + FileExtension;
			var location = Path.Combine(TargetLocation, STFConstants.ResourceDirectoryName, Name + FileExtension);
			File.WriteAllBytes(location, Resource);
			AssetDatabase.Refresh();
			return AssetDatabase.LoadAssetAtPath<T>(location);
		}
		public void SaveResourceBelongingToId(UnityEngine.Object Resource, string FileExtension, string OwnerId)
		{
			if(!FileExtension.StartsWith(".")) FileExtension = "." + FileExtension;
			var location = Path.Combine(TargetLocation, STFConstants.ResourceDirectoryName, Resource.name + "_" + OwnerId + FileExtension);
			AssetDatabase.CreateAsset(Resource, location);
			AssetDatabase.Refresh();
		}
		public void SaveGeneratedResource(UnityEngine.Object Resource, string FileExtension)
		{
			if(!FileExtension.StartsWith(".")) FileExtension = "." + FileExtension;
			var location = Path.Combine(TargetLocation, STFConstants.ResourceDirectoryName, Resource.name + FileExtension);
			AssetDatabase.CreateAsset(Resource, location);
			AssetDatabase.Refresh();
		}

		public Object LoadResource(ISTFResource Resource)
		{
			if(Resource.Resource != null) return Resource.Resource;
			else if(Resource.ResourceLocation != null && Resource.ResourceLocation.Length > 0) return AssetDatabase.LoadAssetAtPath<Object>(Resource.ResourceLocation);
			throw new System.Exception("Error retrieving resource: " + Resource);
		}
		
		public UnityEngine.Object Instantiate(UnityEngine.Object Resource)
		{
			return PrefabUtility.InstantiatePrefab(Resource);
		}
		
		public void SaveAsset(GameObject Root, string Name, bool Main = false)
		{
			this.AssetsToSave.Add((Root, Name, Main));
		}
	}
}

#endif
