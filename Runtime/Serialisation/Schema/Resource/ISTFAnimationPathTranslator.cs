
using System;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System.IO;
using System.Threading.Tasks;

namespace stf.serialisation
{
	public interface ISTFAnimationPathTranslator
	{
		string ToSTF(string property);
		string ToUnity(string property);
	}

	public class STFTransformAnimationPathTranslator : ISTFAnimationPathTranslator
	{
		public string ToSTF(string property)
		{
			if(property.StartsWith("m_LocalPosition"))
			{
				if(property.EndsWith("x")) return "translation.x";
				if(property.EndsWith("y")) return "translation.y";
				if(property.EndsWith("z")) return "translation.z";
			}
			else if(property.StartsWith("m_LocalRotation"))
			{
				if(property.EndsWith("x")) return "rotation.x";
				if(property.EndsWith("y")) return "rotation.y";
				if(property.EndsWith("z")) return "rotation.z";
				if(property.EndsWith("w")) return "rotation.w";
			}
			else if(property.StartsWith("m_LocalScale"))
			{
				if(property.EndsWith("x")) return "scale.x";
				if(property.EndsWith("y")) return "scale.y";
				if(property.EndsWith("z")) return "scale.z";
			}
			throw new Exception("Unrecognized animation property: " + property);
		}

		public string ToUnity(string property)
		{
			if(property.StartsWith("translation"))
			{
				if(property.EndsWith("x")) return "m_LocalPosition.x";
				if(property.EndsWith("y")) return "m_LocalPosition.y";
				if(property.EndsWith("z")) return "m_LocalPosition.z";
			}
			else if(property.StartsWith("rotation"))
			{
				if(property.EndsWith("x")) return "m_LocalRotation.x";
				if(property.EndsWith("y")) return "m_LocalRotation.y";
				if(property.EndsWith("z")) return "m_LocalRotation.z";
				if(property.EndsWith("w")) return "m_LocalRotation.w";
			}
			else if(property.StartsWith("scale"))
			{
				if(property.EndsWith("x")) return "m_LocalScale.x";
				if(property.EndsWith("y")) return "m_LocalScale.y";
				if(property.EndsWith("z")) return "m_LocalScale.z";
			}
			throw new Exception("Unrecognized animation property: " + property);
		}
	}
}
