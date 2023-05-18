using System.Collections.Generic;
using UnityEngine;

namespace stf.serialisation
{
	// Technically this is not a matrix. I dont care.
	public class STFRelationshipMatrix
	{
		public Dictionary<Component, string> ComponentToId = new Dictionary<Component, string>();
		public Dictionary<string, Component> IdToComponent = new Dictionary<string,Component>();

		public Dictionary<Component, List<Component>> Extends = new Dictionary<Component, List<Component>>();
		public Dictionary<Component, List<Component>> ExtendedBys = new Dictionary<Component, List<Component>>();
		public List<Component> IsOverridden = new List<Component>();
		public Dictionary<Component, List<Component>> Overrides = new Dictionary<Component, List<Component>>();
		//public List<Component> ParseMatch = new List<Component>();

		public Dictionary<Component, Component> STFToConverted = new Dictionary<Component, Component>();

		public STFRelationshipMatrix(GameObject root, string target)
		{
			foreach(var component in root.GetComponentsInChildren<Component>())
			{
				if(component is ISTFComponent)
				{
					ComponentToId.Add(component, ((ISTFComponent)component).id);
					IdToComponent.Add(((ISTFComponent)component).id, component);
				}
			}
			// target matches
			foreach(var component in root.GetComponentsInChildren<Component>())
			{
				if(component is ISTFComponent)
				{
					var c = (ISTFComponent)component;
					if(c.overrides != null) foreach(var _override in c.overrides)
					{
						if(Overrides.ContainsKey(component)) Overrides[component].Add(IdToComponent[_override]);
						else Overrides.Add(component, new List<Component> {IdToComponent[_override]});
						if(!IsOverridden.Contains(IdToComponent[_override])) IsOverridden.Add(IdToComponent[_override]);
					}
				}
			}
			foreach(var component in root.GetComponentsInChildren<Component>())
			{
				if(component is ISTFComponent && !IsOverridden.Contains(component))
				{
					var c = (ISTFComponent)component;
					if(c.extends != null) foreach(var extend in c.extends)
					{
						if(Extends.ContainsKey(component)) Extends[component].Add(IdToComponent[extend]);
						else Extends.Add(component, new List<Component> {IdToComponent[extend]});
						
						if(ExtendedBys.ContainsKey(IdToComponent[extend])) ExtendedBys[IdToComponent[extend]].Add(component);
						else ExtendedBys.Add(IdToComponent[extend], new List<Component> {component});
					}
				}
			}
		}
	}
}