#if UNITY_EDITOR

using Newtonsoft.Json.Linq;
using UnityEngine;

namespace STF.Util
{
	public class TRSUtil
	{
		public static Vector3 ParseLocation(JObject Json)
		{
			return new Vector3((float)Json["trs"][0][0], (float)Json["trs"][0][1], (float)Json["trs"][0][2]);
		}
		public static Quaternion ParseRotation(JObject Json)
		{
			return new Quaternion((float)Json["trs"][1][0], (float)Json["trs"][1][1], (float)Json["trs"][1][2], (float)Json["trs"][1][3]);
		}
		public static Vector3 ParseScale(JObject Json)
		{
			return new Vector3((float)Json["trs"][2][0], (float)Json["trs"][2][1], (float)Json["trs"][2][2]);
		}
		public static void ParseTRS(Transform Target, JObject Json)
		{
			Target.localPosition = TRSUtil.ParseLocation(Json);
			Target.localRotation = TRSUtil.ParseRotation(Json);
			Target.localScale = TRSUtil.ParseScale(Json);
		}
		public static void ParseTRS(GameObject Target, JObject Json)
		{
			ParseTRS(Target.transform, Json);
		}
	}
}

#endif
