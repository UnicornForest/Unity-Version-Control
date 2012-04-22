// File Browser Editor Window
// UVCBrowser.cs
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
using System.Collections.Generic;

/// <summary>
/// The main browser editor window.
/// </summary>
/// TODO: Ability to resize different panels and colums, where appropriate.
/// TODO: Filter for different file states
/// TODO: Make it "pretty" - button graphics, custom GUI style, etc.
/// TODO: Unity display errors when doing large operations
public class UVCBrowser : EditorWindow
{
	// Scroll view positions
	private Vector2 scrollPosition1;
	private Vector2 scrollPosition2;
	private Vector2 scrollPosition3;
	private Vector2 scrollPosition4;
	
	// Selection GUI style
	private GUIStyle selectionStyle;
	
	// Controls initialization of the GUI style
	private bool initGUIStyle;
	
	// Controls the overall GUI
	private bool guiEnabled = true;
	
	[MenuItem ("Version Control/Browser")]
	static void Init()
	{
		EditorWindow.GetWindow<UVCBrowser>("Version Control");
	}
	
	void OnEnable()
	{
		BrowserUtility.OnEnable();
		
		this.minSize = new Vector2(600, 300);
		initGUIStyle = false;
	}
	
	void Update()
	{
		BrowserUtility.Update();
	}
	
	// Initializes the selection GUI style
	void InitializeStyle()
	{
		initGUIStyle = true;
		selectionStyle = new GUIStyle(EditorStyles.boldLabel);
		selectionStyle.fontStyle = FontStyle.Normal;
		selectionStyle.padding = new RectOffset(0,0,0,0);
		selectionStyle.font = EditorStyles.label.font;
		selectionStyle.normal.background = new Texture2D(1, 1);
		selectionStyle.normal.background.SetPixel(1, 1, new Color(.25f, .25f, .25f, .25f));
		selectionStyle.normal.background.Apply();
	}
	
	void OnGUI()
	{	
		GUI.enabled = guiEnabled;
		
		if (!initGUIStyle)
		{
			InitializeStyle();
		}
		
		scrollPosition1 = GUILayout.BeginScrollView(scrollPosition1, GUILayout.Height(110));
		
		#region Repository location, initialization
		GUILayout.BeginHorizontal();
		
		if (!string.IsNullOrEmpty(BrowserUtility.repositoryLocation))
		{
			GUILayout.Label("Project: " + BrowserUtility.repositoryLocation);
			
			GUILayout.FlexibleSpace();
			
			if (GUILayout.Button("Reinitialize"))
				BrowserUtility.OnButton_Init(this);
		}
		else
		{
			GUI.color = Color.red;
			GUILayout.Label("No project repository found!");
			GUI.color = Color.white;
			
			GUILayout.FlexibleSpace();
			
			if (GUILayout.Button("Initialize"))
				BrowserUtility.OnButton_Init(this);
		}

		GUILayout.EndHorizontal();
		#endregion
		
		#region Main button row
		GUILayout.BeginHorizontal();
	
		DisplayButtons();
		
		GUILayout.EndHorizontal();
		#endregion
		
		GUILayout.EndScrollView();
		
		
		GUILayout.BeginHorizontal();
		GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
		
		#region First scroll area - Staged files (git)
		if (VersionControl.versionControlType == VersionControlType.Git)
		{
			GUILayout.Label("Staged files", EditorStyles.toolbarButton);
			
			scrollPosition2 = GUILayout.BeginScrollView(scrollPosition2, false, true);
			GUILayout.BeginVertical();
			
			GUILayout.BeginHorizontal();
			GUI.backgroundColor *= .5f;
			GUILayout.Label("State", EditorStyles.toolbarButton, GUILayout.Width(80));
			GUILayout.Label("File", EditorStyles.toolbarButton, GUILayout.Width(300));
			GUILayout.Label("Path", EditorStyles.toolbarButton, GUILayout.ExpandWidth(true));
			GUI.backgroundColor = Color.white;
			GUILayout.EndHorizontal();
			
			var keys1 = new List<string>(BrowserUtility.stagedFiles.Keys);
			int i = 0;
			foreach(var file in keys1)
			{
				DisplayFile(BrowserUtility.stagedFiles[file], i);
				
				i++;
			}
			
			GUILayout.EndVertical();
			GUILayout.EndScrollView();
		}
		#endregion
		
		#region Second scroll area - Working tree
		GUILayout.Label("Working tree", EditorStyles.toolbarButton);
		
		scrollPosition3 = GUILayout.BeginScrollView(scrollPosition3, false, true);
		GUILayout.BeginVertical();
		
		GUILayout.BeginHorizontal();
		GUI.backgroundColor *= .5f;
		GUILayout.Label("State", EditorStyles.toolbarButton, GUILayout.Width(80));
		GUILayout.Label("File", EditorStyles.toolbarButton, GUILayout.Width(300));
		GUILayout.Label("Path", EditorStyles.toolbarButton, GUILayout.ExpandWidth(true));
		GUI.backgroundColor = Color.white;
		GUILayout.EndHorizontal();
		
		var keys2 = new List<string>(BrowserUtility.workingTree.Keys);
		int j = 0;
		foreach(var file in keys2)
		{
			DisplayFile(BrowserUtility.workingTree[file], j);
			
			j++;
		}
		
		GUILayout.EndVertical();
		GUILayout.EndScrollView();
		#endregion
		
		GUILayout.EndVertical();
		
		GUILayout.BeginVertical(GUILayout.Width(500));
		
		#region 3rd scroll area - Diff
		GUILayout.Label("Diff", EditorStyles.toolbarButton, GUILayout.ExpandWidth(true));
		scrollPosition4 = GUILayout.BeginScrollView(scrollPosition4, false, true);
		GUILayout.Label(BrowserUtility.diffString);
		GUILayout.EndScrollView();
		#endregion
		
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();
		
		#region Status bar
		DisplayStatusBar();
		#endregion
		
		GUI.enabled = guiEnabled;
	}
	
	void DisplayButtons()
	{
		GUI.enabled = BrowserUtility.stagedFiles.Count > 0;
		
		if (GUILayout.Button("Commit", GUILayout.Width(70), GUILayout.Height(60)))
			BrowserUtility.OnButton_Commit(this);
		
		GUI.enabled = guiEnabled;
		
		if (GUILayout.Button("Checkout", GUILayout.Width(70), GUILayout.Height(60)))
			BrowserUtility.OnButton_Checkout(this);
		
		if (GUILayout.Button("Reset", GUILayout.Width(70), GUILayout.Height(60)))
			BrowserUtility.OnButton_Reset(this);
		
		GUI.enabled = guiEnabled && BrowserUtility.workingTreeSelected;
		
		if (GUILayout.Button("Add", GUILayout.Width(70), GUILayout.Height(60)))
			BrowserUtility.OnButton_Add(this);
		
		GUI.enabled = guiEnabled && BrowserUtility.anyFileSelected;
		
		if (GUILayout.Button("Remove", GUILayout.Width(70), GUILayout.Height(60)))
			BrowserUtility.OnButton_Remove(this);
		
		GUI.enabled = guiEnabled;
		
		if (GUILayout.Button("Fetch", GUILayout.Width(70), GUILayout.Height(60)))
			BrowserUtility.OnButton_Fetch(this);
		
		if (GUILayout.Button("Pull", GUILayout.Width(70), GUILayout.Height(60)))
			BrowserUtility.OnButton_Pull(this);
		
		if (GUILayout.Button("Push", GUILayout.Width(70), GUILayout.Height(60)))
			BrowserUtility.OnButton_Push(this);
		
		if (GUILayout.Button("Branch", GUILayout.Width(70), GUILayout.Height(60)))
			BrowserUtility.OnButton_Branch(this);
		
		if (GUILayout.Button("Tag", GUILayout.Width(70), GUILayout.Height(60)))
			BrowserUtility.OnButton_Tag(this);
		
		GUILayout.FlexibleSpace();
		
		if (GUILayout.Button("Settings", GUILayout.Width(70), GUILayout.Height(60)))	
			BrowserUtility.OnButton_Settings(this);
	}
	
	void DisplayFile(VCFile file, int index)
	{
		if (index % 2 == 0)
			GUI.backgroundColor = Color.gray;
		else
			GUI.backgroundColor = Color.white;
		
		GUILayout.BeginHorizontal();
		string statusString = file.fileState1 != FileState.Unmodified ? file.fileState1.ToString() : file.fileState2.ToString();
		string fileNameString = !string.IsNullOrEmpty(file.name2) ? file.name2 : file.name1;
		string filePathString = !string.IsNullOrEmpty(file.path2) ? file.path2 : file.path1;
		bool t1 = GUILayout.Toggle(file.selected, statusString, selectionStyle, GUILayout.Width(75));
		bool t2 = GUILayout.Toggle(file.selected, fileNameString, selectionStyle, GUILayout.Width(295));
		bool t3 = GUILayout.Toggle(file.selected, filePathString, selectionStyle, GUILayout.ExpandWidth(true));
		GUILayout.EndHorizontal();
		
		GUI.backgroundColor = Color.white;
		
		if (t2 != file.selected)
		{
			BrowserUtility.ValidateSelection(file, t2);
		}
		else if (t3 != file.selected)
		{
			BrowserUtility.ValidateSelection(file, t3);
		}
		else if (t1 != file.selected)
		{
			BrowserUtility.ValidateSelection(file, t1);
		}	
	}
	
	void DisplayStatusBar()
	{
		var sb = new System.Text.StringBuilder();
		bool clean = true;
		
		if (BrowserUtility.addedFileCount > 0)
		{
			sb.Append(BrowserUtility.addedFileCount).Append(" added");
			clean = false;
		}
		
		if (BrowserUtility.copiedFileCount > 0)
		{
			sb.Append(' ').Append(BrowserUtility.copiedFileCount).Append(" copied");
			clean = false;
		}
		
		if (BrowserUtility.deletedFileCount > 0)
		{
			sb.Append(' ').Append(BrowserUtility.deletedFileCount).Append(" deleted");
			clean = false;
		}
		
		if (BrowserUtility.modifiedFileCount > 0)
		{
			sb.Append(' ').Append(BrowserUtility.modifiedFileCount).Append(" modified");
			clean = false;
		}
		
		if (BrowserUtility.renamedFileCount > 0)
		{
			sb.Append(' ').Append(BrowserUtility.renamedFileCount).Append(" renamed");
			clean = false;
		}
		
		if (BrowserUtility.unmergedFileCount > 0)
		{
			sb.Append(' ').Append(BrowserUtility.unmergedFileCount).Append(" unmerged");
			clean = false;
		}
		
		if (BrowserUtility.untrackedFileCount > 0)
		{
			sb.Append(' ').Append(BrowserUtility.untrackedFileCount).Append(" untracked");
			clean = false;
		}
		
		if (clean)
			sb.Append("Clean");
		
		GUILayout.Label(sb.ToString(), EditorStyles.toolbarButton, GUILayout.ExpandWidth(true));
	}

	public void OnProcessStart()
	{
		guiEnabled = false;
	}
	
	public void OnProcessStop(int errorCode)
	{
		guiEnabled = true;
		BrowserUtility.ForceUpdate();
		Repaint();
	}
	
	public void OnCancelWindow()
	{
		guiEnabled = true;
		Repaint();
	}
}