// Settings Popup Editor Window
// UVCSettingsPopup.cs
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
/// The settings popup editor window.
/// </summary>
public class UVCSettingsPopup : EditorWindow
{
	private UVCBrowser browser;

	/// <summary>
	/// Initialize the settings popup.
	/// </summary>
	/// <param name='browser'>
	/// The main browser instance.
	/// </param>
	public static void Init(UVCBrowser browser)
	{
		var window = EditorWindow.CreateInstance<UVCSettingsPopup>();
		window.title = "Settings";
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
			if (GUILayout.Button("Open repository ignore file"))
				CommandLine.OpenFileInTextEditor(VersionControl.RepositoryIgnoreFile());
			
			GUILayout.FlexibleSpace();
			
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			
			if (GUILayout.Button("OK", GUILayout.Width(100)))
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
			browser.EnableGUI();
		}
	}
}