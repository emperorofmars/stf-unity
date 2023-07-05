
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
		string TranslatePath(GameObject root, string path);
		Type TranslateType(Type originalType);
		string TranslateProperty(string property);
	}

	public abstract class ASTFSecondStageAnimationPathTranslator : ISTFSecondStageAnimationPathTranslator
	{
		public string TranslatePath(GameObject root, string path)
		{
			var targetId = path.Split(':')[1];
			var targetGo = root.GetComponentsInChildren<STFUUID>().First(id => id.id == targetId)?.gameObject;
			if(targetGo == null) throw new Exception("Invalid animation path");
			return Utils.getPath(root.transform, targetGo.transform);
		}

		public Type TranslateType(Type originalType)
		{
			return originalType;
		}
		
		public string TranslateProperty(string property)
		{
			return property;
		}
	}

	public class DefaultSecondStageAnimationPathTranslator : ASTFSecondStageAnimationPathTranslator
	{

	}
}
