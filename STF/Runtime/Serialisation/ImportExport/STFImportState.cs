
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace STF.Serialisation
{
	public class STFImportState
	{
		public STFImportState(STFImportContext Context, IUnityAssetImportContext UnityContext, JObject JsonRoot)
		{
			this.Context = Context;
			this.UnityContext = UnityContext;
			UnityContext.State = this;
			this.JsonRoot = JsonRoot;
			this.AssetId = (string)JsonRoot["asset"]["id"];
		}

		public STFImportContext Context {protected set; get;}
		public IUnityAssetImportContext UnityContext {protected set; get;}
		public JObject JsonRoot {protected set; get;}

		public string AssetId {protected set; get;}

		// id -> node
		public Dictionary<string, GameObject> Nodes {protected set; get;} = new Dictionary<string, GameObject>();

		// id -> node_component
		public Dictionary<string, Component> NodeComponents {protected set; get;} = new Dictionary<string, Component>();

		// id -> resource
		public Dictionary<string, Object> Resources {protected set; get;} = new Dictionary<string, Object>();

		// id -> resource_component
		public Dictionary<string, Object> ResourceComponents {protected set; get;} = new Dictionary<string, Object>();

		// id -> buffer
		public Dictionary<string, byte[]> Buffers {protected set; get;} = new Dictionary<string, byte[]>();


		// stuff to throw away before the import finishes
		public List<Object> Trash = new List<Object>();

		public List<Task> Tasks = new List<Task>();

		public List<Task> PostprocessTasks = new List<Task>();
		public Dictionary<Object, Object> PostprocessContext {protected set; get;} = new Dictionary<Object, Object>();


		public virtual void AddTask(Task task) { Tasks.Add(task); }
		public virtual void AddPostprocessTask(Task task) { PostprocessTasks.Add(task); }
		public virtual void AddNode(GameObject Node, string Id) { Nodes.Add(Id, Node); }
		public virtual void AddNodeComponent(Component Component, string Id) { NodeComponents.Add(Id, Component); }
		public virtual void AddResource(Object Resource, string Id) { Resources.Add(Id, Resource); }
		public virtual void AddResourceComponent(ISTFResourceComponent Component, ISTFResource ResourceMeta, string Id)
		{
			Component.Resource = ResourceMeta;
			ResourceMeta.Components.Add(Component);
			if(string.IsNullOrWhiteSpace(Component.name)) Component.name = Component.Type + ":" + Id;
			UnityContext.SaveSubResource(Component, ResourceMeta);
		}

		public virtual void AddTrash(Object Trash) { this.Trash.Add(Trash); }

		public virtual void SetPostprocessContext(Object Resource, Object Context)
		{
			if(PostprocessContext.ContainsKey(Resource)) PostprocessContext[Resource] = Context;
			else PostprocessContext.Add(Resource, Context);
		}
/*
		public abstract void SaveSubResource(Object Component, Object Resource);
		public abstract void SaveResource(Object Resource, string FileExtension, string Id);
		public abstract void SaveResource<T>(Object Resource, string FileExtension, T Meta, string Id) where T: ISTFResource;
		public abstract void SaveResource<T>(GameObject Resource, T Meta, string Id) where T: ISTFResource;
		public abstract void SaveResource<M, R>(byte[] Resource, string FileExtension, M Meta, string Id) where M: ISTFResource where R: Object;
		public abstract T SaveAndLoadResource<T>(byte[] Resource, string Name, string FileExtension) where T: Object;
		public abstract void SaveResourceBelongingToId(Object Resource, string FileExtension, string OwnerId);
		public abstract void SaveGeneratedResource(Object Resource, string FileExtension);

		public abstract Object Instantiate(Object Resource);
*/
	}
}
