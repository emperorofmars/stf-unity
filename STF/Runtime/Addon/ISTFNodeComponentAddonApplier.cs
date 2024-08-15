using UnityEngine;

namespace STF.Addon
{
	public abstract class ISTFNodeComponentAddonApplier
	{
		public virtual void Apply(ISTFAddonApplierContext Context, GameObject Target, Component SourceComponent)
		{
			STFDefaultNodeComponentAddonApplier.Apply(Context, Target, SourceComponent);
		}
	}

	public static class STFDefaultNodeComponentAddonApplier
	{
		public static void Apply(ISTFAddonApplierContext Context, GameObject Target, Component SourceComponent)
		{
			Component newComponent = Target.GetComponent(SourceComponent.GetType());
			if(newComponent == null) newComponent = Target.AddComponent(SourceComponent.GetType());
			
			System.Reflection.FieldInfo[] fields = SourceComponent.GetType().GetFields(); 
			foreach (System.Reflection.FieldInfo field in fields)
			{
				if(!field.IsStatic) field.SetValue(newComponent, field.GetValue(SourceComponent));
			}
		}
	}
}