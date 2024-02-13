#if UNITY_EDITOR
#if AVA_UNIVRM_FOUND

using System.Collections.Generic;
using System.Threading.Tasks;
using AVA.Serialisation;
using STF.ApplicationConversion;
using STF.Serialisation;
using UnityEngine;
using VRM;

namespace ava.Converters
{
	public class AVAAvatarVRMConverter : ISTFNodeComponentApplicationConverter
	{
		public void Convert(ISTFApplicationConvertState State, Component Component)
		{
			var avaAvatar = (AVAAvatar)Component;
			var asset = State.Root.GetComponent<ISTFAsset>();

			var vrmMetaComponent = Component.gameObject.AddComponent<VRMMeta>();
			var vrmMeta = new VRMMetaObject();
			vrmMeta.name = "VRM_Meta";
			vrmMetaComponent.Meta = vrmMeta;
			vrmMeta.Title = asset.Name;
			vrmMeta.Version = asset.Version;
			vrmMeta.Author = asset.Author;
			vrmMeta.ExporterVersion = asset.Version;
			vrmMeta.OtherLicenseUrl = asset.LicenseLink;
			vrmMeta.Thumbnail = asset.Preview;
			
			State.AddTask(new Task(() => {
				//var humanoid = avaAvatar.TryGetHumanoidDefinition();

				var vrmFirstPerson = Component.gameObject.AddComponent<VRMFirstPerson>();
				vrmFirstPerson.FirstPersonBone = avaAvatar.viewport_parent?.transform;
				vrmFirstPerson.FirstPersonOffset = avaAvatar.viewport_position;
			}));

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
