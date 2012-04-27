// Pull Popup Editor Window
// UVCPullPopup.cs
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
/// The pull popup editor window.
/// </summary>
/// TODO: Pulling from a remote that isn't being tracked
public class UVCPullPopup : EditorWindow
{
	private UVCBrowser browser;
	private bool commit = true;
	private bool includeOldMessages = false;
	private bool commitFastForward = false;
	private bool rebase = false;
	private bool showOutput;
	private int currentRemoteIndex;
	private int currentBranchIndex;
	private string[] currentBranches;
	private List<string> branchList = new List<string>();
	
	/// <summary>
	/// Initialize the pull popup.
	/// </summary>
	/// <param name='browser'>
	/// The main browser instance.
	/// </param>
	public static void Init(UVCBrowser browser)
	{
		var window = EditorWindow.CreateInstance<UVCPullPopup>();
		window.title = "Pull";
		window.browser = browser;
		window.ShowUtility();
	}
	
	void OnEnable()
	{
		this.minSize = new Vector2(550, 220);
		this.maxSize = new Vector2(550, 220);
		RefreshBranches();
		SetCurrentBranch();
	}
	
	void RefreshBranches()
	{
		branchList.Clear();
		
		foreach(var branch in BrowserUtility.branches)
		{
			if (branch.isRemote)
			{
				if (branch.remoteName.Equals(BrowserUtility.remoteNames[currentRemoteIndex]))
					branchList.Add(branch.name);
			}
		}
		
		currentBranches = branchList.ToArray();
	}
	
	void SetCurrentBranch()
	{
		currentBranchIndex = 0;
		
		// Try to find the first matching name, otherwise set it to the first branch
		for(int i = 0; i < currentBranches.Length; i++)
		{
			if (currentBranches[i].Equals(BrowserUtility.localBranchNames[BrowserUtility.localBranchIndex]))
			{
				currentBranchIndex = i;
				return;
			}
		}
	}
	
	void OnGUI()
	{	
		if (browser != null)
		{	
			int i = EditorGUILayout.Popup("Pull from", currentRemoteIndex, BrowserUtility.remoteNames);
			if (i != currentRemoteIndex)
			{
				currentRemoteIndex = i;
				RefreshBranches();
				SetCurrentBranch();
			}
			
			currentBranchIndex = EditorGUILayout.Popup("Remote branch to pull", currentBranchIndex, currentBranches);
			
			GUILayout.Label("Pulling into local branch: " + BrowserUtility.localBranchNames[BrowserUtility.localBranchIndex]);
			
			GUILayout.Space(12);
			
			commit = GUILayout.Toggle(commit, "Commit merged changes immediately");	
			includeOldMessages = GUILayout.Toggle(includeOldMessages, "Include messages from commits being merged in merge commit");	
			commitFastForward = GUILayout.Toggle(commitFastForward, "Create new commit even if fast-forward merge");
			rebase = GUILayout.Toggle(rebase, "Rebase instead of merge (WARNING: Make sure you haven't pushed your changes)");
			showOutput = GUILayout.Toggle(showOutput, "Show output");
			
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			
			if (GUILayout.Button("OK", GUILayout.Width(100)))
			{	
				this.Close();
				
				UVCProcessPopup.Init(VersionControl.Pull(CommandLine.EmptyHandler, BrowserUtility.remoteNames[currentRemoteIndex], currentBranches[currentBranchIndex], commit, includeOldMessages, commitFastForward, rebase), !showOutput, true, browser.OnProcessStop, true);
			}
			GUILayout.Space(10);
			if (GUILayout.Button("Cancel", GUILayout.Width(100)))
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