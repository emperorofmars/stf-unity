
#if UNITY_EDITOR

using Newtonsoft.Json.Linq;
using UnityEditor;

namespace stf.serialisation
{
	public class STFExternalUnityResourceExporter : ASTFResourceExporter
	{

		override public JToken serializeToJson(ISTFExporter state, UnityEngine.Object unityResource)
		{
			var ret = new JObject();
			ret.Add("type", "external");
			
			ret.Add("path", AssetDatabase.GetAssetPath(unityResource));
			if(AssetDatabase.IsSubAsset(unityResource))
			{
				ret.Add("subasset", true);
				ret.Add("subasset_name", unityResource.name);
			}

			return ret;
		}
	}

	public class STFExternalResourceImporter : ASTFResourceImporter
	{
		override public UnityEngine.Object parseFromJson(ISTFImporter state, JToken json, string id)
		{
			var assets = AssetDatabase.LoadAllAssetsAtPath((string)json["path"]);
			if(assets.Length == 1) return assets[0];
			else foreach(var asset in assets) if(asset.name == (string)json["subasset_name"]) return asset;
			return null;
		}
	}

	[InitializeOnLoad]
	public class Register_AVAAvatar
	{
		static Register_AVAAvatar()
		{
			STFRegistry.RegisterResourceImporter("external", new STFExternalResourceImporter());
		}
	}
}

#endif
