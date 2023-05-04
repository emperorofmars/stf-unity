
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace stf
{
	public class Utils
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
	}
}