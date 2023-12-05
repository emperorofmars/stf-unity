using System;

namespace STF.Serialisation
{
	public enum STFObjectType {Asset, Node, NodeComponent, Resource, ResourceComponent}
	public interface ISTFImportPostProcessor
	{
		STFObjectType STFObjectType {get;}
		Type TargetType {get;}
		void PostProcess(ISTFImportState State, object Resource);
	}
}