
#if UNITY_EDITOR
#if AVA_VRCSDK3_FOUND

using AVA.Serialisation;
using STF.ApplicationConversion;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace AVA.ApplicationConversion
{
	public class AVA_VRC_AvatarConverter : ISTFNodeComponentApplicationConverter
	{
		public void ConvertResources(ISTFApplicationConvertState State, Component Component)
		{
			// nothing to convert
		}

		public string ConvertPropertyPath(ISTFApplicationConvertState State, Component Resource, string STFProperty)
		{
			throw new System.NotImplementedException();
		}

		public void Convert(ISTFApplicationConvertState State, Component Component)
		{
			var avaAvatar = (AVAAvatar)Component;

			var vrcAvatar = Component.gameObject.AddComponent<VRCAvatarDescriptor>();
			if(avaAvatar.viewport_parent != null && avaAvatar.viewport_position != null) vrcAvatar.ViewPosition = avaAvatar.viewport_parent.transform.position - State.Root.transform.position + avaAvatar.viewport_position;

			if(avaAvatar.GetComponent<Animator>() == null)
			{
				var animator = avaAvatar.gameObject.AddComponent<Animator>();
				animator.applyRootMotion = true;
				animator.updateMode = AnimatorUpdateMode.Normal;
				animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
				animator.avatar = avaAvatar.TryGetHumanoidDefinition()?.GeneratedAvatar;
				
				State.RelMat.AddConverted(Component, animator);
			}

			State.RelMat.AddConverted(Component, vrcAvatar);
		}
	}
}

#endif
#endif
