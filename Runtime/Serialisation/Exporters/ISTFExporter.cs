
using UnityEngine;
using System.Threading.Tasks;

namespace stf.serialisation
{
	public interface ISTFExporter
	{
		void AddTask(Task task);
		string RegisterNode(GameObject go, ASTFNodeExporter exporter);
		void RegisterResource(UnityEngine.Object unityResource);
		void RegisterResource(UnityEngine.Object unityResource, ASTFResourceExporter exporter);
		void RegisterComponent(string nodeId, Component component);
		void RegisterComponent(string nodeId, Component component, ASTFComponentExporter exporter);
		string RegisterBuffer(byte[] buffer);
		string GetNodeId(GameObject go);
		string GetResourceId(UnityEngine.Object unityResource);
		STFExportContext GetContext();
		string GetJson();
		byte[] GetBinary();
	}
}