using System;
using System.Collections.Generic;
using STF.ApplicationConversion;
using STF.Serialisation;
using UnityEngine;
using UnityEngine.Animations;

namespace AVA.ApplicationConversion
{
	public class AVAVrchatConverter : ASTFApplicationConverter
	{
		public const string _TARGET_NAME = "unity3d";
		public override string TargetName => _TARGET_NAME;

		public override Dictionary<Type, ISTFNodeComponentApplicationConverter> Converters => new Dictionary<Type, ISTFNodeComponentApplicationConverter>() {
			{typeof(STFTwistConstraint), new STFTwistConstraintConverter()},
		};
		public override List<Type> WhitelistedComponents => new List<Type> {
			typeof(Transform), typeof(Animator), typeof(RotationConstraint), typeof(SkinnedMeshRenderer)
		};
		public override List<string> Targets => new List<string> {TargetName};

		public override bool CanConvert(ISTFAsset Asset)
		{
			return Asset.Type == STFAsset._TYPE;
		}
	}
}