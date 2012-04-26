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
using ThinksquirrelSoftware.UnityVersionControl.Helpers;
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
	// View mode
	private BrowserViewMode viewMode;
	
	// Scroll view positions
	private Vector2 mainScrollPosition;
	private Vector2 scrollPosition1;
	private Vector2 scrollPosition2;
	private Vector2 scrollPosition3;
	private Vector2 scrollPosition4;
	
	// Resize widget locations
	private float horizontalResizeWidget1;
	private float verticalResizeWidget1;
	private bool drag1 = false;
	private bool drag2 = false;
	
	// Filters
	private FileState stagedFilesFilter = (FileState)1;
	private FileState workingTreeFilter = (FileState)1;
	
	// GUI styles
	private GUIStyle selectionStyle;
	private GUIStyle vertWidgetStyle;
	
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
		
		if (position.width < this.minSize.x || position.height < this.minSize.y)
		{
			position = new Rect(position.x, position.y, this.minSize.x, this.minSize.y);
		}
	}
	
	void Update()
	{
		BrowserUtility.Update();
	}
	
	// Initialize the GUI styles
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
		
		vertWidgetStyle = new GUIStyle(EditorStyles.toolbarButton);
		vertWidgetStyle.fixedHeight = 0;
	}
	
	void LoadSettings()
	{
		// View Mode
		viewMode = (BrowserViewMode)EditorPrefs.GetInt("UnityVersionControl.BrowserViewMode", (int)BrowserViewMode.Default);
		
		// Staged Files Filter
		stagedFilesFilter = (FileState)EditorPrefs.GetInt("UnityVersionControl.StagedFilesFilter",
			(int)(FileState.Added | FileState.Copied | FileState.Deleted | FileState.Modified | FileState.Renamed | FileState.Unmerged | FileState.Unmodified | FileState.Untracked | FileState.Ignored));

		// Working Tree Filter
		workingTreeFilter = (FileState)EditorPrefs.GetInt("UnityVersionControl.WorkingTreeFilter",
			(int)(FileState.Added | FileState.Copied | FileState.Deleted | FileState.Modified | FileState.Renamed | FileState.Unmerged | FileState.Untracked));
		
		// Diff Widget
		horizontalResizeWidget1 = Mathf.Clamp(EditorPrefs.GetFloat("UnityVersionControl.HWidget1", position.width - 500), 80, position.width - 80);
		
		// Tree/Index Widget
		verticalResizeWidget1 = Mathf.Clamp(EditorPrefs.GetFloat("UnityVersionControl.VWidget1", (position.height - 220) / 2), 60, position.height - 180);
			
	}
	
	void OnGUI()
	{	
		GUI.enabled = guiEnabled;
		
		if (!initGUIStyle)
		{
			InitializeStyle();
			
			LoadSettings();
		}
		
		scrollPosition1 = GUILayout.BeginScrollView(scrollPosition1, GUILayout.Height(110));
		
		#region Repository location, initialization
		GUILayout.BeginHorizontal();
		
		if (!string.IsNullOrEmpty(BrowserUtility.repositoryLocation))
		{
			GUILayout.Label("Project: " + BrowserUtility.repositoryLocation);
			
			GUILayout.FlexibleSpace();
			
			if (viewMode != BrowserViewMode.Mini)
			{
				if (GUILayout.Button("Reinitialize"))
					BrowserUtility.OnButton_Init(this);
			}
			
			var vm = (BrowserViewMode)EditorGUILayout.EnumPopup(viewMode);
			
			if (vm != viewMode)
			{
				viewMode = vm;
				EditorPrefs.SetInt("UnityVersionControl.BrowserViewMode", (int)viewMode);
			}
		}
		else
		{
			GUI.color = Color.red;
			GUILayout.Label("No project repository found!");
			GUI.color = Color.white;
			
			GUILayout.FlexibleSpace();
			
			if (GUILayout.Button("Initialize"))
				BrowserUtility.OnButton_Init(this);
			
			var vm = (BrowserViewMode)EditorGUILayout.EnumPopup(viewMode);
			
			if (vm != viewMode)
			{
				viewMode = vm;
				EditorPrefs.SetInt("UnityVersionControl.BrowserViewMode", (int)viewMode);
			}
		}

		GUILayout.EndHorizontal();
		#endregion
		
		#region Main button row
		GUILayout.BeginHorizontal();
	
		DisplayButtons();
		
		GUILayout.EndHorizontal();
		#endregion
		
		GUILayout.EndScrollView();
		
		
		mainScrollPosition = GUILayout.BeginScrollView(mainScrollPosition);
		
		GUILayout.BeginHorizontal();
		if (viewMode != BrowserViewMode.Mini)
		{
			GUILayout.BeginVertical(GUILayout.Width(horizontalResizeWidget1 - 3));
		}
		else
		{
			GUILayout.BeginVertical();
		}
		
		#region First scroll area - Staged files (git)
		if (VersionControl.versionControlType == VersionControlType.Git && viewMode != BrowserViewMode.Mini)
		{
			GUILayout.BeginVertical(GUILayout.Height(verticalResizeWidget1 - 3));
			
			GUILayout.BeginHorizontal();
			GUILayout.Label("Staged files", EditorStyles.toolbarButton);
			GUILayout.Label("Filter", EditorStyles.toolbarButton, GUILayout.Width(40));
			stagedFilesFilter = (FileState)EditorGUILayout.EnumMaskField(stagedFilesFilter, EditorStyles.toolbarPopup, GUILayout.Width(100));
			GUILayout.EndHorizontal();
			
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
				DisplayFile(BrowserUtility.stagedFiles[file], ref i, true);
				
				i++;
			}
			
			GUILayout.EndVertical();
			GUILayout.EndScrollView();
			GUILayout.EndVertical();
		}
		#endregion
		
		#region Resize widget
		if (VersionControl.versionControlType == VersionControlType.Git && viewMode != BrowserViewMode.Mini)
		{
			GUI.backgroundColor = new Color(.5f, .5f, .5f, 1);
			if (GUILayout.RepeatButton("", EditorStyles.toolbarButton, GUILayout.Height(6), GUILayout.ExpandWidth(true)))
			{
				BeginDrag(true);
			}
			EditorGUIUtility.AddCursorRect (GUILayoutUtility.GetLastRect(), MouseCursor.ResizeVertical);
			GUI.backgroundColor = Color.white;
		}
		#endregion
		
		#region Second scroll area - Working tree
		if (VersionControl.versionControlType == VersionControlType.Git && viewMode != BrowserViewMode.Mini)
		{
			GUILayout.BeginVertical(GUILayout.Height(position.height - 126 - verticalResizeWidget1 - 3));
		}
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Working tree", EditorStyles.toolbarButton);
		GUILayout.Label("Filter", EditorStyles.toolbarButton, GUILayout.Width(40));
		workingTreeFilter = (FileState)EditorGUILayout.EnumMaskField(workingTreeFilter, EditorStyles.toolbarPopup, GUILayout.Width(100));
		GUILayout.EndHorizontal();
		
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
			DisplayFile(BrowserUtility.workingTree[file], ref j, false);
			
			j++;
		}
		
		GUILayout.EndVertical();
		GUILayout.EndScrollView();
		if (VersionControl.versionControlType == VersionControlType.Git && viewMode != BrowserViewMode.Mini)
		{
			GUILayout.EndVertical();
		}
		#endregion
		
		GUILayout.EndVertical();
		
		#region Resize widget
		if (viewMode != BrowserViewMode.Mini)
		{
			GUI.backgroundColor *= .5f;
			if (GUILayout.RepeatButton("", vertWidgetStyle, GUILayout.Width(4), GUILayout.ExpandHeight(true)))
			{
				BeginDrag(false);
			}
			EditorGUIUtility.AddCursorRect (GUILayoutUtility.GetLastRect(), MouseCursor.ResizeHorizontal);   
			GUI.backgroundColor = Color.white;
		}
		#endregion
		
		#region 3rd scroll area - Diff
		if (viewMode != BrowserViewMode.Mini)
		{
			GUILayout.BeginVertical(GUILayout.Width(position.width - horizontalResizeWidget1));
			GUILayout.Label("Diff", EditorStyles.toolbarButton, GUILayout.ExpandWidth(true));
			scrollPosition4 = GUILayout.BeginScrollView(scrollPosition4, false, true);
			EditorGUILayout.SelectableLabel(BrowserUtility.diffString, GUILayout.ExpandHeight(true));
			GUILayout.EndScrollView();
			GUILayout.EndVertical();
		}
		#endregion
		
		GUILayout.EndHorizontal();
		
		GUILayout.EndScrollView();
		
		#region Status bar
		DisplayStatusBar();
		#endregion
		
		#region Events
		CheckDrag();
		#endregion
		
		GUI.enabled = guiEnabled;
	}
	
	void DisplayButtons()
	{	
		if (GUILayout.Button("Commit", GUILayout.Width(70), GUILayout.Height(60)))
			BrowserUtility.OnButton_Commit(this);
		
		if (viewMode != BrowserViewMode.Mini)
		{
			if (GUILayout.Button("Checkout", GUILayout.Width(70), GUILayout.Height(60)))
				BrowserUtility.OnButton_Checkout(this);
		}
				
		if (GUILayout.Button("Reset", GUILayout.Width(70), GUILayout.Height(60)))
			BrowserUtility.OnButton_Reset(this);
		
		if (viewMode != BrowserViewMode.Mini)
		{
			GUI.enabled = guiEnabled && BrowserUtility.workingTreeSelected;
			
			if (GUILayout.Button("Add", GUILayout.Width(70), GUILayout.Height(60)))
				BrowserUtility.OnButton_Add(this);
			
			GUI.enabled = guiEnabled && BrowserUtility.anyFileSelected;
		}
		
		if (GUILayout.Button("Remove", GUILayout.Width(70), GUILayout.Height(60)))
			BrowserUtility.OnButton_Remove(this);
		
		GUI.enabled = guiEnabled;
		
		if (viewMode != BrowserViewMode.Mini)
		{
			if (GUILayout.Button("Fetch", GUILayout.Width(70), GUILayout.Height(60)))
				BrowserUtility.OnButton_Fetch(this);
		}
		
		if (GUILayout.Button("Pull", GUILayout.Width(70), GUILayout.Height(60)))
			BrowserUtility.OnButton_Pull(this);
		
		if (GUILayout.Button("Push", GUILayout.Width(70), GUILayout.Height(60)))
			BrowserUtility.OnButton_Push(this);
		
		if (viewMode != BrowserViewMode.Mini)
		{
			if (GUILayout.Button("Branch", GUILayout.Width(70), GUILayout.Height(60)))
				BrowserUtility.OnButton_Branch(this);
			
			if (GUILayout.Button("Merge", GUILayout.Width(70), GUILayout.Height(60)))
				BrowserUtility.OnButton_Merge(this);
			
			if (GUILayout.Button("Tag", GUILayout.Width(70), GUILayout.Height(60)))
				BrowserUtility.OnButton_Tag(this);
		}
		
		GUILayout.FlexibleSpace();
		
		if (viewMode != BrowserViewMode.Mini)
		{
			if (GUILayout.Button("Settings", GUILayout.Width(70), GUILayout.Height(60)))	
				BrowserUtility.OnButton_Settings(this);
		}
	}
	
	void DisplayFile(VCFile file, ref int index, bool staged)
	{
		// Check filter
		if (staged)
		{
			if (!stagedFilesFilter.Has(file.fileState1))
			{
				index--;
				return;
			}
		}
		else
		{
			if (!workingTreeFilter.Has(file.fileState2))
			{
				index--;
				return;
			}
		}
		
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
		GUILayout.BeginHorizontal();
		
		if (BrowserUtility.localBranchNames != null)
		{
			int currentBranch = EditorGUILayout.Popup(BrowserUtility.localBranchIndex, BrowserUtility.localBranchNames, EditorStyles.toolbarPopup, GUILayout.Width(150));
				
			if (currentBranch != BrowserUtility.localBranchIndex)
			{
				DisplaySwitchBranchPopup(currentBranch);
			}
		}
		
		#region status strings
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
		#endregion
		
		GUILayout.Label(sb.ToString(), EditorStyles.toolbarButton, GUILayout.ExpandWidth(true));
		
		GUILayout.EndHorizontal();
	}
	
	void DisplaySwitchBranchPopup(int index)
	{
		if (EditorUtility.DisplayDialog(
						"Confirm Branch Checkout/Clean",
			            "This will CLEAN the project. Any uncommited changes will be irretrievably lost! Are you sure you want to continue?", "Ok", "Cancel"))
		{
			BrowserUtility.localBranchIndex = index;
			BrowserUtility.OnButton_CheckoutBranch(this);
		}
	}
	
	void BeginDrag(bool vertical)
	{
		if (vertical)
			drag1 = true;
		else
			drag2 = true;
	}
	
	void CheckDrag()
	{
		if (drag1 || drag2)
		{
			if (drag1)
			{
				EditorGUIUtility.AddCursorRect (position, MouseCursor.ResizeVertical);
			}
			else
			{
				EditorGUIUtility.AddCursorRect (position, MouseCursor.ResizeHorizontal);
			}
			
			if (Event.current.type == EventType.MouseUp)
			{
				drag1 = false;
				drag2 = false;
				EditorPrefs.SetFloat("UnityVersionControl.VWidget1", verticalResizeWidget1);
				EditorPrefs.SetFloat("UnityVersionControl.HWidget1", horizontalResizeWidget1);
				Repaint();
				return;
			}
			
			if (Event.current.type == EventType.MouseDrag)
			{
				Vector2 delta = Event.current.delta;
				Vector2 pos = Event.current.mousePosition;
				
				if (!position.Contains(pos))
				{
					drag1 = false;
					drag2 = false;
					EditorPrefs.SetFloat("UnityVersionControl.VWidget1", verticalResizeWidget1);
					EditorPrefs.SetFloat("UnityVersionControl.HWidget1", horizontalResizeWidget1);
					Repaint();
					return;
				}
				
				if (drag1)
				{	
					verticalResizeWidget1 = Mathf.Clamp(verticalResizeWidget1 += delta.y, 60, position.height - 180);
				}
				else if (drag2)
				{
					horizontalResizeWidget1 = Mathf.Clamp(horizontalResizeWidget1 += delta.x, 80, position.width - 80);
				}
				Repaint();
			}
		}
	}
	
	public void OnProcessStart()
	{
		guiEnabled = false;
	}
	
	public void OnProcessStop(int errorCode, string stdout, string stderr)
	{
		if (errorCode == 0)
		{
			BrowserUtility.stagedFiles.Clear();
			BrowserUtility.workingTree.Clear();
		}
		BrowserUtility.ForceUpdate();
	}
	
	public void OnClosePopup()
	{
		guiEnabled = true;
		Repaint();
	}
}