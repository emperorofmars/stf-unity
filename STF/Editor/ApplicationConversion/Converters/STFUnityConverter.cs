
#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using STF.Serialisation;
using STF.Types;
using UnityEngine;
using UnityEngine.Animations;

namespace STF.ApplicationConversion
{
	public class STFUnityConverter : ASTFApplicationConverter
	{
		public const string _TARGET_NAME = "unity3d";
		public override string TargetName => _TARGET_NAME;
		public override STFApplicationConverterContext ConverterContext => new STFApplicationConverterContext {
			NodeComponent = new Dictionary<Type, ISTFNodeComponentApplicationConverter>() {
				{typeof(STFTwistConstraint), new STFTwistConstraintConverter()},
				{typeof(STFResourceHolder), new STFResourceHolderApplicationConverter()},
				{typeof(SkinnedMeshRenderer), new STFMeshInstanceApplicationConverter()},
			},
			Resource = new Dictionary<Type, ISTFResourceApplicationConverter>() {
				{typeof(MTF.Material), new MTFMaterialApplicationConverter()},
				{typeof(AnimationClip), new STFAnimationApplicationConverter()}
			}
		};

		public override List<Type> WhitelistedComponents => new List<Type> {
			typeof(Transform), typeof(Animator), typeof(RotationConstraint), typeof(SkinnedMeshRenderer), typeof(STFResourceHolder)
		};
		public override List<string> Targets => new List<string> {TargetName};

		public override bool CanConvert(ISTFAsset Asset)
		{
			return Asset.Type == STFAsset._TYPE;
		}
	}
}

#endif
