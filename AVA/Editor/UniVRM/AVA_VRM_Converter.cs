#if UNITY_EDITOR
#if AVA_UNIVRM_FOUND

using System;
using System.Collections.Generic;
using AVA.Types;
using STF.ApplicationConversion;
using STF.Serialisation;
using STF.Types;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using VRM;

namespace AVA.ApplicationConversion
{
	public class AVA_VRM_Converter : ASTFApplicationConverter
	{
		public const string _TARGET_NAME = "vrm";
		public override string TargetName => _TARGET_NAME;
		public override STFApplicationConverterContext ConverterContext => new()
		{
			NodeComponent = new Dictionary<Type, ISTFNodeComponentApplicationConverter>() {
				{typeof(STFTwistConstraint), new STFTwistConstraintConverter()},
				{typeof(AVAAvatar), new AVA_VRM_AvatarConverter()},
				{typeof(AVAEyeBoneLimitsSimple), new AVA_VRM_EyeBoneLimitsSimpleConverter()},
				{typeof(SkinnedMeshRenderer), new STFMeshInstanceApplicationConverter()},
				{typeof(AVAJankyFallbackPhysics), new AVA_VRM_JankyFallbackPhysicsConverter()},
			},
			Resource = new Dictionary<Type, ISTFResourceApplicationConverter>() {
				{typeof(MTF.Material), new MTFMaterialApplicationConverter()},
				{typeof(AnimationClip), new STFAnimationApplicationConverter()}
			}
		};
		public override List<Type> WhitelistedComponents => new()
		{
			typeof(Transform), typeof(Animator), typeof(RotationConstraint), typeof(SkinnedMeshRenderer), typeof(MeshFilter), typeof(MeshRenderer), typeof(VRMMeta), typeof(VRMLookAtHead),
			typeof(VRMHumanoidDescription), typeof(VRMFirstPerson), typeof(VRMLookAtBoneApplyer), typeof(VRMBlendShapeProxy), typeof(VRMSpringBone)
		};
		public override List<string> Targets => new() { TargetName, "gltf" };

		public override bool CanConvert(ISTFAsset Asset)
		{
			return Asset.Type == STFAsset._TYPE && Asset.GetComponent<AVAAvatar>() != null;
		}
	}

	[InitializeOnLoad]
	public class Register_AVA_VRM_Converter
	{
		static Register_AVA_VRM_Converter()
		{
			STFRegistry.RegisterApplicationConverter(new AVA_VRM_Converter());
		}
	}
}

#endif
#endif
