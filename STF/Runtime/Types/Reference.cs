using System;
using STF.Util;
using UnityEngine;

namespace STF.Types
{
	public interface IReference
	{
		public abstract string Id {get;}
		public abstract UnityEngine.Object Ref {get;}
	}

	[Serializable]
	public class NodeReference : IReference
	{
		public string NodeId;
		public string Id => Node ? Node.Id : NodeId;
		public ISTFNode Node;
		public UnityEngine.Object Ref => Node;

		public NodeReference(string NodeId) {this.NodeId = NodeId;}
		public NodeReference(ISTFNode Node) {this.Node = Node; NodeId = Node.Id;}
		public NodeReference(GameObject Node) {this.Node = Utils.GetNodeComponent(Node); NodeId = this.Node.Id;}
	}

	[Serializable]
	public class NodeComponentReference : IReference
	{
		public string NodeId;
		public string NodeComponentId;
		public string Id => NodeComponent ? NodeComponent.Id : NodeComponentId;
		public ISTFNode Node;
		public ISTFNodeComponent NodeComponent;
		public UnityEngine.Object Ref => NodeComponent;

		public NodeComponentReference(string NodeId, string NodeComponentId) {this.NodeId = NodeId; this.NodeComponentId = NodeComponentId;}
		public NodeComponentReference(ISTFNode Node, ISTFNodeComponent NodeComponent) {this.Node = Node; this.NodeComponent = NodeComponent; NodeId = Node.Id; NodeComponentId = NodeComponent.Id;}
	}

	[Serializable]
	public class ResourceReference : IReference
	{
		public string ResourceId;
		public string Id => Resource ? Resource.Id : ResourceId;
		public ISTFResource Resource;
		public UnityEngine.Object Ref => Resource;

		public bool IsFallback => Resource is STFUnrecognizedResource && (Resource as STFUnrecognizedResource).Fallback ? true : false;

		public ResourceReference(string ResourceId) {this.ResourceId = ResourceId;}
		public ResourceReference(ISTFResource Resource) { this.Resource = Resource; ResourceId = Resource.Id; }
	}

	[Serializable]
	public class ResourceComponentReference : IReference
	{
		public string ResourceId;
		public string ResourceComponentId;
		public string Id => ResourceComponent ? ResourceComponent.Id : ResourceComponentId;
		public ISTFResource Resource;
		public ISTFResourceComponent ResourceComponent;
		public UnityEngine.Object Ref => ResourceComponent;

		public ResourceComponentReference(string ResourceId, string ResourceComponentId) {this.ResourceId = ResourceId; this.ResourceComponentId = ResourceComponentId;}
		public ResourceComponentReference(ISTFResource Resource, ISTFResourceComponent ResourceComponent) { this.Resource = Resource; ResourceId = Resource.Id; this.ResourceComponent = ResourceComponent; ResourceComponentId = ResourceComponent.Id; }
	}
}
