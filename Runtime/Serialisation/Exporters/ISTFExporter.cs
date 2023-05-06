
using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;

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
		void SetArmature(Mesh mesh, STFArmatureAsset armature);
		bool HasArmature(Mesh mesh);
		void RegisterArmatureNode(STFArmatureAsset armature, GameObject go);
		STFArmatureAsset GetArmature(Mesh mesh);
		STFExportContext GetContext();
		string GetJson();
		byte[] GetBinary();
		STFMeta GetMeta();
	}
}