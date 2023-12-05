using System;
using static STF.Serialisation.STFConstants;

namespace STF.Serialisation
{
	public interface ISTFImportPostProcessor
	{
		STFObjectType STFObjectType {get;}
		Type TargetType {get;}
		void PostProcess(ISTFImportState State, object Resource);
	}
}