
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using STF.Util;

namespace STF.Serialisation
{
	public class STFImportState
	{
		public STFImportContext Context {protected set; get;}
		public IUnityImportContext UnityContext {protected set; get;}
		public JObject JsonRoot {protected set; get;}
		public string AssetId {protected set; get;}

		public STFImportState(STFImportContext Context, IUnityImportContext UnityContext, STFFile Buffers)
		{
			this.Context = Context;
			this.UnityContext = UnityContext;
			JsonRoot = JObject.Parse(Buffers.Json);
			AssetId = (string)JsonRoot["asset"]["id"];
			
			for(int i = 0; i < Buffers.Buffers.Count; i++)
			{
				this.Buffers.Add((string)JsonRoot[STFKeywords.ObjectType.Buffers][i], Buffers.Buffers[i]);
			}
		}
		
		// id -> node
		public Dictionary<string, ISTFNode> Nodes {protected set; get;} = new Dictionary<string, ISTFNode>();

		// id -> node_component
		public Dictionary<string, ISTFNodeComponent> NodeComponents {protected set; get;} = new Dictionary<string, ISTFNodeComponent>();

		// id -> resource
		public Dictionary<string, ISTFResource> Resources {protected set; get;} = new Dictionary<string, ISTFResource>();

		// id -> resource_component
		public Dictionary<string, ISTFResourceComponent> ResourceComponents {protected set; get;} = new Dictionary<string, ISTFResourceComponent>();

		// id -> buffer
		public Dictionary<string, byte[]> Buffers {protected set; get;} = new Dictionary<string, byte[]>();


		// stuff to throw away before the import finishes
		public List<Object> Trash = new List<Object>();

		public List<Task> Tasks = new List<Task>();

		public List<Task> PostprocessTasks = new List<Task>();
		public Dictionary<Object, Object> PostprocessContext {protected set; get;} = new Dictionary<Object, Object>();

		public virtual void AddNode(ISTFNode Node) { Nodes.Add(Node.Id, Node); }
		public virtual void AddNodeComponent(ISTFNodeComponent Component) { NodeComponents.Add(Component.Id, Component); }
		public virtual void AddResource(ISTFResource Resource)
		{
			var saved = (ISTFResource)UnityContext.SaveResource(Resource);
			Resources.Add(saved.Id, saved);
		}
		public virtual void AddResourceComponent(ISTFResourceComponent Component, ISTFResource Resource)
		{
			Component.Resource = Resource;
			Resource.Components.Add(Component);
			if(string.IsNullOrWhiteSpace(Component.name)) Component.name = Component.Type + ":" + Component.Id;
			UnityContext.SaveSubResource(Component, Resource);
			ResourceComponents.Add(Component.Id, Component);
		}

		public virtual void AddTask(Task task) { Tasks.Add(task); }
		public virtual void AddPostprocessTask(Task task) { PostprocessTasks.Add(task); }
		public virtual void AddTrash(Object Trash) { this.Trash.Add(Trash); }

		public virtual void SetPostprocessContext(Object Resource, Object Context)
		{
			if(PostprocessContext.ContainsKey(Resource)) PostprocessContext[Resource] = Context;
			else PostprocessContext.Add(Resource, Context);
		}

		public virtual NodeReference GetNodeReference(string Id)
		{
			return Nodes.ContainsKey(Id) ? new NodeReference(Nodes[Id]) : new NodeReference(Id);
		}

		public virtual NodeReference GetNodeComponentReference(string Id)
		{
			return Nodes.ContainsKey(Id) ? new NodeReference(Nodes[Id]) : new NodeReference(Id);
		}

		internal ResourceReference GetResourceReference(string Id)
		{
			return Resources.ContainsKey(Id) ? new ResourceReference(Resources[Id]) : new ResourceReference(Id);
		}
	}
}
