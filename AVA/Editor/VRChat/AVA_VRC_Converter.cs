#if UNITY_EDITOR
#if AVA_VRCSDK3_FOUND

using System;
using System.Collections.Generic;
using AVA.Serialisation;
using STF.ApplicationConversion;
using STF.Serialisation;
using UnityEngine;
using UnityEngine.Animations;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace AVA.ApplicationConversion
{
	public class AVA_VRC_Converter : ASTFApplicationConverter
	{
		public const string _TARGET_NAME = "vrchat_sdk3";
		public override string TargetName => _TARGET_NAME;

		public override Dictionary<Type, ISTFNodeComponentApplicationConverter> NodeComponentConverters => new Dictionary<Type, ISTFNodeComponentApplicationConverter>() {
			{typeof(STFTwistConstraint), new STFTwistConstraintConverter()},
			{typeof(AVAAvatar), new AVA_VRC_AvatarConverter()},
			{typeof(AVAEyeBoneLimitsSimple), new AVA_VRC_EyeBoneLimitsSimpleConverter()},
			{typeof(STFResourceHolder), new STFResourceHolderApplicationConverter()},
		};
		public override Dictionary<Type, ISTFResourceApplicationConverter> ResourceConverters => new Dictionary<Type, ISTFResourceApplicationConverter>() {
			{typeof(MTF.Material), new MTFMaterialApplicationConverter()},
			{typeof(AnimationClip), new STFAnimationApplicationConverter()}
		};
		public override List<Type> WhitelistedComponents => new List<Type> {
			typeof(Transform), typeof(Animator), typeof(RotationConstraint), typeof(SkinnedMeshRenderer), typeof(VRCAvatarDescriptor), typeof(VRCPipelineManagerEditor), typeof(VRCPhysBone)
		};
		public override List<string> Targets => new List<string> {TargetName};

		public override bool CanConvert(ISTFAsset Asset)
		{
			return Asset.Type == STFAsset._TYPE && Asset.GetComponent<AVAAvatar>() != null;
		}
	}
}

#endif
#endif
