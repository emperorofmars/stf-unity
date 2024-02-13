
namespace STF.Serialisation
{
	public class STFBoneInstanceNode : ISTFNode
	{
		public const string _TYPE = "STF.bone_instance";
		public override string Type => _TYPE;
		public string BoneId;
	}
}
