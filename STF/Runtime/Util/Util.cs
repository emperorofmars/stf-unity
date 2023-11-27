using STF.Serialisation;
using UnityEngine;

namespace STF.Util
{
	public static class Utils
	{
		public static string getPath(Transform transform, bool relative = false)
		{
			string path = "/" + transform.name;
			while (transform.parent != null)
			{
				transform = transform.parent;
				path = "/" + transform.name + path;
			}
			if(relative) path = path.Substring(1);
			return path;
		}
		public static string getPath(Transform root, Transform transform, bool relative = false)
		{
			string path = "/" + transform.name;
			while (transform.parent != root && transform.parent != null)
			{
				transform = transform.parent;
				path = "/" + transform.name + path;
			}
			if(relative) path = path.Substring(1);
			return path;
		}
		public static Transform getRoot(Transform transform)
		{
			Transform parent = transform.parent;
			while (parent != null)
			{
				transform = parent;
				parent = transform.parent;
			}
			return transform;
		}
	}
}
