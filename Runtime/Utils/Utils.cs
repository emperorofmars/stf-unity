
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace stf
{
	public static class Utils
	{
		public static string getPath(Transform transform)
		{
			string path = "/" + transform.name;
			while (transform.parent != null)
			{
				transform = transform.parent;
				path = "/" + transform.name + path;
			}
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