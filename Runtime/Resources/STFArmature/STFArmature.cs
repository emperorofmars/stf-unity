

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace stf.serialisation
{
	public class STFArmatureExporter : ASTFResourceExporter
	{
		public override JToken SerializeToJson(ISTFExporter state, UnityEngine.Object resource)
		{
			var armature = (STFArmatureResource)resource;
			
			var ret = new JObject();
			ret.Add("type", STFArmatureImporter._TYPE);
			ret.Add("name", armature.armatureName);
			ret.Add("root", armature.rootId);

			if(armature.hasArmatureTransform)
			{
				ret.Add("trs", new JArray() {
					new JArray() {armature.armaturePosition.x, armature.armaturePosition.y, armature.armaturePosition.z},
					new JArray() {armature.armatureRotation.x, armature.armatureRotation.y, armature.armatureRotation.z, armature.armatureRotation.w},
					new JArray() {armature.armatureScale.x, armature.armatureScale.y, armature.armatureScale.z}
				});
			}
			var boneIds = new List<string>();
			foreach(var bone in armature.bones)
			{
				var boneJson = new JObject();
				boneJson.Add("name", bone.name);
				boneJson.Add("type", "bone");

				boneJson.Add("trs", new JArray() {
					new JArray() {bone.localPosition.x, bone.localPosition.y, bone.localPosition.z},
					new JArray() {bone.localRotation.x, bone.localRotation.y, bone.localRotation.z, bone.localRotation.w},
					new JArray() {bone.localScale.x, bone.localScale.y, bone.localScale.z}
				});
				boneJson.Add("children", new JArray(bone.children));

				boneIds.Add(bone.id);
				state.RegisterNode(bone.id, boneJson);
			}
			ret.Add("bones", new JArray(boneIds));
			return ret;
		}
	}
}
