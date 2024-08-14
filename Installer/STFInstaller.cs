
#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace STF.Installer
{
	static class STFInstaller
	{
		const string URL_BASE = "https://github.com/emperorofmars/stf-unity.git";
		const string VERSION_TAG_LATEST = null;//"v0.2.0";
		const string Path_STF = "/STF";
		const string Path_MTF = "/MTF";
		const string Path_AVA = "/AVA";

		private static string ConstructURL(string Base, string Path, string Version = null)
		{
			return string.IsNullOrWhiteSpace(Version) ? Base + "?path=" + Path : Base + "?path=" + Path + "#" + Version;
		}

		static AddRequest Request;

		[MenuItem("STF Tools/Install")]
		static void Add()
		{
			Install(VERSION_TAG_LATEST);
		}

		static void Install(string Version = null)
		{
			Request = Client.Add(ConstructURL(URL_BASE, Path_MTF, Version));
			EditorApplication.update += Progress;
			/*Request = Client.Add(ConstructURL(URL_BASE, Path_STF, Version));
			EditorApplication.update += Progress;
			Request = Client.Add(ConstructURL(URL_BASE, Path_AVA, Version));
			EditorApplication.update += Progress;*/
		}

		static void Progress()
		{
			if (Request.IsCompleted)
			{
				if (Request.Status == StatusCode.Success)
					Debug.Log("Installed: " + Request.Result.packageId);
				else if (Request.Status >= StatusCode.Failure)
					Debug.Log(Request.Error.message);

				EditorApplication.update -= Progress;
			}
		}

		
		static STFInstaller()
		{
			Install(VERSION_TAG_LATEST);
		}
	}
}

#endif
