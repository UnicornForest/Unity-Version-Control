// Branch Popup Editor Window
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
/// The branch popup editor window.
/// </summary>
public class UVCBranchPopup : EditorWindow
{
	private UVCBrowser browser;
	private bool showOutput;
	private int selectionGrid;
	private string[] selectionText = new string[3]{"Create New Branch", "Delete Local Branches", "Delete Remote Branches"};
	
	private Vector2 scrollVector;
	
	// Creating a branch
	private string newBranch = string.Empty;
	private bool checkoutNewBranch;
	
	// Deleting a branch
	private bool[] branchToggles;
	private string[] remoteBranches;
	private bool[] remoteBranchToggles;
	private bool forceDelete;
	private int currentRemoteIndex;
	
	private List<string> branchList = new List<string>();
	
	/// <summary>
	/// Initialize the branch popup.
	/// </summary>
	/// <param name='browser'>
	/// The main browser instance.
	/// </param>
	public static void Init(UVCBrowser browser)
	{
		var window = EditorWindow.CreateInstance<UVCBranchPopup>();
		window.title = "Branch";
		window.browser = browser;
		window.ShowUtility();
	}
	
	void OnEnable()
	{
		this.minSize = new Vector2(600, 160);
		this.maxSize = new Vector2(600, 160);
		branchToggles = new bool[BrowserUtility.localBranchNames.Length];
		
		RefreshBranches();
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
		remoteBranchToggles = new bool[remoteBranches.Length];
	}
	
	void OnGUI()
	{
		if (browser != null)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label("", EditorStyles.toolbarButton, GUILayout.ExpandWidth(true));
			int sel = GUILayout.SelectionGrid(selectionGrid, selectionText, 3, EditorStyles.toolbarButton);
			GUILayout.Label("", EditorStyles.toolbarButton, GUILayout.ExpandWidth(true));
			GUILayout.EndHorizontal();
			
			if (sel != selectionGrid)
			{
				selectionGrid = sel;
				if (sel == 0)
				{
					this.minSize = new Vector2(600, 160);
					this.maxSize = new Vector2(600, 160);
					Repaint();
				}
				else
				{
					this.minSize = new Vector2(600, 200);
					this.maxSize = new Vector2(Screen.currentResolution.width, Screen.currentResolution.height);
					Repaint();
				}
			}
			if (selectionGrid == 0)
			{
				EditorGUILayout.LabelField("Current Branch:",BrowserUtility.localBranchNames[BrowserUtility.localBranchIndex]);
				newBranch = EditorGUILayout.TextField("New Branch:", newBranch);
				checkoutNewBranch = EditorGUILayout.Toggle("Checkout new branch", checkoutNewBranch);
				
				GUILayout.Space(6);
			}
			else
			{
				if (selectionGrid == 2)
				{
					int index = EditorGUILayout.Popup("Delete from:", currentRemoteIndex, BrowserUtility.remoteNames);
					if (index != currentRemoteIndex)
					{
						currentRemoteIndex = index;
						RefreshBranches();
					}
				}
				GUILayout.Label("Branches to delete:");
				
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				GUILayout.Label("", EditorStyles.toolbarButton, GUILayout.Width(54));
				GUILayout.Label("Branch Name", EditorStyles.toolbarButton, GUILayout.MinWidth(154));
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
				
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				scrollVector = GUILayout.BeginScrollView(scrollVector, GUILayout.MinWidth(250), GUILayout.ExpandHeight(true));
				GUILayout.BeginVertical();
				
				int ii = selectionGrid == 1 ? branchToggles.Length : remoteBranchToggles.Length;
				
				for(int i = 0; i < ii; i++)
				{
					
					GUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					
					GUILayout.Space(20);
					
					if (selectionGrid == 1)
					{
						branchToggles[i] = GUILayout.Toggle(branchToggles[i], "", GUILayout.Width(30));
					}
					else
					{
						remoteBranchToggles[i] = GUILayout.Toggle(remoteBranchToggles[i], "", GUILayout.Width(30));
					}
					
					string n = selectionGrid == 1 ? BrowserUtility.localBranchNames[i] : remoteBranches[i];
					
					GUILayout.Label(n, GUILayout.MinWidth(150));
					
					GUILayout.FlexibleSpace();
					GUILayout.EndHorizontal();
				}
				GUILayout.EndVertical();
				GUILayout.EndScrollView();
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
				
				GUILayout.Space(12);
				
				if (selectionGrid == 1)
				{
					forceDelete = GUILayout.Toggle(forceDelete, "Force delete regardless of merge status");
				}
			}
			
			showOutput = GUILayout.Toggle(showOutput, "Show output");
			
			GUILayout.Space(6);
			
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			
			if (GUILayout.Button("OK"))
			{	
				this.Close();
				
				if (selectionGrid == 0)
				{
					UVCProcessPopup.Init(VersionControl.CreateBranch(CommandLine.EmptyHandler, newBranch, checkoutNewBranch), !showOutput, true, browser.OnProcessStop, true);
				}
				else if (selectionGrid == 1)
				{
					branchList.Clear();
					
					for(int i = 0; i < branchToggles.Length; i++)
					{
						if (branchToggles[i])
							branchList.Add(BrowserUtility.localBranchNames[i]);
					}
					
					UVCProcessPopup.Init(VersionControl.DeleteLocalBranches(CommandLine.EmptyHandler, branchList.ToArray(), forceDelete), !showOutput, true, browser.OnProcessStop, true);
				}
				else
				{
					branchList.Clear();
					
					for(int i = 0; i < remoteBranchToggles.Length; i++)
					{
						if (remoteBranchToggles[i])
							branchList.Add(remoteBranches[i]);
					}
					
					string[] blanks = new string[branchList.Count];
					
					for(int i = 0; i < blanks.Length; i++)
					{
						blanks[i] = string.Empty;
					}
					
					UVCProcessPopup.Init(VersionControl.Push(CommandLine.EmptyHandler, BrowserUtility.remoteNames[currentRemoteIndex], blanks, branchList.ToArray(), false), !showOutput, true, browser.OnProcessStop, true);
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