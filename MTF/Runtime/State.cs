
namespace MTF
{
	public interface IImportState
	{
		UnityEngine.Object GetResource(string Id);
	}

	public interface IExportState
	{
		string AddResource(UnityEngine.Object Resource);
	}
}