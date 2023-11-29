using System;
using System.Collections.Generic;
using System.Linq;
using STF.IdComponents;
using STF.Serialisation;
using UnityEngine;
using UnityEngine.Animations;

namespace STF.ApplicationConversion
{
	public class STFUnityConverter : ASTFApplicationConverter
	{
		public override string TargetName => "unity3d";

		public override Dictionary<Type, ISTFNodeComponentApplicationConverter> Converters => new Dictionary<Type, ISTFNodeComponentApplicationConverter>() {
			{typeof(STFTwistConstraint), new STFTwistConstraintConverter()},
		};
		public override List<Type> WhitelistedComponents => new List<Type> {
			typeof(Transform), typeof(Animator), typeof(RotationConstraint), typeof(SkinnedMeshRenderer)
		};
		public override List<string> Targets => new List<string> {TargetName};

		public override bool CanConvert(STFAsset Asset)
		{
			return Asset.assetInfo?.assetType == STFAssetImporter._TYPE;
		}
	}
}