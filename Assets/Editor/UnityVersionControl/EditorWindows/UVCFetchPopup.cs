// Fetch Popup Editor Window
// UVCFetchPopup.cs
// Unity Version Control
//  
// Authors:
//       Josh Montoute <josh@thinksquirrel.com>
// 
// Copyright (c) 2012, Thinksquirrel Software, LLC
//
// This file is part of Unity Version Control.
//
//    Unity Version Control is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    Unity Version Control is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with Unity Version Control.  If not, see <http://www.gnu.org/licenses/>.
//
using UnityEditor;
using UnityEngine;
using ThinksquirrelSoftware.UnityVersionControl.Core;
using ThinksquirrelSoftware.UnityVersionControl.UserInterface;
using ThinksquirrelSoftware.UnityVersionControl.Helpers;
using System.Collections.Generic;

/// <summary>
/// The fetch popup editor window.
/// </summary>
/// TODO: Fetching from a remote that isn't being tracked
public class UVCFetchPopup : EditorWindow
{
	private UVCBrowser browser;
	private bool allRemotes = true;
	private bool prune = false;
	private bool showOutput = false;
	private int currentRemoteIndex;
	
	/// <summary>
	/// Initialize the fetch popup.
	/// </summary>
	/// <param name='browser'>
	/// The main browser instance.
	/// </param>
	public static void Init(UVCBrowser browser)
	{
		var window = EditorWindow.CreateInstance<UVCFetchPopup>();
		window.title = "Fetch";
		window.browser = browser;
		window.ShowUtility();
	}
	
	void OnEnable()
	{
		this.minSize = new Vector2(350, 150);
		this.maxSize = new Vector2(350, 150);
	}
	
	void OnGUI()
	{
		if (browser != null)
		{
			allRemotes = GUILayout.Toggle(allRemotes, "Fetch from all remotes");
			
			GUI.enabled = !allRemotes;
			
			currentRemoteIndex = EditorGUILayout.Popup("Fetch from", currentRemoteIndex, BrowserUtility.remoteNames);
				
			GUI.enabled = true;
			
			prune = GUILayout.Toggle(prune, "Prune tracking branches no longer present on remote(s)");
			showOutput = GUILayout.Toggle(showOutput, "Show output");
			
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			
			if (GUILayout.Button("OK"))
			{	
				this.Close();
				
				if (allRemotes)
				{
					UVCProcessPopup.Init(VersionControl.Fetch(CommandLine.EmptyHandler, "--all", prune), !showOutput, true, browser.OnProcessStop, true);
				}
				else
				{
					UVCProcessPopup.Init(VersionControl.Fetch(CommandLine.EmptyHandler, BrowserUtility.remoteNames[currentRemoteIndex], prune), !showOutput, true, browser.OnProcessStop, true);
				}
			}
			GUILayout.Space(10);
			if (GUILayout.Button("Cancel"))
			{
				this.Close();
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		}
		else
		{
			this.Close();
		}
	}
	
	void OnDestroy()
	{
		if (browser)
		{
			browser.OnClosePopup();
		}
	}
}