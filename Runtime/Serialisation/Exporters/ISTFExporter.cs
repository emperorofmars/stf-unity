
using UnityEngine;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

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
		void AddResourceContext(UnityEngine.Object unityResource, string key, System.Object data);
		Dictionary<string, System.Object> GetResourceContext(UnityEngine.Object unityResource);
		string GetJson();
		byte[] GetBinary();

		void AddMeta(STFMeta meta);
		List<STFMeta> GetMetas();
	}
}