
using UnityEngine;
using STF.Types;

namespace STF.Serialisation
{
	public interface IUnityImportContext
	{
		bool IsDegraded {get;}

		Object SaveResource(ISTFResource Resource);
		Object SaveSubResource(Object SubResource, Object Resource);
		Object SaveGeneratedResource(GameObject Resource);
		Object SaveGeneratedResource(Object Resource, string FileExtension);
		Object SaveGeneratedResource(byte[] Resource, string Name, string FileExtension);

		Object Instantiate(Object Resource);
	}
}