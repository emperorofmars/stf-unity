
using System;
using System.Collections.Generic;
using stf.Components;
using UnityEngine;
using UnityEngine.Animations;

namespace stf.serialisation
{
	public class STFDefaultSecondStage : ASTFSecondStageDefault
	{
		protected override Dictionary<Type, ISTFSecondStageResourceProcessor> ResourceProcessors => new Dictionary<Type, ISTFSecondStageResourceProcessor> {
#if UNITY_EDITOR
			{typeof(AnimationClip), new STFAnimationSecondStageProcessor()}
#endif
		};

		protected override Dictionary<Type, ISTFSecondStageConverter> Converters => new Dictionary<Type, ISTFSecondStageConverter>() {
			{typeof(STFTwistConstraintBack), new STFTwistConstraintBackConverter()},
			{typeof(STFTwistConstraintForward), new STFTwistConstraintForwardConverter()}
		};

		protected override List<Type> WhitelistedComponents => new List<Type> {
			typeof(Transform), typeof(Animator), typeof(RotationConstraint), typeof(SkinnedMeshRenderer)
		};

		protected override string GameObjectSuffix => "Unity";

		protected override string StageName => "Unity";

		protected override string AssetTypeName => "Unity";

		protected override List<string> Targets => new List<string> {"unity"};

		public override bool CanHandle(ISTFAsset asset, UnityEngine.Object adaptedUnityAsset)
		{
			return asset.GetSTFAssetType() == "asset";
		}
	}
}