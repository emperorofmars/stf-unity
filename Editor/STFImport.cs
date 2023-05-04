
#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using stf.serialisation;
using UnityEditor;
using UnityEngine;


namespace stf
{
	public class STFUIImport : EditorWindow
	{
		private Vector2 scrollPos;
		//private GameObject asset;

		[MenuItem("STF Tools/Import")]
		public static void Init()
		{
			STFUIImport window = EditorWindow.GetWindow(typeof(STFUIImport)) as STFUIImport;
			window.titleContent = new GUIContent("Import STF");
			window.minSize = new Vector2(600, 700);
		}
		
		void OnGUI()
		{
			GUILayout.Label("Import STF ", EditorStyles.whiteLargeLabel);
			drawHLine();
			scrollPos = GUILayout.BeginScrollView(scrollPos, GUIStyle.none);
			
			if(GUILayout.Button("Open STF Json File", GUILayout.ExpandWidth(true))) {
				var path = EditorUtility.OpenFilePanel("Open STF File", "Assets", "json");
				if(path != null && path.Length > 0) {
					ImportJson(path);
				}
			}

			
			if(GUILayout.Button("Open STF File", GUILayout.ExpandWidth(true))) {
				var path = EditorUtility.OpenFilePanel("Open STF File", "Assets", "stf");
				if(path != null && path.Length > 0) {
					Import(path);
				}
			}
			
			GUILayout.EndScrollView();
			drawHLine();
			GUILayout.Label("v0.0.1", EditorStyles.label);
		}

		private void drawHLine() {
			GUILayout.Space(10);
			EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 2), Color.gray);
			GUILayout.Space(10);
		}

		private static void ImportJson(string path)
		{
			//var reader = new JsonTextReader(new StreamReader(path));
			JObject jsonRoot = JObject.Parse(new StreamReader(path).ReadToEnd());
			var importer = new STFImporter(jsonRoot);
			foreach(var resource in importer.GetResources())
			{
				if(resource.GetType() == typeof(Mesh))
				{
					AssetDatabase.CreateAsset(resource, "Assets/mymesh");
				}
			}
			foreach(var asset in importer.GetAssets())
			{
				var unityAsset = asset.Value.GetAsset();
			}
			AssetDatabase.SaveAssets();
			//var root = Object.Instantiate(asset);
			//root.name = asset.name;
			
			// Check the main asset type

			/*var state = new STFImporterExternalAsset(root);
			JObject jsonRoot = JObject.Parse(new StreamReader(path).ReadToEnd());
			
			state.parse(jsonRoot);*/
		}

		private static void Import(string path)
		{
			byte[] byteArray = File.ReadAllBytes(path);
			var importer = new STFImporter(byteArray);
			foreach(var resource in importer.GetResources())
			{
				if(resource.GetType() == typeof(Mesh))
				{
					AssetDatabase.CreateAsset(resource, "Assets/" + resource.name + ".mesh");
				}
				else
				{
					AssetDatabase.CreateAsset(resource, "Assets/" + resource.name);
				}
			}
			foreach(var asset in importer.GetAssets())
			{
				var unityAsset = asset.Value.GetAsset();
			}
			AssetDatabase.SaveAssets();
		}
	}
}
#endif
