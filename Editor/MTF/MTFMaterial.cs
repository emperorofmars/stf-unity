using System;
using System.Collections.Generic;
using UnityEngine;

namespace MTF
{
	public interface IProperty
	{
		string Type {get;}
		string Name {get; set;}
		List<IPropertyValue> Values {get; set;}
	}

	[Serializable]
	public abstract class AProperty : IProperty
	{
		public abstract string Type { get; }
		public string _Name;
		public string Name { get => _Name; set => _Name = value; }
		public List<IPropertyValue> _Values;
		public List<IPropertyValue> Values { get => _Values; set => _Values = value; }
	}

	public interface IPropertyValue
	{
		string Type {get;}
	}

	[CreateAssetMenu(fileName = "MTF Material", menuName = "MTF/Material", order = 1)]
	public class Material : ScriptableObject
	{
		public string Id = Guid.NewGuid().ToString();
		public UnityEngine.Material ConvertedMaterial;
		public List<string> PreferedShaderPerTarget = new List<string>();
		public List<string> StyleHints = new List<string>();
		public List<IProperty> Properties = new List<IProperty>();
	}

	public interface IMaterialConverter
	{
		string ShaderName {get;}
		UnityEngine.Material ConvertToUnityMaterial(Material MTFMaterial, UnityEngine.Material ExistingUnityMaterial);
	}

	public interface IMaterialParser
	{
		string ShaderName {get;}
		Material ParseFromUnityMaterial(UnityEngine.Material UnityMaterial, Material ExistingMTFMaterial);
	}
}