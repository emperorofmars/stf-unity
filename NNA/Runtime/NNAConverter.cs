
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace nna
{
	public static class NNAConverter
	{
		public static void Convert(NNAContext Context)
		{
			var Trash = new List<Transform>();
			foreach(var nnaNode in Context.Root.GetComponentsInChildren<Transform>())
			{
				if(!Trash.Contains(nnaNode) && ParseUtil.IsNNANode(nnaNode.name))
				{
					var target = nnaNode;
					if(nnaNode.name.StartsWith("$nna"))
					{
						target = target.parent;
						Trash.Add(nnaNode);
					}
					foreach(JObject component in ParseUtil.ParseNode(nnaNode, Trash))
					{

						if(Context.ContainsProcessor(component))
						{
							var actualNodeName = ParseUtil.GetActualNodeName(target.name);
							Context.Get(component).Process(Context, target.gameObject, nnaNode.gameObject, component);
							if(string.IsNullOrWhiteSpace(actualNodeName)) Trash.Add(target);
							else target.name = actualNodeName;
						}
						else
						{
							Debug.LogWarning($"Processor not found for NNA type: {Context.GetType(component)}");
							continue;
						}
					}
				}
			}
			Context.RunTasks();
			foreach(var t in Trash)
			{
				Object.DestroyImmediate(t.gameObject);
			}
		}
	}
}
