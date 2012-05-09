// Unity Version Control Preference Item
// UVCPreferences.cs
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

/// <summary>
/// Provides a preference item for Unity Version Control
/// </summary>
public static class UVCPreferences
{	
	const string defaultWindows = "Notepad";
	const string defaultOSX = "TextEdit";
	
	private static string[] defaultTextEditor;
	private static int index;
	
	public static string GetDefaultTextEditor()
	{
		string d = defaultWindows;
		
		if (Application.platform == RuntimePlatform.OSXEditor)
			d = defaultOSX;
		
		return EditorPrefs.GetString("UnityVersionControl.DefaultTextEditor", d);
	}
		
	[PreferenceItem("Version Control")]
	public static void PreferencesGUI()
	{
		GUILayout.Space(10);
		LoadPreferences();
		index = EditorGUILayout.Popup("Default Text Editor", index, defaultTextEditor);
		
		if (index == 2)
		{
			if (Application.platform == RuntimePlatform.WindowsEditor)
			{
				string path = EditorUtility.OpenFilePanel("Select Default Text Editor", Application.dataPath, "exe");
				if (!string.IsNullOrEmpty(path))
				{
					EditorPrefs.SetString("UnityVersionControl.DefaultTextEditor", path);
					defaultTextEditor[0] =  System.IO.Path.GetFileNameWithoutExtension(path.Substring(path.LastIndexOf("\\") + 1));
					EditorPrefs.SetString("UnityVersionControl.DefaultTextEditorString", defaultTextEditor[0]);
					index = 0;
				}
				else if (!string.IsNullOrEmpty(defaultTextEditor[0]))
				{
					index = 0;
				}
				else
				{
					index = 1;
				}
			}
			else if (Application.platform == RuntimePlatform.OSXEditor)
			{
				string path = EditorUtility.OpenFilePanel("Select Default Text Editor", Application.dataPath, "app");
				if (!string.IsNullOrEmpty(path))
				{
					EditorPrefs.SetString("UnityVersionControl.DefaultTextEditor", path);
					defaultTextEditor[0] = System.IO.Path.GetFileNameWithoutExtension(path.Substring(path.LastIndexOf("/") + 1));
					EditorPrefs.SetString("UnityVersionControl.DefaultTextEditorString", defaultTextEditor[0]);
					index = 0;
				}
				else if (!string.IsNullOrEmpty(defaultTextEditor[0]))
				{
					index = 0;
				}
				else
				{
					index = 1;
				}
			}
		}
		else if (index == 1)
		{
			EditorPrefs.SetString("UnityVersionControl.DefaultTextEditor", defaultTextEditor[1]);
		}
	}	
	private static void LoadPreferences()
	{
		defaultTextEditor = new string[3];
		
		// Windows - default to Notepad
		if (Application.platform == RuntimePlatform.WindowsEditor)
		{
			defaultTextEditor[0] = EditorPrefs.GetString("UnityVersionControl.DefaultTextEditorString", null);
			defaultTextEditor[1] = defaultWindows;
		}
		// OSX - default to TextEdit
		else if (Application.platform == RuntimePlatform.OSXEditor)
		{
			defaultTextEditor[0] = EditorPrefs.GetString("UnityVersionControl.DefaultTextEditorString", null);
			defaultTextEditor[1] = defaultOSX;
		}
		
		defaultTextEditor[2] = "Other...";
		
		if (string.IsNullOrEmpty(defaultTextEditor[0]))
		{
			index = 1;
		}
	}
}