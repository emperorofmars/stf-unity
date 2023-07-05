
using System;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

namespace stf.serialisation
{
	public interface ISTFSecondStageAnimationPathTranslator
	{
		(bool omit, string path, Type type, string property, AnimationCurve curve) Translate(GameObject root, string originalPath, Type originalType, string originalProperty, AnimationCurve originalCurve);
	}

	public abstract class ASTFSecondStageAnimationPathTranslator : ISTFSecondStageAnimationPathTranslator
	{
		public (bool omit, string path, Type type, string property, AnimationCurve curve) Translate(GameObject root, string originalPath, Type originalType, string originalProperty, AnimationCurve originalCurve)
		{
			var targetId = originalPath.Split(':')[1];
			var targetGo = root.GetComponentsInChildren<STFUUID>().First(id => id.id == targetId)?.gameObject;
			if(targetGo == null) throw new Exception("Invalid animation path");
			var newCurve = new AnimationCurve();
			foreach(var keyframe in originalCurve.keys)
			{
				newCurve.AddKey(keyframe.time, keyframe.value);
			}
			return (false, Utils.getPath(root.transform, targetGo.transform), originalType, originalProperty, newCurve);
		}
	}

	public class DefaultSecondStageAnimationPathTranslator : ASTFSecondStageAnimationPathTranslator {}
}
