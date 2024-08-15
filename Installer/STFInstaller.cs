
#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace STF.Installer
{
	static class STFInstaller
	{
		const string URL_BASE = "https://github.com/emperorofmars/stf-unity.git";
		const string VERSION_TAG_LATEST = null;//"v0.3.0";
		const string Path_STF = "/STF";
		const string Path_MTF = "/MTF";
		//const string Path_AVA = "/AVA";

		static AddRequest Request;
		static List<string> InstallQueue = new();

		private static string ConstructURL(string Base, string Path, string Version = null)
		{
			return string.IsNullOrWhiteSpace(Version) ? Base + "?path=" + Path : Base + "?path=" + Path + "#" + Version;
		}

		//[MenuItem("STF Tools/Install")]
		static void Install()
		{
			InstallQueue = BuildInstallQueue(VERSION_TAG_LATEST);
			InstallNext();
		}

		static void InstallNext()
		{
			lock(InstallQueue)
			{
				if(InstallQueue.Count > 0)
				{
					var url = InstallQueue[0];
					InstallQueue.Remove(url);
					Request = Client.Add(url);
					EditorApplication.update += Progress;
				}
			}
		}

		static List<string> BuildInstallQueue(string Version = null)
		{
			return new List<string>
			{
				#if !MTF
				ConstructURL(URL_BASE, Path_MTF, Version),
				#endif
				#if !STF
				ConstructURL(URL_BASE, Path_STF, Version),
				#endif
				/*#if !AVA
				ConstructURL(URL_BASE, Path_AVA, Version),
				#endif*/
			};
		}

		static void Progress()
		{
			if (Request.IsCompleted)
			{
				if (Request.Status == StatusCode.Success)
				{
					Debug.Log("Installed: " + Request.Result.packageId);
					InstallNext();
				}
				else if (Request.Status >= StatusCode.Failure)
				{
					Debug.Log(Request.Error.message);
				}
				EditorApplication.update -= Progress;
			}
		}

		
		static STFInstaller()
		{
			Install();
		}
	}
}

#endif
