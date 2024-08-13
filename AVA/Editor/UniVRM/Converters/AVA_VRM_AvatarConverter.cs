#if UNITY_EDITOR
#if AVA_UNIVRM_FOUND

using AVA.Types;
using STF.ApplicationConversion;
using STF.Types;
using UnityEngine;
using VRM;

namespace AVA.ApplicationConversion
{
	public class AVA_VRM_AvatarConverter : ISTFNodeComponentApplicationConverter
	{
		public void Convert(ISTFApplicationConvertState State, Component Component)
		{
			var avaAvatar = (AVAAvatar)Component;
			var asset = State.Root.GetComponent<ISTFAsset>();

			if(avaAvatar.GetComponent<Animator>() == null)
			{
				var animator = avaAvatar.gameObject.AddComponent<Animator>();
				animator.applyRootMotion = true;
				animator.updateMode = AnimatorUpdateMode.Normal;
				animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
				animator.avatar = avaAvatar.TryGetHumanoidDefinition()?.GeneratedAvatar;
				
				State.RelMat.AddConverted(Component, animator);
			}

			var vrmMetaComponent = Component.gameObject.AddComponent<VRMMeta>();
			var vrmMeta = ScriptableObject.CreateInstance<VRMMetaObject>();
			vrmMeta.name = "VRM_Meta";
			vrmMetaComponent.Meta = vrmMeta;
			vrmMeta.Title = asset.Name;
			vrmMeta.Version = asset.Version;
			vrmMeta.Author = asset.Author;
			vrmMeta.ExporterVersion = asset.Version;
			vrmMeta.OtherLicenseUrl = asset.LicenseLink;
			vrmMeta.Thumbnail = asset.Preview;
			
			var vrmFirstPerson = Component.gameObject.AddComponent<VRMFirstPerson>();
			vrmFirstPerson.FirstPersonBone = avaAvatar.viewport_parent.Node?.transform;
			vrmFirstPerson.FirstPersonOffset = avaAvatar.viewport_position;

			var secondary = new GameObject();
			secondary.name = "VRM_secondary";
			secondary.transform.SetParent(State.Root.transform, false);

			State.SaveGeneratedResource(vrmMeta, "Asset");
			State.RelMat.AddConverted(Component, vrmMetaComponent);
		}

		public string ConvertPropertyPath(ISTFApplicationConvertState State, Component Component, string STFProperty)
		{
			throw new System.NotImplementedException();
		}

		public void ConvertResources(ISTFApplicationConvertState State, Component Component)
		{
			return;
		}
	}
}

#endif
#endif
