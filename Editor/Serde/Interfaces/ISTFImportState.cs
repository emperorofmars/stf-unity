
#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json.Linq;
using STF.IdComponents;
using UnityEditor;
using System.Threading.Tasks;
using System.IO;

namespace STF.Serde
{
	public interface ISTFImportState
	{
		STFImportContext Context {get;}
		string TargetLocation {get;}
		string MainAssetId {get;}
		JObject JsonRoot {get;}

		// id -> asset
		Dictionary<string, STFAsset> Assets {get;}

		// id -> resource
		Dictionary<string, UnityEngine.Object> Resources  {get;}

		// id -> buffer
		Dictionary<string, byte[]> Buffers {get;}

		void AddTask(Task task);
		void AddResource(UnityEngine.Object Resource, string Id);
		void AddTrash(UnityEngine.Object Trash);

		void SaveResource(UnityEngine.Object Resource, string FileExtension, string Id);
		void SaveResource<T>(UnityEngine.Object Resource, string FileExtension, T Meta, string Id) where T: UnityEngine.Object, ISTFResource;
		void SaveResource<T>(GameObject Resource, T Meta, string Id) where T: UnityEngine.Object, ISTFResource;
		void SaveResource<T>(byte[] Resource, string FileExtension, T Meta, string Id) where T: UnityEngine.Object, ISTFResource;
	}

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
		Dictionary<string, STFAsset> _Assets = new Dictionary<string, STFAsset>();
		public Dictionary<string, STFAsset> Assets {get => _Assets;}
		Dictionary<string, UnityEngine.Object> _Resources = new Dictionary<string, UnityEngine.Object>();
		public Dictionary<string, UnityEngine.Object> Resources {get => _Resources;}
		Dictionary<string, byte[]> _Buffers = new Dictionary<string, byte[]>();
		public Dictionary<string, byte[]> Buffers {get => _Buffers;}

		// stuff to delete before the import finishes
		public List<UnityEngine.Object> Trash = new List<UnityEngine.Object>();
		public List<Task> Tasks = new List<Task>();

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

		public void AddTrash(UnityEngine.Object Trash)
		{
			this.Trash.Add(Trash);
		}

		public void SaveResource(UnityEngine.Object Resource, string FileExtension, string Id)
		{
			var location = Path.Combine(TargetLocation, STFConstants.ResourceDirectoryName, Resource.name + "_" + Id + "." + FileExtension);
			AssetDatabase.CreateAsset(Resource, location);
			AddResource(Resource, Id);
			AssetDatabase.Refresh();
		}
		public void SaveResource<T>(UnityEngine.Object Resource, string FileExtension, T Meta, string Id) where T: UnityEngine.Object, ISTFResource
		{
			var location = Path.Combine(TargetLocation, STFConstants.ResourceDirectoryName, Meta.Name + "_" + Id + "." + FileExtension);
			AssetDatabase.CreateAsset(Resource, location);
			Meta.Resource = Resource;
			Meta.ResourceLocation = location;
			AssetDatabase.CreateAsset(Meta, Path.ChangeExtension(location, "Asset"));
			AddResource(Meta, Id);
			AssetDatabase.Refresh();
		}
		public void SaveResource<T>(GameObject Resource, T Meta, string Id) where T: UnityEngine.Object, ISTFResource
		{
			var location = Path.Combine(TargetLocation, STFConstants.ResourceDirectoryName, Meta.Name + "_" + Id + ".Prefab");
			var saved = PrefabUtility.SaveAsPrefabAsset(Resource, location);
			Meta.Resource = saved;
			Meta.ResourceLocation = location;
			AssetDatabase.CreateAsset(Meta, Path.ChangeExtension(location, "Asset"));
			AddResource(Meta, Id);
			AssetDatabase.Refresh();
		}
		public void SaveResource<T>(byte[] Resource, string FileExtension, T Meta, string Id) where T: UnityEngine.Object, ISTFResource
		{
			var location = Path.Combine(TargetLocation, STFConstants.ResourceDirectoryName, Meta.Name + "_" + Id + "." + FileExtension);
			File.WriteAllBytes(location, Resource);
			Meta.ResourceLocation = location;
			AssetDatabase.CreateAsset(Meta, Path.ChangeExtension(location, "Asset"));
			AddResource(Meta, Id);
			AssetDatabase.Refresh();
		}
	}
}

#endif
