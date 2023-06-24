using System.Collections.Generic;
using UnityEngine;

namespace stf.serialisation
{
	// Technically this is not a matrix. I dont care.
	public class STFRelationshipMatrix
	{
		private Dictionary<Component, string> ComponentToId = new Dictionary<Component, string>();
		private Dictionary<string, Component> IdToComponent = new Dictionary<string,Component>();
		private Dictionary<Component, List<Component>> Extends = new Dictionary<Component, List<Component>>();
		private Dictionary<Component, List<Component>> ExtendedBys = new Dictionary<Component, List<Component>>();
		private List<Component> IsOverridden = new List<Component>();
		private Dictionary<Component, List<Component>> Overrides = new Dictionary<Component, List<Component>>();		
		private Dictionary<Component, bool> TargetMatch = new Dictionary<Component, bool>();
		private Dictionary<Component, Component> STFToConverted = new Dictionary<Component, Component>();

		public STFRelationshipMatrix(GameObject root, List<string> targets)
		{
			foreach(var component in root.GetComponentsInChildren<Component>())
			{
				if(component is ISTFComponent)
				{
					ComponentToId.Add(component, ((ISTFComponent)component).id);
					IdToComponent.Add(((ISTFComponent)component).id, component);
				}
			}
			foreach(var component in root.GetComponentsInChildren<ISTFComponent>())
			{
				bool match = false;
				if(component.targets != null && component.targets.Count > 0)
				{
					foreach(var componentTarget in component.targets)
					{
						if(targets.Find(t => t == componentTarget) != null)
						{
							match = true;
							break;
						}
					}
				}
				else
				{
					match = true;
				}
				TargetMatch.Add((Component) component, match);
			}
			foreach(var component in root.GetComponentsInChildren<Component>())
			{
				if(component is ISTFComponent)
				{
					var c = (ISTFComponent)component;
					if(c.overrides != null)
					{
						foreach(var _override in c.overrides)
						{
							if(_override == null || _override.Length == 0) continue;

							if(Overrides.ContainsKey(component)) Overrides[component].Add(IdToComponent[_override]);
							else Overrides.Add(component, new List<Component> {IdToComponent[_override]});
							if(!IsOverridden.Contains(IdToComponent[_override])) IsOverridden.Add(IdToComponent[_override]);
						}
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

		public bool IsMatched(Component c)
		{
			return (TargetMatch.ContainsKey(c) ? TargetMatch[c] : true) && !IsOverridden.Contains(c);
		}

		public List<Component> GetExtended(Component component)
		{
			if(Extends.ContainsKey(component)) return Extends[component];
			else return new List<Component>();
		}

		public List<Component> GetOverridden(Component component)
		{
			if(Overrides.ContainsKey(component)) return Overrides[component];
			else return new List<Component>();
		}

		public void AddConverted(Component component, Component converted)
		{
			STFToConverted.Add(component, converted);
		}

		public Component GetConverted(Component component)
		{
			return STFToConverted[component];
		}

		public Component GetById(string id)
		{
			return IdToComponent.ContainsKey(id) ? IdToComponent[id] : null;
		}
	}
}