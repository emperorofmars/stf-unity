
#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using UnityEditor;
using System.Threading.Tasks;
using System.IO;

namespace STF.Serialisation
{
	public class STFExportState : ISTFExportState
	{
		STFExportContext _Context;
		public STFExportContext Context {get =>_Context;}
		string _TargetLocation;
		public string TargetLocation {get =>_TargetLocation;}
		public string _MainAssetId;
		public string MainAssetId {get => _MainAssetId;}

		public Dictionary<ISTFAsset, (string Id, JObject JsonAsset)> _Assets = new Dictionary<ISTFAsset, (string Id, JObject JsonAsset)>();
		public Dictionary<ISTFAsset, (string Id, JObject JsonAsset)> Assets {get => _Assets;}

		public Dictionary<GameObject, (string Id, JObject JsonNode)> _Nodes = new Dictionary<GameObject, (string Id, JObject JsonNode)>();
		public Dictionary<GameObject, (string Id, JObject JsonNode)> Nodes {get => _Nodes;}

		public Dictionary<UnityEngine.Object, (string Id, JObject JsonResource)> _Resources = new Dictionary<UnityEngine.Object, (string Id, JObject JsonResource)>();
		public Dictionary<UnityEngine.Object, (string Id, JObject JsonResource)> Resources {get => _Resources;}

		public Dictionary<Component, (string Id, JObject JsonComponent)> _Components = new Dictionary<Component, (string Id, JObject JsonComponent)>();
		public Dictionary<Component, (string Id, JObject JsonComponent)> Components {get => _Components;}

		public Dictionary<ISTFResourceComponent, (string Id, JObject JsonResourceComponent)> _ResourceComponents = new Dictionary<ISTFResourceComponent, (string Id, JObject JsonResourceComponent)>();
		public Dictionary<ISTFResourceComponent, (string Id, JObject JsonResourceComponent)> ResourceComponents => _ResourceComponents;

		// id -> buffer
		public Dictionary<string, byte[]> Buffers = new Dictionary<string, byte[]>();

		// stuff to delete before the import finishes
		public List<UnityEngine.Object> Trash = new List<UnityEngine.Object>();
		public List<Task> Tasks = new List<Task>();

		public Dictionary<UnityEngine.Object, UnityEngine.Object> ResourceMeta = new Dictionary<UnityEngine.Object, UnityEngine.Object>();

		public STFExportState(STFExportContext Context, string TargetLocation, Dictionary<UnityEngine.Object, UnityEngine.Object> ResourceMeta)
		{
			this._Context = Context;
			this._TargetLocation = TargetLocation;
			this.ResourceMeta = ResourceMeta;
		}

		public void AddTask(Task task)
		{
			Tasks.Add(task);
		}

		public string AddAsset(ISTFAsset Asset, JObject Serialized, string Id = null)
		{
			if(Id == null || Id.Length == 0) Id = Guid.NewGuid().ToString();
			Assets.Add(Asset, (Id, Serialized));
			return Id;
		}

		public string AddNode(GameObject Go, JObject Serialized, string Id = null)
		{
			if(Id == null || Id.Length == 0) Id = Guid.NewGuid().ToString();
			Nodes.Add(Go, (Id, Serialized));
			return Id;
		}

		public string AddComponent(Component Component, JObject Serialized, string Id = null)
		{
			if(Id == null || Id.Length == 0) Id = Guid.NewGuid().ToString();
			Components.Add(Component, (Id, Serialized));
			return Id;
		}

		public string AddResource(UnityEngine.Object Resource, JObject Serialized, string Id = null)
		{
			if(Id == null || Id.Length == 0) Id = Guid.NewGuid().ToString();
			Resources.Add(Resource, (Id, Serialized));
			return Id;
		}

		public string AddBuffer(byte[] Data, string Id = null)
		{
			if(Id == null || Id.Length == 0) Id = Guid.NewGuid().ToString();
			Buffers.Add(Id, Data);
			return Id;
		}

		public void AddTrash(UnityEngine.Object Trash)
		{
			this.Trash.Add(Trash);
		}

		public T LoadMeta<T>(UnityEngine.Object Resource) where T: UnityEngine.Object, ISTFResource
		{
			if(ResourceMeta.ContainsKey(Resource)) return (T)ResourceMeta[Resource];
			
			var assetPath = AssetDatabase.GetAssetPath(Resource);
			var metaPath = Path.ChangeExtension(assetPath, "Asset");
			return AssetDatabase.LoadAssetAtPath<T>(metaPath);
		}
		public (byte[], T, string) LoadAsset<T>(UnityEngine.Object Resource) where T: UnityEngine.Object, ISTFResource
		{
			var assetPath = AssetDatabase.GetAssetPath(Resource);
			var arrayBuffer = File.ReadAllBytes(assetPath);
			var meta = AssetDatabase.LoadAssetAtPath<T>(Path.ChangeExtension(assetPath, "Asset"));
			return (arrayBuffer, meta, Path.GetFileName(assetPath));
		}

		public string AddResourceComponent(ISTFResourceComponent ResourceComponent, JObject Serialized, string Id = null)
		{
			throw new NotImplementedException();
		}
	}
}

#endif
