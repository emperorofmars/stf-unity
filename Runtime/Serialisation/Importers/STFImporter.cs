
using UnityEngine;
using System.Collections.Generic;
using stf.Components;
using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Text;

namespace stf.serialisation
{
	public class STFImporter : ISTFImporter
	{
		private STFImportContext context;
		public string mainAssetId;
		public Dictionary<string, ISTFAsset> assets = new Dictionary<string, ISTFAsset>();
		public Dictionary<string, string> assetNodes = new Dictionary<string, string>();
		public Dictionary<string, GameObject> nodes = new Dictionary<string, GameObject>();
		public Dictionary<string, UnityEngine.Object> resources = new Dictionary<string, UnityEngine.Object>();
		public Dictionary<string, Component> components = new Dictionary<string, Component>();
		public Dictionary<string, byte[]> buffers = new Dictionary<string, byte[]>();
		private List<UnityEngine.Object> trash = new List<UnityEngine.Object>();
		private List<Task> tasks = new List<Task>();
		private List<Task> postprocessTasks = new List<Task>();
		//private List<Task> componentTasks = new List<Task>();
		public STFMeta meta = ScriptableObject.CreateInstance<STFMeta>();
		public ISTFSecondStage nextStage;

		public STFImporter(JObject jsonRoot)
		{
			this.context = STFRegistry.GetDefaultImportContext();
			parse(jsonRoot);
		}

		public STFImporter(byte[] byteArray)
		{
			this.context = STFRegistry.GetDefaultImportContext();
			parse(byteArray);
		}

		public STFImporter(JObject jsonRoot, STFImportContext context)
		{
			this.context = context;
			if(this.context == null) STFRegistry.GetDefaultImportContext();
			parse(jsonRoot);
		}

		public STFImporter(byte[] byteArray, STFImportContext context)
		{
			this.context = context;
			if(this.context == null) STFRegistry.GetDefaultImportContext();
			parse(byteArray);
		}

		public STFImportContext GetContext()
		{
			return context;
		}

		public STFMeta GetMeta()
		{
			return meta;
		}

		public void AddTask(Task task)
		{
			tasks.Add(task);
		}

		public void AddPostprocessTask(Task task)
		{
			postprocessTasks.Add(task);
		}

		public GameObject GetNode(string id)
		{
			if(nodes.ContainsKey(id)) return nodes[id];
			else return null;
		}
		
		public string GetMainAssetId()
		{
			return this.mainAssetId;
		}

		public Dictionary<string, ISTFAsset> GetAssets()
		{
			return assets;
		}

		public UnityEngine.Object GetResource(string id)
		{
			if(resources.ContainsKey(id)) return resources[id];
			else return null;
		}

		public List<UnityEngine.Object> GetResources()
		{
			return resources.Values.ToList();
		}

		public Component GetComponent(string id)
		{
			if(components.ContainsKey(id)) return components[id];
			else return null;
		}

		public byte[] GetBuffer(string id)
		{
			return buffers[id];
		}

		public void AddNode(string id, GameObject go)
		{
			this.nodes.Add(id, go);
		}

		public void AddResources(string id, UnityEngine.Object resource)
		{
			this.resources.Add(id, resource);
		}
		
		public void AddComponent(string id, Component component)
		{
			this.components.Add(id, component);
		}

		public void AddTrashObject(UnityEngine.Object trash)
		{
			this.trash.Add(trash);
		}

		private void _runTasks()
		{
			do
			{
				var currentTasks = tasks;
				tasks = new List<Task>();
				foreach(var task in currentTasks)
				{
					task.RunSynchronously();
					if(task.Exception != null) throw task.Exception;
				}
			}
			while(tasks.Count > 0);
		}

		public void parse(JObject jsonRoot)
		{
			try
			{
				mainAssetId = (string)jsonRoot["main"];
				meta.mainAssetId = mainAssetId;
				meta.versionDefinition = (string)jsonRoot["meta"]["version"];
				meta.copyright = (string)jsonRoot["meta"]["copyright"];
				meta.generator = (string)jsonRoot["meta"]["generator"];
				meta.author = (string)jsonRoot["meta"]["author"];

				foreach(var jsonResource in ((JObject)jsonRoot["resources"]))
				{
					if((string)jsonResource.Value["type"] != null && context.ResourceImporters.ContainsKey((string)jsonResource.Value["type"]))
					{
						var resourceImporter = context.ResourceImporters[(string)jsonResource.Value["type"]];
						var resource = resourceImporter.ParseFromJson(this, jsonResource.Value, jsonResource.Key, jsonRoot);
						resources.Add(jsonResource.Key, resource);
					}
					else
					{
						Debug.LogWarning($"Skipping Unrecognized Resource: {(string)jsonResource.Value["type"]}");
					}
				}
				foreach(var jsonAsset in ((JObject)jsonRoot["assets"]))
				{
					if(!context.AssetImporters.ContainsKey((string)jsonAsset.Value["type"]))
						throw new Exception("Assettype '" + (string)jsonAsset.Value["type"] + "' is not supported");

					var asset = context.AssetImporters[(string)jsonAsset.Value["type"]].ParseFromJson(this, jsonAsset.Value, jsonAsset.Key, jsonRoot);
					assets.Add(jsonAsset.Key, asset);
				}
				foreach(var jsonNode in ((JObject)jsonRoot["nodes"]))
				{
					var go = nodes[jsonNode.Key];

					if(go.GetComponent<STFUUID>() == null)
					{
						var uuidComponent = go.AddComponent<STFUUID>();
						uuidComponent.id = jsonNode.Key;
					}
				}
				_runTasks();
				tasks = postprocessTasks;
				_runTasks();
			} catch(Exception e)
			{
				foreach(var node in nodes.Values)
				{
					if(node != null)
					{
						#if UNITY_EDITOR
							UnityEngine.Object.DestroyImmediate(node);
						#else
							UnityEngine.Object.Destroy(node);
						#endif
					}
				}
				throw new Exception("Error during STF import: ", e);
			} finally
			{
				foreach(var trashObject in trash)
				{
					if(trashObject != null)
					{
						#if UNITY_EDITOR
							UnityEngine.Object.DestroyImmediate(trashObject);
						#else
							UnityEngine.Object.Destroy(trashObject);
						#endif
					}
				}
			}
			foreach(var asset in assets)
			{
				meta.importedRawAssets.Add(new STFMeta.AssetInfo {assetId = asset.Key, assetType = asset.Value.GetSTFAssetType(), assetName = asset.Value.GetSTFAssetName(), assetRoot = asset.Value.GetAsset()});
			}
		}

		public void parse(byte[] byteArray)
		{
			var offset = 0;

			// Magic
			int magicLen = Encoding.UTF8.GetBytes(STFExporter._MAGIC).Length;
			var magicUtf8 = new byte[magicLen];
			Buffer.BlockCopy(byteArray, 0, magicUtf8, 0, magicUtf8.Length * sizeof(byte));
			offset += magicUtf8.Length * sizeof(byte);

			var magic = Encoding.UTF8.GetString(magicUtf8);
			if(magic != STFExporter._MAGIC)
				throw new Exception("Not an STF file, invalid magic number.");
			
			// Version
			int versionMayor = BitConverter.ToInt32(byteArray, offset);
			offset += sizeof(int);
			int versionMinor = BitConverter.ToInt32(byteArray, offset);
			offset += sizeof(int);
			meta.versionBinary = versionMayor + "." + versionMinor;
			
			// Header Length
			int headerLen = BitConverter.ToInt32(byteArray, offset);
			offset += sizeof(int);

			// Buffer Lengths
			var bufferLengths = new int[headerLen / sizeof(int)];
			for(int i = 0; i < headerLen / sizeof(int); i++)
			{
				bufferLengths[i] = BitConverter.ToInt32(byteArray, offset);
				offset += sizeof(int);
			}

			// Validity Check
			if(bufferLengths.Length < 1)
				throw new Exception("Invalid file: At least one buffer needed.");
			var totalLengthCheck = offset;
			foreach(var l in bufferLengths) totalLengthCheck += l;
			if(totalLengthCheck != byteArray.Length)
				throw new Exception("Invalid file: Size of buffers doesn't line up with total file size. ( calculated: " + totalLengthCheck + " | actual: " + byteArray.Length + " )");

			// First buffer, the Json definition
			var json = Encoding.UTF8.GetString(byteArray, offset, bufferLengths[0]);
			offset += bufferLengths[0];

			JObject jsonRoot = JObject.Parse(json);
			if(((JArray)jsonRoot["buffers"]).Count() != bufferLengths.Count() - 1)
				throw new Exception("Invalid file: Mismatching number of defined buffers and actual buffer count.");

			for(int i = 1; i < bufferLengths.Count(); i++)
			{
				var buffer = new byte[bufferLengths[i]];
				Buffer.BlockCopy(byteArray, offset, buffer, 0, bufferLengths[i]);
				offset += bufferLengths[i];
				buffers.Add((string)jsonRoot["buffers"][i - 1], buffer);
			}

			parse(jsonRoot);
		}
	}
}