
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;
using VRC.Core;

namespace nna.processors
{
	public class HumanoidMapping : IProcessor
	{
		public const string _Type = "humanoid";
		public string Type => _Type;

		public void Process(NNAContext Context, GameObject Target, GameObject NNANode, JObject Json)
		{
			var locomotionType = (string)ParseUtil.GetMulkikeyOrDefault(Json, "planti", "lt", "locomotion_type");

			var nnaHumanoid = new NNAHumanoidArmatureMapper { LocomotionType = locomotionType };
			nnaHumanoid.Map(Target.GetComponentsInChildren<Transform>());
			var unityAvatar = NNAHumanoidArmatureMapper.GenerateAvatar(nnaHumanoid, Target);
			unityAvatar.name = "Avatar";

			Context.AddObjectToAsset("avatar", unityAvatar);
			Animator animator = Context.Root.GetComponent<Animator>();
			if(!animator) animator = Context.Root.AddComponent<Animator>();

			animator.applyRootMotion = true;
			animator.updateMode = AnimatorUpdateMode.Normal;
			animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
			animator.avatar = unityAvatar;
		}
	}
	
	internal class NNAHumanoidArmatureMapper
	{
		[Serializable] public class BoneMappingPair { public BoneMappingPair(string humanoidName, GameObject bone) { this.humanoidName = humanoidName; this.bone = bone; } public string humanoidName; public GameObject bone; }

		public static readonly Dictionary<string, string> _Translations = new Dictionary<string, string> {
			{"Hip", HumanBodyBones.Hips.ToString()},
			{"Spine", HumanBodyBones.Spine.ToString()},
			{"Chest", HumanBodyBones.Chest.ToString()},
			{"UpperChest", HumanBodyBones.UpperChest.ToString()},
			{"Neck", HumanBodyBones.Neck.ToString()},
			{"Head",  HumanBodyBones.Head.ToString()},
			{"Jaw", HumanBodyBones.Jaw.ToString()},
			{"EyeLeft", HumanBodyBones.LeftEye.ToString()},
			{"EyeRight", HumanBodyBones.RightEye.ToString()},
			{"ShoulderLeft", HumanBodyBones.LeftShoulder.ToString()},
			{"UpperArmLeft", HumanBodyBones.LeftUpperArm.ToString()},
			{"LowerArmLeft", HumanBodyBones.LeftLowerArm.ToString()},
			{"HandLeft", HumanBodyBones.LeftHand.ToString()},
			{"FingerThumb1Left", "Left Thumb Proximal"},//HumanBodyBones.LeftThumbProximal.ToString()},
			{"FingerThumb2Left", "Left Thumb Intermediate"},//, HumanBodyBones.LeftThumbIntermediate.ToString()},
			{"FingerThumb3Left", "Left Thumb Distal"},//, HumanBodyBones.LeftThumbDistal.ToString()},
			{"FingerIndex1Left", "Left Index Proximal"},//, HumanBodyBones.LeftIndexProximal.ToString()},
			{"FingerIndex2Left", "Left Index Intermediate"},//, HumanBodyBones.LeftIndexIntermediate.ToString()},
			{"FingerIndex3Left", "Left Index Distal"},//, HumanBodyBones.LeftIndexDistal.ToString()},
			{"FingerMiddle1Left", "Left Middle Proximal"},//, HumanBodyBones.LeftMiddleProximal.ToString()},
			{"FingerMiddle2Left", "Left Middle Intermediate"},//, HumanBodyBones.LeftMiddleIntermediate.ToString()},
			{"FingerMiddle3Left", "Left Middle Distal"},//, HumanBodyBones.LeftMiddleDistal.ToString()},
			{"FingerRing1Left", "Left Ring Proximal"},//, HumanBodyBones.LeftRingProximal.ToString()},
			{"FingerRing2Left", "Left Ring Intermediate"},//, HumanBodyBones.LeftRingIntermediate.ToString()},
			{"FingerRing3Left", "Left Ring Distal"},//, HumanBodyBones.LeftRingDistal.ToString()},
			{"FingerLittle1Left", "Left Little Proximal"},//, HumanBodyBones.LeftLittleProximal.ToString()},
			{"FingerLittle2Left", "Left Little Intermediate"},//, HumanBodyBones.LeftLittleIntermediate.ToString()},
			{"FingerLittle3Left", "Left Little Distal"},//, HumanBodyBones.LeftLittleDistal.ToString()},
			{"ShoulderRight", HumanBodyBones.RightShoulder.ToString()},
			{"UpperArmRight", HumanBodyBones.RightUpperArm.ToString()},
			{"LowerArmRight", HumanBodyBones.RightLowerArm.ToString()},
			{"HandRight", HumanBodyBones.RightHand.ToString()},
			{"FingerThumb1Right", "Right Thumb Proximal"},//, HumanBodyBones.RightThumbProximal.ToString()},
			{"FingerThumb2Right", "Right Thumb Intermediate"},//, HumanBodyBones.RightThumbIntermediate.ToString()},
			{"FingerThumb3Right", "Right Thumb Distal"},//, HumanBodyBones.RightThumbDistal.ToString()},
			{"FingerIndex1Right", "Right Index Proximal"},//, HumanBodyBones.RightIndexProximal.ToString()},
			{"FingerIndex2Right", "Right Index Intermediate"},//, HumanBodyBones.RightIndexIntermediate.ToString()},
			{"FingerIndex3Right", "Right Index Distal"},//, HumanBodyBones.RightIndexDistal.ToString()},
			{"FingerMiddle1Right", "Right Middle Proximal"},//, HumanBodyBones.RightMiddleProximal.ToString()},
			{"FingerMiddle2Right", "Right Middle Intermediate"},//, HumanBodyBones.RightMiddleIntermediate.ToString()},
			{"FingerMiddle3Right", "Right Middle Distal"},//, HumanBodyBones.RightMiddleDistal.ToString()},
			{"FingerRing1Right", "Right Ring Proximal"},//, HumanBodyBones.RightRingProximal.ToString()},
			{"FingerRing2Right", "Right Ring Intermediate"},//, HumanBodyBones.RightRingIntermediate.ToString()},
			{"FingerRing3Right", "Right Ring Distal"},//, HumanBodyBones.RightRingDistal.ToString()},
			{"FingerLittle1Right", "Right Little Proximal"},//, HumanBodyBones.RightLittleProximal.ToString()},
			{"FingerLittle2Right", "Right Little Intermediate"},//, HumanBodyBones.RightLittleIntermediate.ToString()},
			{"FingerLittle3Right", "Right Little Distal"},//, HumanBodyBones.RightLittleDistal.ToString()},
			{"UpperLegLeft", HumanBodyBones.LeftUpperLeg.ToString()},
			{"LowerLegLeft", HumanBodyBones.LeftLowerLeg.ToString()},
			{"FootLeft", HumanBodyBones.LeftFoot.ToString()},
			{"ToesLeft", HumanBodyBones.LeftToes.ToString()},
			{"UpperLegRight", HumanBodyBones.RightUpperLeg.ToString()},
			{"LowerLegRight", HumanBodyBones.RightLowerLeg.ToString()},
			{"FootRight", HumanBodyBones.RightFoot.ToString()},
			{"ToesRight", HumanBodyBones.RightToes.ToString()}
		};

		public static readonly List<string> _MappingsLeftList = new List<string>{"left", "_l", ".l", "-l"};
		public static readonly List<string> _MappingsRightList = new List<string>{"right", "_r", ".r", "-r"};
		public static readonly Dictionary<string, List<List<string>>> NameMappings = new Dictionary<string, List<List<string>>> {
			{"Hip", new List<List<string>>{new List<string>{"hip", "hips"}}},
			{"Spine", new List<List<string>>{new List<string>{"spine"}}},
			{"Chest", new List<List<string>>{new List<string>{"chest"}}},
			{"UpperChest", new List<List<string>>{new List<string>{"upper"}, new List<string>{"chest"}}},
			{"Neck", new List<List<string>>{new List<string>{"neck"}}},
			{"Head", new List<List<string>>{new List<string>{"head"}}},
			{"Jaw", new List<List<string>>{new List<string>{"jaw"}}},
			{"EyeLeft", new List<List<string>>{new List<string>{"eye"}, _MappingsLeftList}},
			{"EyeRight", new List<List<string>>{new List<string>{"eye"}, _MappingsRightList}},
			{"ShoulderLeft", new List<List<string>>{new List<string>{"shoulder"}, _MappingsLeftList}},
			{"UpperArmLeft", new List<List<string>>{new List<string>{"upper"}, new List<string>{"arm"}, _MappingsLeftList}},
			{"LowerArmLeft", new List<List<string>>{new List<string>{"lower"}, new List<string>{"arm"}, _MappingsLeftList}},
			{"HandLeft", new List<List<string>>{new List<string>{"hand", "wrist"}, _MappingsLeftList}},
			{"FingerThumb1Left", new List<List<string>>{new List<string>{"thumb"}, new List<string>{"1", "proximal"}, _MappingsLeftList}},
			{"FingerThumb2Left", new List<List<string>>{new List<string>{"thumb"}, new List<string>{"2", "intermediate"}, _MappingsLeftList}},
			{"FingerThumb3Left", new List<List<string>>{new List<string>{"thumb"}, new List<string>{"3", "distal"}, _MappingsLeftList}},
			{"FingerIndex1Left", new List<List<string>>{new List<string>{"index"}, new List<string>{"1", "proximal"}, _MappingsLeftList}},
			{"FingerIndex2Left", new List<List<string>>{new List<string>{"index"}, new List<string>{"2", "intermediate"}, _MappingsLeftList}},
			{"FingerIndex3Left", new List<List<string>>{new List<string>{"index"}, new List<string>{"3", "distal"}, _MappingsLeftList}},
			{"FingerMiddle1Left", new List<List<string>>{new List<string>{"middle"}, new List<string>{"1", "proximal"}, _MappingsLeftList}},
			{"FingerMiddle2Left", new List<List<string>>{new List<string>{"middle"}, new List<string>{"2", "intermediate"}, _MappingsLeftList}},
			{"FingerMiddle3Left", new List<List<string>>{new List<string>{"middle"}, new List<string>{"3", "distal"}, _MappingsLeftList}},
			{"FingerRing1Left", new List<List<string>>{new List<string>{"ring"}, new List<string>{"1", "proximal"}, _MappingsLeftList}},
			{"FingerRing2Left", new List<List<string>>{new List<string>{"ring"}, new List<string>{"2", "intermediate"}, _MappingsLeftList}},
			{"FingerRing3Left", new List<List<string>>{new List<string>{"ring"}, new List<string>{"3", "distal"}, _MappingsLeftList}},
			{"FingerLittle1Left", new List<List<string>>{new List<string>{"little", "pinkie"}, new List<string>{"1", "proximal"}, _MappingsLeftList}},
			{"FingerLittle2Left", new List<List<string>>{new List<string>{"little", "pinkie"}, new List<string>{"2", "intermediate"}, _MappingsLeftList}},
			{"FingerLittle3Left", new List<List<string>>{new List<string>{"little", "pinkie"}, new List<string>{"3", "distal"}, _MappingsLeftList}},
			{"ShoulderRight", new List<List<string>>{new List<string>{"shoulder"}, _MappingsRightList}},
			{"UpperArmRight", new List<List<string>>{new List<string>{"upper"}, new List<string>{"arm"}, _MappingsRightList}},
			{"LowerArmRight", new List<List<string>>{new List<string>{"lower"}, new List<string>{"arm"}, _MappingsRightList}},
			{"HandRight", new List<List<string>>{new List<string>{"hand", "wrist"}, _MappingsRightList}},
			{"FingerThumb1Right", new List<List<string>>{new List<string>{"thumb"}, new List<string>{"1", "proximal"}, _MappingsRightList}},
			{"FingerThumb2Right", new List<List<string>>{new List<string>{"thumb"}, new List<string>{"2", "intermediate"}, _MappingsRightList}},
			{"FingerThumb3Right", new List<List<string>>{new List<string>{"thumb"}, new List<string>{"3", "distal"}, _MappingsRightList}},
			{"FingerIndex1Right", new List<List<string>>{new List<string>{"index"}, new List<string>{"1", "proximal"}, _MappingsRightList}},
			{"FingerIndex2Right", new List<List<string>>{new List<string>{"index"}, new List<string>{"2", "intermediate"}, _MappingsRightList}},
			{"FingerIndex3Right", new List<List<string>>{new List<string>{"index"}, new List<string>{"3", "distal"}, _MappingsRightList}},
			{"FingerMiddle1Right", new List<List<string>>{new List<string>{"middle"}, new List<string>{"1", "proximal"}, _MappingsRightList}},
			{"FingerMiddle2Right", new List<List<string>>{new List<string>{"middle"}, new List<string>{"2", "intermediate"}, _MappingsRightList}},
			{"FingerMiddle3Right", new List<List<string>>{new List<string>{"middle"}, new List<string>{"3", "distal"}, _MappingsRightList}},
			{"FingerRing1Right", new List<List<string>>{new List<string>{"ring"}, new List<string>{"1", "proximal"}, _MappingsRightList}},
			{"FingerRing2Right", new List<List<string>>{new List<string>{"ring"}, new List<string>{"2", "intermediate"}, _MappingsRightList}},
			{"FingerRing3Right", new List<List<string>>{new List<string>{"ring"}, new List<string>{"3", "distal"}, _MappingsRightList}},
			{"FingerLittle1Right", new List<List<string>>{new List<string>{"little", "pinkie"}, new List<string>{"1", "proximal"}, _MappingsRightList}},
			{"FingerLittle2Right", new List<List<string>>{new List<string>{"little", "pinkie"}, new List<string>{"2", "intermediate"}, _MappingsRightList}},
			{"FingerLittle3Right", new List<List<string>>{new List<string>{"little", "pinkie"}, new List<string>{"3", "distal"}, _MappingsRightList}},
			{"UpperLegLeft", new List<List<string>>{new List<string>{"upper"}, new List<string>{"leg"}, _MappingsLeftList}},
			{"LowerLegLeft", new List<List<string>>{new List<string>{"lower"}, new List<string>{"leg"}, _MappingsLeftList}},
			{"FootLeft", new List<List<string>>{new List<string>{"foot"}, _MappingsLeftList}},
			{"ToesLeft", new List<List<string>>{new List<string>{"toes"}, _MappingsLeftList}},
			{"UpperLegRight", new List<List<string>>{new List<string>{"upper"}, new List<string>{"leg"}, _MappingsRightList}},
			{"LowerLegRight", new List<List<string>>{new List<string>{"lower"}, new List<string>{"leg"}, _MappingsRightList}},
			{"FootRight", new List<List<string>>{new List<string>{"foot"}, _MappingsRightList}},
			{"ToesRight", new List<List<string>>{new List<string>{"toes"}, _MappingsRightList}}
		};

		public string LocomotionType;
		public List<BoneMappingPair> Mappings = new List<BoneMappingPair>();
		public Avatar GeneratedAvatar;

		public static string translateHumanoidSTFtoUnity(string STFName, string LocomotionType)
		{
			if(LocomotionType == "digi")
			{
				switch(STFName)
				{
					case "ToesLeft":
						return _Translations["FootLeft"];
					case "ToesRight":
						return _Translations["FootRight"];
					case "FootLeft":
						return null;
					case "FootRight":
						return null;
				}
			}
			if(STFName == "Jaw") return null;
			return _Translations[STFName];
		}
		
		public void Map(Transform[] Bones)
		{
			var tmpMappings = new Dictionary<string, GameObject>();
			foreach(var bone in Bones)
			{
				foreach(var mapping in NameMappings)
				{
					var and_list = mapping.Value;
					var and_condition = true;
					foreach(var or_list in and_list)
					{
						var or_condition = false;
						foreach(var or_arg in or_list)
						{
							if(bone.name.ToLower().Contains(or_arg))
							{
								or_condition = true;
								break;
							}
						}
						if(!or_condition)
						{
							and_condition = false;
						}
					}
					if(and_condition)
					{
						if(tmpMappings.ContainsKey(mapping.Key))
						{
							if(tmpMappings[mapping.Key].name.Length > bone.name.Length)
							{
								tmpMappings[mapping.Key] = bone.gameObject;
							}
						}
						else
						{
							tmpMappings.Add(mapping.Key, bone.gameObject);
						}
					}
				}
			}
			this.Mappings = tmpMappings.Select(m => new BoneMappingPair(m.Key, m.Value)).ToList();
		}

		public static Avatar GenerateAvatar(NNAHumanoidArmatureMapper stfComponent, GameObject ArmatureNode)
		{
			try
			{
				var mappings = stfComponent.Mappings
						.FindAll(mapping => !String.IsNullOrWhiteSpace(mapping.humanoidName) && mapping.bone != null)
						.Select(mapping => new KeyValuePair<string, GameObject>(translateHumanoidSTFtoUnity(mapping.humanoidName, stfComponent.LocomotionType), mapping.bone))
						.Where(mapping => !String.IsNullOrWhiteSpace(mapping.Key)).ToList();
				
				var humanDescription = new HumanDescription
				{
					armStretch = 0.05f,
					feetSpacing = 0f,
					hasTranslationDoF = false,
					legStretch = 0.05f,
					lowerArmTwist = 0.5f,
					lowerLegTwist = 0.5f,
					upperArmTwist = 0.5f,
					upperLegTwist = 0.5f,
					skeleton = ArmatureNode.GetComponentsInChildren<Transform>().Select(t => {
						return new SkeletonBone()
						{
							name = t.name,
							position = t.localPosition,
							rotation = t.localRotation,
							scale = t.localScale
						};
					}).ToArray(),
					human = mappings.Select(mapping => 
					{
						var bone = new HumanBone {
							humanName = mapping.Key,
							boneName = mapping.Value.name,
							limit = new HumanLimit {useDefaultValues = true}
						};
						return bone;
					}).ToArray()
				};

				var avatar = AvatarBuilder.BuildHumanAvatar(ArmatureNode, humanDescription);
				avatar.name = ArmatureNode.name + "Avatar";

				if (!avatar.isValid)
				{
					throw new Exception("Invalid humanoid avatar");
				}
				return avatar;
			}
			catch(Exception e)
			{
				Debug.Log(e);
				return null;
			}
		}
	}
}
