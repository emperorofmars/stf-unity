using System;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System.Threading.Tasks;
using STF.Serialisation;
using STF.Util;
using System.Linq;


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
		public STFMeshInstance MainMesh;

		public void TrySetup()
		{
			var meshInstances = GetComponentsInChildren<STFMeshInstance>();
			if(meshInstances.Count() == 1) MainMesh = meshInstances[0];
			else MainMesh = meshInstances.FirstOrDefault(m => m.name.ToLower().Contains("body"));
			
			if(MainMesh != null)
			{
				var humanoidDefinition = TryGetHumanoidDefinition();
				SetupViewport(humanoidDefinition);
			}
		}

		public STFHumanoidArmature TryGetHumanoidDefinition()
		{
			return (STFHumanoidArmature)MainMesh?.ArmatureInstance?.armature?.Components?.FirstOrDefault(comp => comp.Type == STFHumanoidArmature._TYPE);
		}

		public GameObject FindBoneInstance(STFHumanoidArmature HumanoidDefinition, string HumanoidBone)
		{
			var humanoidBone = HumanoidDefinition.Mappings.Find(m => m.humanoidName == HumanoidBone)?.bone;
			return MainMesh?.ArmatureInstance.bones.Find(b => b.GetComponent<STFBoneInstanceNode>().BoneId == humanoidBone?.GetComponent<STFBoneNode>().Id);
		}

		public void SetupViewport(STFHumanoidArmature HumanoidDefinition)
		{
			viewport_parent = FindBoneInstance(HumanoidDefinition, "Head");
			var eyeLeft = FindBoneInstance(HumanoidDefinition, "EyeLeft");
			var eyeRight = FindBoneInstance(HumanoidDefinition, "EyeRight");

			viewport_position = ((eyeLeft.transform.position + eyeRight.transform.position) / 2) - viewport_parent.transform.position;
			viewport_position.x = Math.Abs(viewport_position.x) < 0.0001 ? 0 : viewport_position.x;
			viewport_position.y = Math.Abs(viewport_position.y) < 0.0001 ? 0 : viewport_position.y;
			viewport_position.z = Math.Abs(viewport_position.z) < 0.0001 ? 0 : viewport_position.z;
		}
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