
namespace STF.Serialisation
{
	public class STFBoneInstanceNode : ASTFNode
	{
		public const string _TYPE = "STF.bone_instance";
		public override string Type => _TYPE;
		public string BoneId;
	}
}
