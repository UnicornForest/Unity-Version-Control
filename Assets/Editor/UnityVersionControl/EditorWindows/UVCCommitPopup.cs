// Commit Popup Editor Window
// UVCCommitPopup.cs
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
/// The commit popup editor window.
/// </summary>
/// TODO: Add a list of previous user commit messages (store in EditorPrefs, to keep it from being versioned)
/// TODO: Add a toggle to push commits
public class UVCCommitPopup : EditorWindow
{
	// Up to 30 old commit messages
	private const int oldCommitsMaxLength = 30;
	
	private UVCBrowser browser;
	private string commitMessage = string.Empty;
	public string[] oldCommits;
	public int oldCommitSelection = 0;
	private bool cancel = false;
	private bool showOutput = false;
	private bool amend = false;

	/// <summary>
	/// Initialize the commit popup.
	/// </summary>
	/// <param name='browser'>
	/// The main browser instance.
	/// </param>
	public static void Init(UVCBrowser browser)
	{
		var window = EditorWindow.CreateInstance<UVCCommitPopup>();
		window.title = "Commit Changes";
		window.browser = browser;
		window.ShowUtility();
	}
	
	void OnEnable()
	{
		this.minSize = new Vector2(350, 200);
		LoadOldCommits();
	}
	
	void LoadOldCommits()
	{
		string old = EditorPrefs.GetString("UnityVersionControl.OldCommitMessages", null);
		
		var oldCommitsList = new List<string>();
		oldCommitsList.Add(string.Empty);
		
		if (old != null)
		{
			oldCommitsList.AddRange(old.Split('\0'));
		}
		
		oldCommits = oldCommitsList.ToArray();
		
	}
	
	void OnGUI()
	{
		if (browser != null)
		{
			GUILayout.Label("Commit Message:");
			
			commitMessage = EditorGUILayout.TextArea(commitMessage, GUILayout.ExpandHeight(true));
			
			oldCommits[0] = commitMessage;
			
			int selection = EditorGUILayout.Popup(oldCommitSelection, oldCommits, GUILayout.ExpandWidth(true));
			
			if (selection != oldCommitSelection)
			{
				selection = oldCommitSelection;
				commitMessage = oldCommits[oldCommitSelection];
			}
			
			GUILayout.Space(6);
			
			showOutput = GUILayout.Toggle(showOutput, "Show output");
			amend = GUILayout.Toggle(amend, "Amend previous commit");
			
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("OK"))
			{
				EditorPrefs.SetString("UnityVersionControl.OldCommitMessages", string.Join("\0", oldCommits));
				this.Close();
				if (amend)
				{
					if (EditorUtility.DisplayDialog(
						"Warning - changing history",
			            "You have chosen to amend the previous commit - this alters history and will cause problems if you have already pushed the previous commit to a remote. Are you sure you want to continue?", "Ok", "Cancel"))
					{
						UVCProcessPopup.Init(VersionControl.Commit(CommandLine.EmptyHandler, commitMessage.ToLiteral(), true, BrowserUtility.selectedFileCache), !showOutput, true, browser.OnProcessStop);
					}
				}
				else
				{
					UVCProcessPopup.Init(VersionControl.Commit(CommandLine.EmptyHandler, commitMessage.ToLiteral(), false, BrowserUtility.selectedFileCache), !showOutput, true, browser.OnProcessStop);
				}
			}
			GUILayout.Space(10);
			if (GUILayout.Button("Cancel"))
			{
				cancel = true;
				this.Close();
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		}
		else
		{
			cancel = true;
			this.Close();
		}
	}
	
	void OnDestroy()
	{
		if (cancel && browser)
		{
			browser.OnCancelWindow();
		}
	}
}