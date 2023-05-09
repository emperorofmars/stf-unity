
using UnityEngine;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace stf.serialisation
{
	public interface ISTFExporter
	{
		STFExportContext GetContext();
		void AddTask(Task task);
		void RegisterNode(string nodeId, JObject node);
		void RegisterNode(string nodeId, JObject node, GameObject go);
		void RegisterResource(string resourceId, JObject resource);
		void RegisterResource(UnityEngine.Object unityResource);
		void RegisterResource(UnityEngine.Object unityResource, ASTFResourceExporter exporter);
		void RegisterComponent(string nodeId, Component component);
		void RegisterComponent(string nodeId, Component component, ASTFComponentExporter exporter);
		string RegisterBuffer(byte[] buffer);
		string GetNodeId(GameObject go);
		string GetResourceId(UnityEngine.Object unityResource);
		void RegisterSubresourceId(UnityEngine.Object unityResource, string key, string id);
		string GetSubresourceId(UnityEngine.Object unityResource, string key);
		string GetJson();
		byte[] GetBinary();
		//STFMeta GetMeta();
	}
}