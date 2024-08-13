using System;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System.Threading.Tasks;
using STF.Serialisation;
using STF.Util;
using System.Linq;
using STF.Types;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AVA.Types
{
	public class AVAAvatar : ISTFNodeComponent
	{
		public const string _TYPE = "AVA.avatar";
		public override string Type => _TYPE;
		public NodeComponentReference<STFMeshInstance> MainMeshInstance = new();
		public NodeReference viewport_parent = new();
		public Vector3 viewport_position = Vector3.zero;

		public void TrySetup()
		{
			var meshInstances = GetComponentsInChildren<STFMeshInstance>();
			if(meshInstances.Count() == 1) MainMeshInstance = (NodeComponentReference)meshInstances[0];
			else MainMeshInstance = (NodeComponentReference)meshInstances.FirstOrDefault(m => m.name.ToLower().Contains("body"));
			
			if(MainMeshInstance.IsRef)
			{
				var humanoidDefinition = TryGetHumanoidDefinition();
				if(humanoidDefinition) SetupViewport(humanoidDefinition);
			}
		}

		public STFHumanoidArmature TryGetHumanoidDefinition()
		{
			return (STFHumanoidArmature)MainMeshInstance.GetRef()?.ArmatureInstance.GetRef()?.Armature.Resource?.Components?.FirstOrDefault(comp => comp.Type == STFHumanoidArmature._TYPE);
		}

		public GameObject FindBoneInstance(STFHumanoidArmature HumanoidDefinition, string HumanoidBoneName)
		{
			var humanoidBone = HumanoidDefinition.Mappings.Find(m => m.humanoidName == HumanoidBoneName)?.bone;
			return humanoidBone != null ?
				MainMeshInstance.GetRef()?.ArmatureInstance.GetRef()?.Bones.Find(b => b.GetComponent<STFBoneInstanceNode>().BoneId == humanoidBone?.GetComponent<STFBoneNode>().Id)
				: null;
		}

		public void SetupViewport(STFHumanoidArmature HumanoidDefinition)
		{
			viewport_parent = new NodeReference(FindBoneInstance(HumanoidDefinition, "Head"));
			var eyeLeft = FindBoneInstance(HumanoidDefinition, "EyeLeft");
			var eyeRight = FindBoneInstance(HumanoidDefinition, "EyeRight");
			if(eyeLeft && eyeRight)
			{
				viewport_position = ((eyeLeft.transform.position + eyeRight.transform.position) / 2) - viewport_parent.Node.transform.position;
				viewport_position.x = Math.Abs(viewport_position.x) < 0.0001 ? 0 : viewport_position.x;
				viewport_position.y = Math.Abs(viewport_position.y) < 0.0001 ? 0 : viewport_position.y;
				viewport_position.z = Math.Abs(viewport_position.z) < 0.0001 ? 0 : viewport_position.z;
			}
			else
			{
				viewport_position = Vector3.zero;
			}
		}
	}

	public class AVAAvatarExporter : ASTFNodeComponentExporter
	{
		public override string ConvertPropertyPath(STFExportState State, Component Component, string UnityProperty)
		{
			throw new NotImplementedException();
		}

		public override (string, JObject) SerializeToJson(STFExportState State, Component Component)
		{
			var c = (AVAAvatar)Component;
			var ret = new JObject() {
				{"type", AVAAvatar._TYPE}
			};
			var rf = new RefSerializer(ret);
			SerializeRelationships(c, ret);
			State.AddTask(new Task(() => {
				if(c.MainMeshInstance.IsId) ret.Add("main_mesh", rf.NodeComponentRef(c.MainMeshInstance.Id));
				ret.Add("viewport_parent", c.viewport_parent != null ? rf.NodeRef(c.viewport_parent.Id) : null);
				ret.Add("viewport_position", new JArray() {c.viewport_position.x, c.viewport_position.y, c.viewport_position.z});
			}));
			return (c.Id, ret);
		}
	}

	public class AVAAvatarImporter : ASTFNodeComponentImporter
	{
		public override string ConvertPropertyPath(STFImportState State, Component Component, string STFProperty)
		{
			throw new NotImplementedException();
		}

		public override void ParseFromJson(STFImportState State, JObject Json, string Id, GameObject Go)
		{
			var rf = new RefDeserializer(Json);
			var c = Go.AddComponent<AVAAvatar>();
			c.Id = Id;
			ParseRelationships(Json, c);
			State.AddTask(new Task(() => {
				c.MainMeshInstance = (NodeComponentReference)(Json["main_mesh"] != null ? (STFMeshInstance)State.NodeComponents[rf.NodeComponentRef(Json["main_mesh"])] : null);
				c.viewport_parent = State.GetNodeReference(rf.NodeRef(Json["viewport_parent"]));
				c.viewport_position = TRSUtil.ParseLocation((JArray)Json["viewport_position"]);
			}));
			State.AddNodeComponent(c);
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