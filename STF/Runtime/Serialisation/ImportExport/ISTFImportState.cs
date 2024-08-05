
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace STF.Serialisation
{
	public interface ISTFImportState
	{
		STFImportContext Context {get;}
		string TargetLocation {get;}
		JObject JsonRoot {get;}

		ISTFAsset Asset {get;}
		Dictionary<UnityEngine.Object, UnityEngine.Object> PostprocessContext {get;}

		// id -> node
		Dictionary<string, UnityEngine.GameObject> Nodes {get;}

		// id -> node_component
		Dictionary<string, Component> NodeComponents {get;}

		// id -> resource
		Dictionary<string, UnityEngine.Object> Resources  {get;}

		// id -> resource_component
		Dictionary<string, UnityEngine.Object> ResourceComponents {get;}

		// id -> buffer
		Dictionary<string, byte[]> Buffers {get;}

		void AddTask(Task task);
		void AddPostprocessTask(Task task);
		void AddNode(GameObject Node, string Id);
		void AddNodeComponent(Component Component, string Id);
		void AddResource(UnityEngine.Object Resource, string Id);
		void AddResourceComponent(ISTFResourceComponent Component, ISTFResource ResourceMeta, string Id);
		void AddTrash(UnityEngine.Object Trash);
		void SetPostprocessContext(UnityEngine.Object Resource, UnityEngine.Object Context);

		void SaveSecondaryResource(UnityEngine.Object Component, UnityEngine.Object Resource);
		void SaveResource(UnityEngine.Object Resource, string FileExtension, string Id);
		void SaveResource<T>(UnityEngine.Object Resource, string FileExtension, T Meta, string Id) where T: ISTFResource;
		void SaveResource<T>(GameObject Resource, T Meta, string Id) where T: ISTFResource;
		void SaveResource<M, R>(byte[] Resource, string FileExtension, M Meta, string Id) where M: ISTFResource where R: UnityEngine.Object;
		T SaveAndLoadResource<T>(byte[] Resource, string Name, string FileExtension) where T: UnityEngine.Object;
		void SaveResourceBelongingToId(UnityEngine.Object Resource, string FileExtension, string OwnerId);
		void SaveGeneratedResource(UnityEngine.Object Resource, string FileExtension);

		UnityEngine.Object LoadResource(ISTFResource Resource);
		UnityEngine.Object Instantiate(UnityEngine.Object Resource);
	}
}
