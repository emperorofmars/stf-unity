
#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;
using STF.Serde;
using Newtonsoft.Json.Linq;
using UnityEditor;

namespace STF.Tools
{
	// A scripted importer for STF files. It only parses the basic info about the contained assets, the full import happens on an explicit user action. Full import puts everything into the Assets folder.
	[ScriptedImporter(1, new string[] {"stf"})]
	public class STFScriptedImporter : ScriptedImporter
	{
		public static string DefaultUnpackFolder = "/STF Imports";
		public string UnpackFolder;

		public override void OnImportAsset(AssetImportContext ctx)
		{
			var importInfo = STFImportInfo.CreateInstance(new STFFile(ctx.assetPath));

			ctx.AddObjectToAsset("main", importInfo);
			ctx.SetMainObject(importInfo);

			EnsureDefaultUnpackFolder(Path.GetFileNameWithoutExtension(ctx.assetPath));
		}

		private void EnsureDefaultUnpackFolder(string filename)
		{
			if(!Directory.Exists("Assets/" + DefaultUnpackFolder))
			{
				AssetDatabase.CreateFolder("Assets", DefaultUnpackFolder);
				AssetDatabase.Refresh();
			}
			if(!Directory.Exists("Assets/" + DefaultUnpackFolder + "/" + filename))
			{
				AssetDatabase.CreateFolder("Assets/" + DefaultUnpackFolder, filename);
				AssetDatabase.Refresh();
			}
		}
	}
}

#endif
