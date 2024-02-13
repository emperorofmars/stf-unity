
#if UNITY_EDITOR
#if AVA_VRCSDK3_FOUND

using AVA.Serialisation;
using STF.ApplicationConversion;
using UnityEngine;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace AVA.ApplicationConversion
{
	public class AVA_VRC_JankyFallbackPhysicsConverter : ISTFNodeComponentApplicationConverter
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
			var c = (AVAJankyFallbackPhysics)Component;
			var physbone = Component.gameObject.AddComponent<VRCPhysBone>();
			
			physbone.rootTransform = c.target ? c.target.transform : c.transform;
			physbone.pull = c.pull;
			physbone.spring = c.spring;
			physbone.limitType = VRC.Dynamics.VRCPhysBoneBase.LimitType.Angle;
			physbone.maxAngleX = c.stiffness * 180;
			physbone.maxAngleZ = c.stiffness * 180;
			
			State.RelMat.AddConverted(Component, physbone);
		}
	}
}

#endif
#endif
