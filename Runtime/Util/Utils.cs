
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using stf.serialisation;
using UnityEngine;

namespace stf
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

		public static bool isNodeInAsset(string id, GameObject root)
		{
			return false;
		}

		public static string getIdFromUnityObject(UnityEngine.Object o)
		{
			if(o is GameObject)
			{
				return ((GameObject)o).GetComponent<STFUUID>()?.id;
			}
			else if(o is Transform)
			{
				return ((Transform)o).GetComponent<STFUUID>()?.id;
			}
			else if(o is ISTFComponent)
			{
				return ((ISTFComponent)o).id;
			}
			else if(o is Component)
			{
				return ((Component)o).gameObject.GetComponent<STFUUID>()?.componentIds.Find(c => c.component == o)?.id;
			}
			// also handle resources
			return null;
		}
	}
}