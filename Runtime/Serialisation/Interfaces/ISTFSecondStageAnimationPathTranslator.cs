
using System;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace stf.serialisation
{
	// Animation paths are stored with UUID's and then the property. Implementing classes of this interface translate these STF paths into application specific paths for second-stage assets.

	public interface ISTFSecondStageAnimationPathTranslator
	{
		(bool omit, string path, Type type, string property, AnimationCurve curve) Translate(GameObject root, string originalPath, Type originalType, string originalProperty, AnimationCurve originalCurve);
#if UNITY_EDITOR
		(bool omit, string path, Type type, string property, ObjectReferenceKeyframe[] curve) Translate(GameObject root, string originalPath, Type originalType, string originalProperty, ObjectReferenceKeyframe[] originalCurve);
#endif
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
			return (false, Utils.getPath(root.transform, targetGo.transform, true), originalType, originalProperty, newCurve);
		}
		
#if UNITY_EDITOR
		public (bool omit, string path, Type type, string property, ObjectReferenceKeyframe[] curve) Translate(GameObject root, string originalPath, Type originalType, string originalProperty, ObjectReferenceKeyframe[] originalCurve)
		{
			var targetId = originalPath.Split(':')[1];
			var targetGo = root.GetComponentsInChildren<STFUUID>().First(id => id.id == targetId)?.gameObject;
			if(targetGo == null) throw new Exception("Invalid animation path");
			var newCurve = new ObjectReferenceKeyframe[originalCurve.Count()];
			for(int i = 0; i < originalCurve.Count(); i++)
			{
				newCurve[i].time = originalCurve[i].time;
				newCurve[i].value = originalCurve[i].value;
			}
			return (false, Utils.getPath(root.transform, targetGo.transform, true), originalType, originalProperty, newCurve);
		}
#endif
	}

	public class DefaultSecondStageAnimationPathTranslator : ASTFSecondStageAnimationPathTranslator {}
}
