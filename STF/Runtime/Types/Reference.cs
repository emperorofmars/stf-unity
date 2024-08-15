using System;
using STF.Util;
using UnityEngine;

namespace STF.Types
{
	public interface IReference
	{
		string Id {get;}
		UnityEngine.Object Ref {get;}
		public T GetRef<T>() where T : UnityEngine.Object => (T)Ref;
		public bool IsId {get;}
		public bool IsRef {get;}
	}

	[Serializable]
	public class NodeReference : IReference
	{
		public string NodeId;
		public string Id => Node ? Node.Id : NodeId;
		public ISTFNode Node;
		public UnityEngine.Object Ref => Node;
		public T GetRef<T>() where T : UnityEngine.Object => (T)Ref;
		public bool IsId {get => !string.IsNullOrWhiteSpace(Id); }
		public bool IsRef {get => Ref != null; }

		public NodeReference() {}
		public NodeReference(string NodeId) {Node = null; this.NodeId = NodeId;}
		public NodeReference(ISTFNode Node) {this.Node = Node; NodeId = Node?.Id;}
		public NodeReference(GameObject Node) {this.Node = Utils.GetNodeComponent(Node); NodeId = this.Node?.Id;}

		public static implicit operator NodeReference(ISTFNode Node) => new NodeReference(Node);
		public static implicit operator NodeReference(string Id) => new NodeReference(Id);
			
		public static implicit operator ISTFNode(NodeReference Reference) => Reference.Node;
		public static implicit operator string(NodeReference Reference) => Reference.Id;
	}

	[Serializable]
	public class NodeReference<T> : IReference where T : ISTFNode
	{
		public string NodeId;
		public string Id => Node ? Node.Id : NodeId;
		public T Node;
		public UnityEngine.Object Ref => Node;
		public T GetRef() => (T)Ref;
		public bool IsId {get => !string.IsNullOrWhiteSpace(Id); }
		public bool IsRef {get => Ref != null; }

		public NodeReference() {}
		public NodeReference(string NodeId) {Node = null; this.NodeId = NodeId;}
		public NodeReference(T Node) {this.Node = Node; NodeId = Node?.Id;}
		public NodeReference(GameObject Node) {this.Node = (T)Utils.GetNodeComponent(Node); NodeId = this.Node?.Id;}

		public static implicit operator NodeReference<T>(T Node) => new NodeReference<T>(Node);
		public static implicit operator NodeReference<T>(string Id) => new NodeReference<T>(Id);
			
		public static implicit operator T(NodeReference<T> Reference) => Reference.Node;
		public static implicit operator string(NodeReference<T> Reference) => Reference.Id;

		public static implicit operator NodeReference<T>(NodeReference Reference) => Reference.Node ? new NodeReference<T>((T)Reference.Node) : new NodeReference<T>(Reference.Id);
	}

	[Serializable]
	public class NodeComponentReference : IReference
	{
		public string NodeComponentId;
		public string Id => NodeComponent ? NodeComponent.Id : NodeComponentId;
		public ISTFNodeComponent NodeComponent;
		public UnityEngine.Object Ref => NodeComponent;
		public T GetRef<T>() where T : UnityEngine.Object => (T)Ref;
		public bool IsId {get => !string.IsNullOrWhiteSpace(Id); }
		public bool IsRef {get => Ref != null; }

		public NodeComponentReference() {}
		public NodeComponentReference(string NodeComponentId) {this.NodeComponentId = NodeComponentId; NodeComponent = null;}
		public NodeComponentReference(ISTFNodeComponent NodeComponent) {this.NodeComponent = NodeComponent; NodeComponentId = NodeComponent?.Id;}

		public static implicit operator NodeComponentReference(ISTFNodeComponent NodeComponent) => new NodeComponentReference(NodeComponent);
		public static implicit operator NodeComponentReference(string Id) => new NodeComponentReference(Id);
			
		public static implicit operator ISTFNodeComponent(NodeComponentReference Reference) => Reference.NodeComponent;
		public static implicit operator string(NodeComponentReference Reference) => Reference.Id;
	}

	[Serializable]
	public class NodeComponentReference<T> : IReference where T : ISTFNodeComponent
	{
		public string NodeComponentId;
		public string Id => NodeComponent ? NodeComponent.Id : NodeComponentId;
		public T NodeComponent;
		public UnityEngine.Object Ref => NodeComponent;
		public T GetRef() => (T)Ref;
		public bool IsId {get => !string.IsNullOrWhiteSpace(Id); }
		public bool IsRef {get => Ref != null; }

		public NodeComponentReference() {}
		public NodeComponentReference(string NodeComponentId) {this.NodeComponentId = NodeComponentId; NodeComponent = null;}
		public NodeComponentReference(T NodeComponent) {this.NodeComponent = NodeComponent; NodeComponentId = NodeComponent?.Id;}

		public static implicit operator NodeComponentReference<T>(T NodeComponent) => new NodeComponentReference<T>(NodeComponent);
		public static implicit operator NodeComponentReference<T>(string Id) => new NodeComponentReference<T>(Id);
			
		public static implicit operator T(NodeComponentReference<T> Reference) => Reference.NodeComponent;
		public static implicit operator string(NodeComponentReference<T> Reference) => Reference.Id;

		public static implicit operator NodeComponentReference<T>(NodeComponentReference Reference) => Reference.NodeComponent ? new NodeComponentReference<T>((T)Reference.NodeComponent) : new NodeComponentReference<T>(Reference.Id);
	}

	[Serializable]
	public class ResourceReference : IReference
	{
		public string ResourceId;
		public string Id => Resource ? Resource.Id : ResourceId;
		public ISTFResource Resource;
		public UnityEngine.Object Ref => !IsFallback ? Resource : Fallback.Ref;
		public ResourceReference Fallback = null;

		public bool IsFallback => Fallback != null && Fallback.IsRef;

		public ResourceReference() {}
		public ResourceReference(string ResourceId) {this.ResourceId = ResourceId;}
		public ResourceReference(ISTFResource Resource) { this.Resource = Resource; ResourceId = Resource?.Id; if(Resource is STFUnrecognizedResource) Fallback = Resource.Fallback; }

		public T GetRef<T>() where T : UnityEngine.Object => !IsFallback ? (T)Ref : Fallback.GetRef<T>();
		public bool IsId {get => !string.IsNullOrWhiteSpace(Id); }
		public bool IsRef {get => Ref != null || IsFallback; }

		public static implicit operator ResourceReference(ISTFResource Resource) => new ResourceReference(Resource);
		public static implicit operator ResourceReference(string Id) => new ResourceReference(Id);
			
		public static implicit operator ISTFResource(ResourceReference Reference) => Reference.Resource;
		public static implicit operator string(ResourceReference Reference) => Reference.Id;
	}

	[Serializable]
	public class ResourceReference<T> : IReference where T : ISTFResource
	{
		public string ResourceId;
		public string Id => Resource ? Resource.Id : ResourceId;
		public T Resource;
		public UnityEngine.Object Ref => !IsFallback ? Resource : Fallback.Ref;
		public ResourceReference Fallback = null;

		public bool IsFallback => Fallback != null && Fallback.IsRef;

		public ResourceReference() {}
		public ResourceReference(string ResourceId) {this.ResourceId = ResourceId;}
		public ResourceReference(T Resource) { this.Resource = Resource; ResourceId = Resource?.Id; if(Resource is STFUnrecognizedResource) Fallback = Resource.Fallback; }

		public T GetRef() => !IsFallback ? (T)Ref : Fallback.GetRef<T>();
		public bool IsId {get => !string.IsNullOrWhiteSpace(Id); }
		public bool IsRef {get => Ref != null || IsFallback; }

		public static implicit operator ResourceReference<T>(T Resource) => new(Resource);
		public static implicit operator ResourceReference<T>(string Id) => new(Id);
			
		public static implicit operator T(ResourceReference<T> Reference) => Reference.Resource;
		public static implicit operator string(ResourceReference<T> Reference) => Reference.Id;

		public static implicit operator ResourceReference<T>(ResourceReference Reference) => Reference.Resource ? new ResourceReference<T>((T)Reference.Resource) : new ResourceReference<T>(Reference.Id);
	}

	[Serializable]
	public class ResourceComponentReference : IReference
	{
		public string ResourceComponentId;
		public string Id => ResourceComponent ? ResourceComponent.Id : ResourceComponentId;
		public ISTFResourceComponent ResourceComponent;
		public UnityEngine.Object Ref => ResourceComponent;
		public T GetRef<T>() where T : UnityEngine.Object => (T)Ref;
		public bool IsId {get => !string.IsNullOrWhiteSpace(Id); }
		public bool IsRef {get => Ref != null; }

		public ResourceComponentReference() {}
		public ResourceComponentReference(string ResourceComponentId) {this.ResourceComponentId = ResourceComponentId; ResourceComponent = null;}
		public ResourceComponentReference(ISTFResourceComponent ResourceComponent) {this.ResourceComponent = ResourceComponent; ResourceComponentId = ResourceComponent?.Id;}

		public static implicit operator ResourceComponentReference(ISTFResourceComponent ResourceComponent) => new ResourceComponentReference(ResourceComponent);
		public static implicit operator ResourceComponentReference(string Id) => new ResourceComponentReference(Id);
			
		public static implicit operator ISTFResourceComponent(ResourceComponentReference Reference) => Reference.ResourceComponent;
		public static implicit operator string(ResourceComponentReference Reference) => Reference.Id;
	}

	[Serializable]
	public class ResourceComponentReference<T> : IReference where T : ISTFResourceComponent
	{
		public string ResourceComponentId;
		public string Id => ResourceComponent ? ResourceComponent.Id : ResourceComponentId;
		public T ResourceComponent;
		public UnityEngine.Object Ref => ResourceComponent;
		public T GetRef() => (T)Ref;
		public bool IsId {get => !string.IsNullOrWhiteSpace(Id); }
		public bool IsRef {get => Ref != null; }

		public ResourceComponentReference() {}
		public ResourceComponentReference(string ResourceComponentId) {this.ResourceComponentId = ResourceComponentId; ResourceComponent = null;}
		public ResourceComponentReference(T ResourceComponent) {this.ResourceComponent = ResourceComponent; ResourceComponentId = ResourceComponent?.Id;}

		public static implicit operator ResourceComponentReference<T>(T ResourceComponent) => new ResourceComponentReference<T>(ResourceComponent);
		public static implicit operator ResourceComponentReference<T>(string Id) => new ResourceComponentReference<T>(Id);
			
		public static implicit operator T(ResourceComponentReference<T> Reference) => Reference.ResourceComponent;
		public static implicit operator string(ResourceComponentReference<T> Reference) => Reference.Id;

		public static implicit operator ResourceComponentReference<T>(ResourceComponentReference Reference) => Reference.ResourceComponent ? new ResourceComponentReference<T>((T)Reference.ResourceComponent) : new ResourceComponentReference<T>(Reference.Id);
	}
}
