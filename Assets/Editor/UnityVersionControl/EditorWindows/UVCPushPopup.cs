// Push Popup Editor Window
// UVCPushPopup.cs
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
/// TODO: Tracking and untracking branches
/// TODO: Pushing to custom locations
/// TODO: Pushing new branches
public class UVCPushPopup : EditorWindow
{
	private UVCBrowser browser;
	private int currentRemoteIndex;
	private bool[] pushToggles;
	private int[] remoteBranchIndices;
	private string[] remoteBranches;
	private List<string> branchList = new List<string>();
	private bool selectAll;
	private bool pushAllTags = true;
	private bool showOutput;
	
	private Vector2 scrollVector;
	
	/// <summary>
	/// Initialize the pull popup.
	/// </summary>
	/// <param name='browser'>
	/// The main browser instance.
	/// </param>
	public static void Init(UVCBrowser browser)
	{
		var window = EditorWindow.CreateInstance<UVCPushPopup>();
		window.title = "Push";
		window.browser = browser;
		window.ShowUtility();
	}
	
	void OnEnable()
	{
		this.minSize = new Vector2(600, 300);
		
		remoteBranchIndices = new int[BrowserUtility.localBranchNames.Length];
		pushToggles = new bool[remoteBranchIndices.Length];
		for(int j = 0; j < pushToggles.Length; j++)
		{
			pushToggles[j] = true;
		}
		RefreshBranches();
		SetCurrentBranches();
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
		
		remoteBranches = branchList.ToArray();
	}
	
	void SetCurrentBranches()
	{
		for(int j = 0; j < remoteBranchIndices.Length; j++)
		{
			if (selectAll)
			{
				pushToggles[j] = true;
			}
			
			// Try to find the first matching name, otherwise set it to the first branch
			for(int i = 0; i < remoteBranches.Length; i++)
			{
				if (remoteBranches[i].Equals(BrowserUtility.localBranchNames[j]))
				{
					remoteBranchIndices[j] = i;
					break;
				}
			}
		}
	}
	
	void OnGUI()
	{	
		if (browser != null)
		{	
			int index = EditorGUILayout.Popup("Push to:", currentRemoteIndex, BrowserUtility.remoteNames);
			if (index != currentRemoteIndex)
			{
				currentRemoteIndex = index;
				RefreshBranches();
				SetCurrentBranches();
			}
			
			GUILayout.Space(12);
			
			GUILayout.Label("Branches to push:");
			
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label("Push", EditorStyles.toolbarButton, GUILayout.Width(54));
			GUILayout.Label("Local Branch", EditorStyles.toolbarButton, GUILayout.Width(254));
			GUILayout.Label("Remote Branch", EditorStyles.toolbarButton, GUILayout.Width(254));
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			scrollVector = GUILayout.BeginScrollView(scrollVector, GUILayout.ExpandHeight(true));
			GUILayout.BeginVertical();
			for(int i = 0; i < remoteBranchIndices.Length; i++)
			{
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				GUILayout.Space(20);
				pushToggles[i] = GUILayout.Toggle(pushToggles[i], "", GUILayout.Width(30));
				GUILayout.Label(BrowserUtility.localBranchNames[i], GUILayout.Width(250));
				GUILayout.Space(10);
				remoteBranchIndices[i] = EditorGUILayout.Popup(remoteBranchIndices[i], remoteBranches, GUILayout.Width(240));
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
			}
			GUILayout.EndVertical();
			GUILayout.EndScrollView();
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			
			GUILayout.Space(12);
			
			bool sel = GUILayout.Toggle(selectAll, "Select All");
					
			if (sel != selectAll)
			{
				selectAll = sel;
				for(int j = 0; j < pushToggles.Length; j++)
				{
					pushToggles[j] = selectAll;
				}
			}
			
			pushAllTags = GUILayout.Toggle(pushAllTags, "Push all tags");
			
			showOutput = GUILayout.Toggle(showOutput, "Show output");
			
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			
			if (GUILayout.Button("OK", GUILayout.Width(100)))
			{	
				this.Close();				
				
				var localNameList = new List<string>();
				var remoteNameList = new List<string>();
				
				for(int i = 0; i < BrowserUtility.localBranchNames.Length; i++)
				{
					if (pushToggles[i])
					{
						localNameList.Add(BrowserUtility.localBranchNames[i]);
						remoteNameList.Add(remoteBranches[remoteBranchIndices[i]]);
					}
				}
				
				UVCProcessPopup.Init(VersionControl.Push(CommandLine.EmptyHandler, BrowserUtility.remoteNames[currentRemoteIndex], localNameList.ToArray(), remoteNameList.ToArray(), pushAllTags), !showOutput, true, browser.OnProcessStop, true);

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