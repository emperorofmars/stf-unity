
using STF.Types;

namespace STF.Serialisation
{
	public interface IUnityExportContext
	{
		T LoadMeta<T>(UnityEngine.Object Resource) where T: ISTFResource;
		(byte[] Data, T Meta, string Path) LoadAsset<T>(UnityEngine.Object Resource) where T: ISTFResource;
	}
}
