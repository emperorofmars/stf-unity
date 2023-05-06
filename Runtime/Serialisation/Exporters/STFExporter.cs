
using UnityEngine;
using System.Collections.Generic;
using stf.Components;
using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text;

namespace stf.serialisation
{
	public class STFExporter : ISTFExporter
	{
		public static string _MAGIC = "STF0";

		private STFExportContext context;
		private List<ISTFAssetExporter> assets;
		public List<Task> registerResourceTasks = new List<Task>();
		public List<Task> registerComponentTasks = new List<Task>();
		public List<Task> tasks = new List<Task>();

		public Dictionary<Mesh, STFArmatureAsset> armatures = new Dictionary<Mesh, STFArmatureAsset>();
		public Dictionary<GameObject, STFArmatureAsset> nodeArmature = new Dictionary<GameObject, STFArmatureAsset>();
		public Dictionary<GameObject, string> nodeIds = new Dictionary<GameObject, string>();
		public Dictionary<UnityEngine.Object, string> resourceIds = new Dictionary<UnityEngine.Object, string>();
		public Dictionary<string, JObject> nodes = new Dictionary<string, JObject>();
		public Dictionary<string, JObject> resources = new Dictionary<string, JObject>();
		public Dictionary<string, byte[]> buffers = new Dictionary<string, byte[]>();
		private JObject jsonDefinition = new JObject();
		private STFMeta meta = ScriptableObject.CreateInstance<STFMeta>();

		public STFExporter(List<ISTFAssetExporter> assets)
		{
			this.assets = assets;
			context = STFRegistry.GetDefaultExportContext();
			_run();
		}

		public STFExporter(List<ISTFAssetExporter> assets, STFExportContext context)
		{
			this.assets = assets;
			this.context = context;
			if(this.context == null) context = STFRegistry.GetDefaultExportContext();
			_run();
		}
		public STFMeta GetMeta()
		{
			return meta;
		}

		private void _run()
		{
			try
			{
				foreach(var asset in assets)
				{
					asset.Convert(this);
				}
				foreach(var task in registerResourceTasks)
				{
					task.RunSynchronously();
					if(task.Exception != null) throw task.Exception;
				}
				foreach(var task in registerComponentTasks)
				{
					task.RunSynchronously();
					if(task.Exception != null) throw task.Exception;
				}
				foreach(var task in tasks)
				{
					task.RunSynchronously();
					if(task.Exception != null) throw task.Exception;
				}
				jsonDefinition = createRoot();
			} catch(Exception e)
			{
				Debug.Log(e);
				throw e;
			}
		}

		public bool HasArmature(Mesh mesh)
		{
			return armatures.ContainsKey(mesh);
		}

		public STFArmatureAsset GetArmature(Mesh mesh)
		{
			return armatures[mesh];
		}

		public void SetArmature(Mesh mesh, STFArmatureAsset armature)
		{
			if(armatures.ContainsKey(mesh)) armatures[mesh] = armature;
			else armatures.Add(mesh, armature);
		}
		public void RegisterArmatureNode(STFArmatureAsset armature, GameObject go)
		{
			nodeArmature.Add(go, armature);
		}

		public STFExportContext GetContext()
		{
			return context;
		}

		public void AddTask(Task task)
		{
			tasks.Add(task);
		}

		public string RegisterNode(GameObject go, ASTFNodeExporter exporter)
		{
			if(nodeIds.ContainsKey(go)) return nodeIds[go];
			if(nodeArmature.ContainsKey(go))
			{
				// create armature node
			}
			var uuid = go.GetComponent<STFUUID>();
			var id = uuid != null && uuid.id != null && uuid.id.Length > 0 ? uuid.id : Guid.NewGuid().ToString();
			var jnode = exporter.serializeToJson(go, this);
			nodeIds.Add(go, id);
			nodes.Add(id, (JObject)jnode);
			return id;
		}

		public void RegisterResource(UnityEngine.Object unityResource)
		{
			var exporter = context.ResourceExporters[unityResource.GetType()];
			RegisterResource(unityResource, exporter);
			return;// RegisterResource(unityResource, exporter);
		}

		public void RegisterResource(UnityEngine.Object unityResource, ASTFResourceExporter exporter)
		{
			if(resourceIds.ContainsKey(unityResource)) return;// resourceIds[unityResource];
			registerResourceTasks.Add(new Task(() => {
				string id;
				var info = meta.resourceInfo.Find(ri => ri.resource == unityResource);
				if(info != null && info.uuid != null) id = info.uuid;
				else id = Guid.NewGuid().ToString();
				resourceIds.Add(unityResource, id);
				resources.Add(id, (JObject)exporter.serializeToJson(this, unityResource));
			}));
			return;// id;
		}

		private string GetComponentId(Component component)
		{
			string id;
			if(component is ISTFComponent)
			{
				id = ((ISTFComponent)component).id;
				if(id == null || id.Length == 0)
				{
					id = Guid.NewGuid().ToString();
					((ISTFComponent)component).id = id;
				}
			}
			else
			{
				var info = component.GetComponent<STFUUID>();
				if(info != null && info.componentIds != null && info.componentIds.ContainsKey(component))
					id = info.componentIds[component];
				else
					id = Guid.NewGuid().ToString();
			}
			return id;
		}

		private void AddComponentToNode(string nodeId, string componentId, JToken jsonComponent)
		{
			var node = GetNode(nodeId);
			if(node["components"] != null)
				((JObject)node["components"]).Add(componentId, jsonComponent);
			else
				node.Add("components", new JObject(){{componentId, jsonComponent}});
		}

		public void RegisterComponent(string nodeId, Component component)
		{
			if(component.GetType() == typeof(STFUnrecognizedComponent))
			{
				var componentId = GetComponentId(component);
				var jsonComponent = ((STFUnrecognizedComponent)component).serializeToJson(this);
				AddComponentToNode(nodeId, componentId, jsonComponent);
				return;
			}
			RegisterComponent(nodeId, component, context.ComponentExporters[component.GetType()]);
			return;
		}

		public void RegisterComponent(string nodeId, Component component, ASTFComponentExporter exporter)
		{
			registerComponentTasks.Add(new Task(() => {
				string componentId = GetComponentId(component);
				var jsonComponent = exporter.serializeToJson(this, component);
				AddComponentToNode(nodeId, componentId, jsonComponent);
			}));
			return;
		}

		public string RegisterBuffer(byte[] buffer)
		{
			var id = Guid.NewGuid().ToString();
			buffers.Add(id, buffer);
			return id;
		}

		public string GetNodeId(GameObject go)
		{
			return nodeIds[go];
		}

		public JObject GetNode(string id)
		{
			return nodes[id];
		}

		public string GetResourceId(UnityEngine.Object unityResource)
		{
			return resourceIds[unityResource];
		}

		private JObject createRoot()
		{
			var ret = new JObject();
			ret.Add("meta", new JObject() {
					{"version", "0.0.1"},
					{"copyright", "testcopyright"},
					{"generator", "stf-unity"},
					{"author", "testauthor"}
			});
			if(assets != null && assets.Count > 0)
			{
				ret.Add("main", assets[0].GetId(this));
				ret.Add("assets", new JObject(assets.Select(asset => new JProperty(asset.GetId(this), asset.SerializeToJson(this)))));
			}
			ret.Add("nodes", new JObject(nodes.Select(node => new JProperty(node.Key, node.Value))));
			ret.Add("resources", new JObject(resources.Select(resource => new JProperty(resource.Key, resource.Value))));
			ret.Add("buffers", new JArray(buffers.Select(buffer => buffer.Key)));
			return ret;
		}

		public string GetJson()
		{
			return jsonDefinition.ToString(Formatting.None);
		}

		public string GetPrettyJson()
		{
			return jsonDefinition.ToString(Formatting.Indented);
		}

		public byte[] GetBinary()
		{
			byte[] magicUtf8 = Encoding.UTF8.GetBytes(_MAGIC);
			var headerSize = (buffers.Count + 1) * sizeof(int); // +1 for the json definition
			var bufferInfo = buffers.Select(buffer => buffer.Value.Length).ToArray(); // lengths of all binary buffers
			byte[] jsonUtf8 = Encoding.UTF8.GetBytes(GetJson());

			var arrayLen = magicUtf8.Length * sizeof(byte) + sizeof(int) + sizeof(int) + headerSize + jsonUtf8.Length * sizeof(byte);
			foreach(var bufferLen in bufferInfo) arrayLen += bufferLen;

			// handle endianness at some point maybe

			var byteArray = new byte[arrayLen];
			var offset = 0;

			// Magic
			{
				var size = magicUtf8.Length * sizeof(byte);
				Buffer.BlockCopy(magicUtf8, 0, byteArray, offset, size);
				offset += size;
			}

			// Version
			{
				var size = sizeof(int);
				Buffer.BlockCopy(BitConverter.GetBytes(0), 0, byteArray, offset, size);
				offset += size;
			}

			// Header Length
			{
				var size = sizeof(int);
				Buffer.BlockCopy(BitConverter.GetBytes(headerSize), 0, byteArray, offset, size);
				offset += size;
			}

			// Header: array of buffer lengths
			// First the Json definition length
			{
				var size = sizeof(int);
				Buffer.BlockCopy(BitConverter.GetBytes(jsonUtf8.Length), 0, byteArray, offset, size);
				offset += size;
			}

			// Now the array of the lengths of all the binary buffers
			{
				var size = bufferInfo.Length * sizeof(int);
				Buffer.BlockCopy(bufferInfo, 0, byteArray, offset, size);
				offset += size;
			}

			// Json definition
			{
				var size = jsonUtf8.Length * sizeof(byte);
				Buffer.BlockCopy(jsonUtf8, 0, byteArray, offset, size);
				offset += size;
			}

			// Now all the buffers
			foreach(var buffer in buffers)
			{
				var size = buffer.Value.Length * sizeof(byte);
				Buffer.BlockCopy(buffer.Value, 0, byteArray, offset, size);
				offset += size;
			}

			return byteArray;
		}
	}
}