
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.IO;

namespace STF.Serialisation
{
	public interface ISTFExportState
	{
		STFExportContext Context {get;}
		string TargetLocation {get;}
		string MainAssetId {get;}

		// Unity Asset -> Json Asset
		Dictionary<ISTFAsset, (string Id, JObject JsonAsset)> Assets {get;}

		// Unity Resource -> Json Resource
		Dictionary<UnityEngine.Object, (string Id, JObject JsonResource)> Resources {get;}

		// Unity GameObject -> STF Json Node
		Dictionary<GameObject, (string Id, JObject JsonNode)> Nodes {get;}

		// Unity Component -> STF Json Component
		Dictionary<Component, (string Id, JObject JsonComponent)> Components {get;}

		void AddTask(Task task);
		string AddAsset(ISTFAsset Asset, JObject Serialized, string Id = null);
		string AddNode(GameObject Go, JObject Serialized, string Id = null);
		string AddComponent(Component Component, JObject Serialized, string Id = null);
		string AddResource(UnityEngine.Object Resource, JObject Serialized, string Id = null);
		string AddBuffer(byte[] Data, string Id = null);
		void AddTrash(UnityEngine.Object Trash);

		T LoadMeta<T>(UnityEngine.Object Resource) where T: UnityEngine.Object, ISTFResource;
		(byte[] Data, T Meta, string Path) LoadAsset<T>(UnityEngine.Object Resource) where T: UnityEngine.Object, ISTFResource;
	}
}
