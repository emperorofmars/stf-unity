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
		public const string _TARGET_NAME = "unity3d";
		public override string TargetName => _TARGET_NAME;


		private Dictionary<Type, ISTFNodeComponentApplicationConverter> _Converters = new Dictionary<Type, ISTFNodeComponentApplicationConverter>() {
			{typeof(STFTwistConstraint), new STFTwistConstraintConverter()},
		};
		public override Dictionary<Type, ISTFNodeComponentApplicationConverter> Converters => _Converters;

		private List<Type> _WhitelistedComponents = new List<Type> {
			typeof(Transform), typeof(Animator), typeof(RotationConstraint), typeof(SkinnedMeshRenderer)
		};
		public override List<Type> WhitelistedComponents => _WhitelistedComponents;

		public override List<string> Targets => new List<string> {_TARGET_NAME};

		public override bool CanConvert(STFAsset Asset)
		{
			return Asset.assetInfo?.assetType == STFAssetImporter._TYPE;
		}
	}
}