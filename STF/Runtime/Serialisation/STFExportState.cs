
using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using STF.Util;
using STF.Types;

namespace STF.Serialisation
{
	public class STFExportState
	{
		public STFExportContext Context {protected set; get;}
		public IUnityExportContext UnityContext {protected set; get;}

		// Unity GameObject -> STF Json Node
		public readonly Dictionary<GameObject, (string Id, JObject JsonNode)> Nodes = new();

		// Unity Component -> STF Json NodeComponent
		public readonly Dictionary<Component, (string Id, JObject JsonComponent)> NodeComponents = new();

		// Unity Resource -> Json Resource
		public readonly Dictionary<UnityEngine.Object, (string Id, JObject JsonResource)> Resources = new();

		// Unity ResourceComponent -> Json ResourceComponent
		public readonly Dictionary<ISTFResourceComponent, (string Id, JObject JsonResourceComponent)> ResourceComponents = new();
		

		// id -> buffer
		public readonly Dictionary<string, byte[]> Buffers = new();

		// stuff to delete before the import finishes
		public readonly List<UnityEngine.Object> Trash = new();

		public List<Task> Tasks = new();

		public STFResourceMeta ResourceMeta = new();

		public STFExportState(STFExportContext Context, IUnityExportContext UnityContext, STFResourceMeta ResourceMeta)
		{
			this.Context = Context;
			this.UnityContext = UnityContext;
			this.ResourceMeta = ResourceMeta;
		}

		public void AddTask(Task task) { Tasks.Add(task); }
		public string AddNode(GameObject Go, JObject Serialized, string Id = null)
		{
			if(string.IsNullOrWhiteSpace(Id)) Id = Guid.NewGuid().ToString();
			Nodes.Add(Go, (Id, Serialized));
			return Id;
		}
		public string AddNodeComponent(Component Component, JObject Serialized, string Id = null)
		{
			if(string.IsNullOrWhiteSpace(Id)) Id = Guid.NewGuid().ToString();
			NodeComponents.Add(Component, (Id, Serialized));
			return Id;
		}
		public string AddResource(UnityEngine.Object Resource, JObject Serialized, string Id = null)
		{
			if(string.IsNullOrWhiteSpace(Id)) Id = Guid.NewGuid().ToString();
			Resources.Add(Resource, (Id, Serialized));
			return Id;
		}
		public string AddResourceComponent(ISTFResourceComponent ResourceComponent, JObject Serialized, string Id = null)
		{
			if(string.IsNullOrWhiteSpace(Id)) Id = Guid.NewGuid().ToString();
			ResourceComponents.Add(ResourceComponent, (Id, Serialized));
			return Id;
		}
		public string AddBuffer(byte[] Data, string Id = null)
		{
			if(string.IsNullOrWhiteSpace(Id)) Id = Guid.NewGuid().ToString();
			Buffers.Add(Id, Data);
			return Id;
		}

		public T LoadMeta<T>(UnityEngine.Object Resource) where T: ISTFResource
		{
			if(ResourceMeta.ContainsKey(Resource)) return (T)ResourceMeta[Resource];
			else return UnityContext.LoadMeta<T>(Resource);
		}

		public void AddTrash(UnityEngine.Object Trash) { this.Trash.Add(Trash); }
	}
}
