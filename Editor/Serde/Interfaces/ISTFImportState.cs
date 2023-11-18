
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
		string GetResourceLocation();
		void AddResource(UnityEngine.Object Resource, string Id);
		void AddTrash(UnityEngine.Object Trash);
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

		public string GetResourceLocation()
		{
			return Path.Combine(TargetLocation, STFConstants.ResourceDirectoryName);
		}

		public void AddResource(UnityEngine.Object Resource, string Id)
		{
			Resources.Add(Id, Resource);
		}

		public void AddTrash(UnityEngine.Object Trash)
		{
			this.Trash.Add(Trash);
		}
	}
}

#endif
