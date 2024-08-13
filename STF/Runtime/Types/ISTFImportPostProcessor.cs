using System;
using static STF.Util.STFConstants;

namespace STF.Serialisation
{
	public interface ISTFImportPostProcessor
	{
		STFObjectType STFObjectType {get;}
		Type TargetType {get;}
		void PostProcess(STFImportState State, object Resource);
	}
}