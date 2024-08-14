using System;
using System.Collections.Generic;
using STF.Types;
using UnityEngine;

namespace STF.ApplicationConversion
{
	// Initiated at the start of a second-stage-importer.
	// Determines which component is being overridden and should therefore not be parsed, which component is extended by which and if a component is matched for the second-stages targets.
	// All is easily query-able by second-stage importers.

	// Technically this is not a matrix. I dont care.
	public class STFRelationshipMatrix
	{
		private readonly Dictionary<Component, string> ComponentToId = new();
		private readonly Dictionary<string, Component> IdToComponent = new();
		private readonly Dictionary<Component, List<Component>> Extends = new();
		private readonly Dictionary<Component, List<Component>> ExtendedBys = new();
		private readonly List<Component> IsOverridden = new List<Component>();
		private readonly Dictionary<Component, List<Component>> Overrides = new();		
		private readonly Dictionary<Component, bool> TargetMatch = new();
		private readonly Dictionary<Component, List<Component>> STFToConverted = new();

		public STFRelationshipMatrix(GameObject root, List<string> targets, List<Type> conversibleTypes)
		{
			// First build Component <--> ID maps
			foreach(var component in root.GetComponentsInChildren<Component>())
			{
				if(component is ISTFNodeComponent && component.GetType() != typeof(STFUnrecognizedNodeComponent))
				{
					ComponentToId.Add(component, ((ISTFNodeComponent)component).Id);
					IdToComponent.Add(((ISTFNodeComponent)component).Id, component);
				}
			}
			// Check which component is valid for the current target
			foreach(var component in root.GetComponentsInChildren<ISTFNodeComponent>())
			{
				bool match = false;
				if(component.Targets != null && component.Targets.Count > 0)
				{
					foreach(var componentTarget in component.Targets)
					{
						if(targets.Find(t => t == componentTarget) != null)
						{
							Debug.Log(component);
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
			// Figure out which component overrides which component
			foreach(var component in root.GetComponentsInChildren<Component>())
			{
				if(component is ISTFNodeComponent && conversibleTypes.Contains(component.GetType()) && TargetMatch.ContainsKey(component))
				{
					var c = (ISTFNodeComponent)component;
					if(c.Overrides != null)
					{
						foreach(var _override in c.Overrides)
						{
							if(_override == null || _override.Length == 0) continue;

							if(Overrides.ContainsKey(component)) Overrides[component].Add(IdToComponent[_override]);
							else Overrides.Add(component, new List<Component> {IdToComponent[_override]});
							if(!IsOverridden.Contains(IdToComponent[_override])) IsOverridden.Add(IdToComponent[_override]);
						}
					}
				}
			}
			// Build the extends relationship lists
			foreach(var component in root.GetComponentsInChildren<Component>())
			{
				if(component is ISTFNodeComponent && TargetMatch.ContainsKey(component) && !IsOverridden.Contains(component))
				{
					var c = (ISTFNodeComponent)component;
					if(c.Extends != null) foreach(var extend in c.Extends)
					{
						if(IdToComponent.ContainsKey(extend))
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

		public bool IsMatched(Component c)
		{
			return (TargetMatch.ContainsKey(c) ? TargetMatch[c] : true) && !IsOverridden.Contains(c);
		}

		public List<Component> GetExtended(Component component)
		{
			if(Extends.ContainsKey(component)) return Extends[component];
			else return new List<Component>();
		}

		public T GetExtended<T>(Component component) where T: Component
		{
			if(Extends.ContainsKey(component))
			{
				foreach(var extend in Extends[component])
				{
					if(extend is T) return (T)extend;
				}
			}
			return default(T);
		}

		public List<Component> GetOverridden(Component component)
		{
			if(Overrides.ContainsKey(component)) return Overrides[component];
			else return new List<Component>();
		}

		public void AddConverted(Component component, Component converted)
		{
			if(STFToConverted.ContainsKey(component)) STFToConverted[component].Add(converted);
			else STFToConverted.Add(component, new List<Component> {converted});
		}

		public List<Component> GetConverted(Component component)
		{
			return STFToConverted[component];
		}

		public Component GetById(string id)
		{
			return IdToComponent.ContainsKey(id) ? IdToComponent[id] : null;
		}
	}
}
