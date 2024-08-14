
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using STF.Util;
using STF.Types;

namespace STF.Serialisation
{
	public class STFImportState
	{
		public STFImportContext Context {protected set; get;}
		public IUnityImportContext UnityContext {protected set; get;}
		public JObject JsonRoot {protected set; get;}
		public string AssetId {protected set; get;}

		public bool AnyDegraded {protected set; get;} = false;
		
		// id -> node
		public readonly Dictionary<string, ISTFNode> Nodes = new();

		// id -> node_component
		public readonly Dictionary<string, ISTFNodeComponent> NodeComponents = new();

		// id -> resource
		public readonly Dictionary<string, ISTFResource> Resources = new();

		// id -> resource_component
		public readonly Dictionary<string, ISTFResourceComponent> ResourceComponents = new();

		// id -> buffer
		public readonly Dictionary<string, byte[]> Buffers = new();


		// stuff to throw away before the import finishes
		public readonly List<Object> Trash = new();

		public List<Task> Tasks = new();
		public List<Task> PostprocessTasks = new();

		public readonly Dictionary<Object, Object> PostprocessContext = new();

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

		public virtual void AddNode(ISTFNode Node) { Nodes.Add(Node.Id, Node); if(Node.Degraded) AnyDegraded = true; }
		public virtual void AddNodeComponent(ISTFNodeComponent Component) { NodeComponents.Add(Component.Id, Component); if(Component.Degraded) AnyDegraded = true; }
		public virtual void AddResource(ISTFResource Resource)
		{
			var saved = (ISTFResource)UnityContext.SaveResource(Resource);
			if(Resource.Degraded) AnyDegraded = true;
			Resources.Add(saved.Id, saved);
		}
		public virtual void AddResourceComponent(ISTFResourceComponent Component, ISTFResource Resource)
		{
			Component.Resource = new ResourceReference(Resource);
			Resource.Components.Add(Component);
			if(string.IsNullOrWhiteSpace(Component.name)) Component.name = Component.Type + ":" + Component.Id;
			UnityContext.SaveSubResource(Component, Resource);
			ResourceComponents.Add(Component.Id, Component);
			if(Component.Degraded) AnyDegraded = true;
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
			return !string.IsNullOrWhiteSpace(Id) && Nodes.ContainsKey(Id) ? new NodeReference(Nodes[Id]) : new NodeReference(Id);
		}

		public virtual NodeComponentReference GetNodeComponentReference(string Id)
		{
			return !string.IsNullOrWhiteSpace(Id) && NodeComponents.ContainsKey(Id) ? new NodeComponentReference(NodeComponents[Id]) : new NodeComponentReference(Id);
		}

		public virtual ResourceReference GetResourceReference(string Id)
		{
			return !string.IsNullOrWhiteSpace(Id) && Resources.ContainsKey(Id) ? new ResourceReference(Resources[Id]) : new ResourceReference(Id);
		}

		public virtual ResourceComponentReference GetResourceComponentReference(string Id)
		{
			return !string.IsNullOrWhiteSpace(Id) && ResourceComponents.ContainsKey(Id) ? new ResourceComponentReference(ResourceComponents[Id]) : new ResourceComponentReference(Id);
		}
	}
}
