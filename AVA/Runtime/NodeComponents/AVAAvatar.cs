using System;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System.Threading.Tasks;
using STF.Serialisation;
using STF.Util;
using VRC;



#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AVA.Serialisation
{
	public class AVAAvatar : ASTFNodeComponent
	{
		public const string _TYPE = "AVA.avatar";
		public override string Type => _TYPE;
		public GameObject viewport_parent;
		public Vector3 viewport_position;
		public SkinnedMeshRenderer MainMesh;
	}

	public class AVAAvatarExporter : ASTFNodeComponentExporter
	{
		public override string ConvertPropertyPath(string UnityProperty)
		{
			throw new NotImplementedException();
		}

		public override (string, JObject) SerializeToJson(ISTFExportState State, Component Component)
		{
			var c = (AVAAvatar)Component;
			var ret = new JObject();
			State.AddTask(new Task(() => {
				ret.Add("viewport_parent", State.Nodes[c.viewport_parent].Id);
				ret.Add("viewport_position", new JArray() {c.viewport_position.x, c.viewport_position.y, c.viewport_position.z});
			}));
			return (c.Id, ret);
		}
	}

	public class AVAAvatarImporter : ASTFNodeComponentImporter
	{
		public override string ConvertPropertyPath(string STFProperty)
		{
			throw new NotImplementedException();
		}

		public override void ParseFromJson(ISTFAssetImportState State, JObject Json, string Id, GameObject Go)
		{
			var c = Go.AddComponent<AVAAvatar>();
			State.AddTask(new Task(() => {
				c.viewport_parent = State.Nodes[(string)Json["viewport_parent"]];
				c.viewport_position = TRSUtil.ParseLocation((JArray)Json["viewport_position"]);
			}));
			State.AddComponent(c, Id);
		}
	}

#if UNITY_EDITOR
	[InitializeOnLoad]
	public class Register_AVAAvatar
	{
		static Register_AVAAvatar()
		{
			STFRegistry.RegisterNodeComponentImporter(AVAAvatar._TYPE, new AVAAvatarImporter());
			STFRegistry.RegisterNodeComponentExporter(typeof(AVAAvatar), new AVAAvatarExporter());
		}
	}
#endif

}