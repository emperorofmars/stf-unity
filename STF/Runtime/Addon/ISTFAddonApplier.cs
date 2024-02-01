using UnityEngine;

namespace STF.Addon
{
	public interface ISTFAddonApplier
	{
		public void Apply(ISTFAddonApplierContext Context, GameObject Target, Component SourceComponent);
	}

	public abstract class ASTFAddonApplier : ISTFAddonApplier
	{
		public virtual void Apply(ISTFAddonApplierContext Context, GameObject Target, Component SourceComponent)
		{
			
			var newComponent = Target.AddComponent(SourceComponent.GetType());
			System.Reflection.FieldInfo[] fields = SourceComponent.GetType().GetFields(); 
			foreach (System.Reflection.FieldInfo field in fields)
			{
				field.SetValue(newComponent, field.GetValue(SourceComponent));
			}
		}
	}
}