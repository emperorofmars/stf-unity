using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using STF.Serialisation;
using UnityEngine;

namespace STF.Util
{
	public static class Utils
	{
		/*public static string getPath(Transform transform, bool relative = false)
		{
			string path = "/" + transform.name;
			while (transform.parent != null)
			{
				transform = transform.parent;
				path = "/" + transform.name + path;
			}
			if(relative) path = path.Substring(1);
			return path;
		}*/
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

		public static void RunTasks(List<Task> Tasks)
		{
			var originalTasks = Tasks;
			do
			{
				var currentTasks = Tasks;
				Tasks = new List<Task>();
				foreach(var task in currentTasks)
				{
					task.RunSynchronously();
					if(task.Exception != null) throw task.Exception;
				}
			}
			while(Tasks.Count > 0);
			originalTasks.Clear();
		}

		public static ISTFNode GetNodeComponent(GameObject Go)
		{
			return Go?.GetComponents<ISTFNode>()?.OrderByDescending(c => c.PrefabHirarchy).FirstOrDefault();
		}
	}
}
