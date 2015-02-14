// -------------------------------------------------------
//  Created by Andrew Witte.
// -------------------------------------------------------

using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using Reign;

using System.Xml;
using System.Xml.Linq;
using System.Linq;
using System.IO;
using System.Text;
using System;
using System.Collections.Generic;

namespace Reign.EditorTools
{
	public static class Tools
	{
		#region PostBuildTools
		static void addPostProjectCompilerDirectives(XDocument doc)
		{
			foreach (var element in doc.Root.Elements())
			{
				if (element.Name.LocalName != "PropertyGroup") continue;
				foreach (var subElement in element.Elements())
				{
					if (subElement.Name.LocalName == "DefineConstants")
					{
						// make sure we need to add compiler directive
						bool needToAdd = true;
						foreach (var name in subElement.Value.Split(';', ' '))
						{
							if (name == "REIGN_POSTBUILD")
							{
								needToAdd = false;
								break;
							}
						}

						// add compiler directive
						if (needToAdd) subElement.Value += ";REIGN_POSTBUILD";
					}
				}
			}
		}

		static void addPostProjectReferences(XDocument doc, string pathToBuiltProject, string extraPath, string productName, string extraRefValue)
		{
			XElement sourceElementRoot = null;
			foreach (var element in doc.Root.Elements())
			{
				if (element.Name.LocalName != "ItemGroup") continue;
				foreach (var subElement in element.Elements())
				{
					if (subElement.Name.LocalName == "Compile")
					{
						sourceElementRoot = element;
						break;
					}
				}

				if (sourceElementRoot != null) break;
			}

			if (sourceElementRoot != null)
			{
				var csSources = new string[]
				{
					"Shared/WinRT/WinRTPlugin.cs"
				};

				foreach (var source in csSources)
				{
					// copy cs file
					string sourcePath = string.Format("{0}/{1}/{2}", Application.dataPath, "Plugins/Reign", source);
					string sourceFileName = Path.GetFileName(source);
					File.Copy(sourcePath, string.Format("{0}/{1}{2}/{3}", pathToBuiltProject, productName, extraPath, sourceFileName), true);

					// make sure we need to reference the file
					bool needToRefFile = true;
					foreach (var element in sourceElementRoot.Elements())
					{
						if (element.Name.LocalName == "Compile")
						{
							foreach (var a in element.Attributes())
							{
								if (a.Name.LocalName == "Include" && a.Value == sourceFileName)
								{
									needToRefFile = false;
									break;
								}
							}
						}

						if (!needToRefFile) break;
					}

					// add reference to cs proj
					if (needToRefFile)
					{
						var name = XName.Get("Compile", doc.Root.GetDefaultNamespace().NamespaceName);
						var newSource = new XElement(name);
						newSource.SetAttributeValue(XName.Get("Include"), extraRefValue + sourceFileName);
						sourceElementRoot.Add(newSource);
					}
				}
			}
			else
			{
				Debug.LogError("Reign Post Build Error: Failed to find CS source element in proj!");
			}
		}

		[PostProcessBuild]
		static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
		{
			#if UNITY_5
			if (target == BuildTarget.WSAPlayer || target == BuildTarget.WP8Player)
			#else
			if (target == BuildTarget.MetroPlayer || target == BuildTarget.WP8Player)
			#endif
			{
				var productName = PlayerSettings.productName.Replace(" ", "").Replace("_", "");
				
				#if UNITY_5
				if (EditorUserBuildSettings.wsaSDK == WSASDK.UniversalSDK81 && EditorUserBuildSettings.activeBuildTarget != BuildTarget.WP8Player)
				#else
				if (EditorUserBuildSettings.metroSDK == MetroSDK.UniversalSDK81 && EditorUserBuildSettings.activeBuildTarget != BuildTarget.WP8Player)
				#endif
				{
					var projPath = string.Format("{0}/{1}/{1}.Shared/{1}.Shared.projItems", pathToBuiltProject, productName);
					Debug.Log("Modifing Proj: " + projPath);
					var doc = XDocument.Load(projPath);
					addPostProjectReferences(doc, pathToBuiltProject, string.Format("/{0}.Shared", productName), productName, "$(MSBuildThisFileDirectory)");
					doc.Save(projPath);

					projPath = string.Format("{0}/{1}/{1}.Windows/{1}.Windows.csproj", pathToBuiltProject, productName);
					Debug.Log("Modifing Proj: " + projPath);
					doc = XDocument.Load(projPath);
					addPostProjectCompilerDirectives(doc);
					doc.Save(projPath);

					projPath = string.Format("{0}/{1}/{1}.WindowsPhone/{1}.WindowsPhone.csproj", pathToBuiltProject, productName);
					Debug.Log("Modifing Proj: " + projPath);
					doc = XDocument.Load(projPath);
					addPostProjectCompilerDirectives(doc);
					doc.Save(projPath);
				}
				else
				{
					var projPath = string.Format("{0}/{1}/{1}.csproj", pathToBuiltProject, productName);
					Debug.Log("Modifing Proj: " + projPath);

					var doc = XDocument.Load(projPath);
					addPostProjectCompilerDirectives(doc);
					addPostProjectReferences(doc, pathToBuiltProject, "", productName, "");
					doc.Save(projPath);
				}
			}
    	}
		#endregion
	}
}