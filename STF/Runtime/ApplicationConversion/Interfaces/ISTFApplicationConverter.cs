using System.Collections.Generic;
using System.Threading.Tasks;
using STF.IdComponents;
using UnityEngine;

namespace STF.ApplicationConversion
{
	public interface ISTFApplicationConverter
	{
		string TargetName {get;}
		bool CanConvert(STFAsset Asset);
		GameObject Convert(ISTFApplicationConvertStorageContext StorageContext, STFAsset Asset);
	}
}