
#if UNITY_EDITOR
#if AVA_VRCSDK3_FOUND

using AVA.Types;
using STF.ApplicationConversion;
using UnityEngine;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace AVA.ApplicationConversion
{
	public class AVA_VRC_PhysbonesConverter : ISTFNodeComponentApplicationConverter
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
			var c = (AVAVRCPhysbones)Component;
			var physbone = Component.gameObject.AddComponent<VRCPhysBone>();
			
			physbone.rootTransform = c.target.IsRef ? c.target.Node.transform : c.transform;
			physbone.version = c.version == "1.1" ? VRC.Dynamics.VRCPhysBoneBase.Version.Version_1_1 : VRC.Dynamics.VRCPhysBoneBase.Version.Version_1_0;
			physbone.integrationType = c.integration_type == "simplified" ? VRC.Dynamics.VRCPhysBoneBase.IntegrationType.Simplified : VRC.Dynamics.VRCPhysBoneBase.IntegrationType.Advanced;
			physbone.pull = c.pull;
			physbone.spring = c.spring;
			physbone.stiffness = c.stiffness;
			physbone.gravity = c.gravity;
			physbone.gravityFalloff = c.gravity_falloff;

			State.RelMat.AddConverted(Component, physbone);
		}
	}
}

#endif
#endif
