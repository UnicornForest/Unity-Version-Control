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
	
	// Staged files and diff show/hide
	private bool showStagedFiles;
	private bool showDiff;
	
	// Filters
	private FileState stagedFilesFilter = (FileState)1;
	private FileState workingTreeFilter = (FileState)1;
	private List<VCFile> filteredStagedFiles = new List<VCFile>();
	private List<VCFile> filteredWorkingTree = new List<VCFile>();
	
	// GUI styles
	private GUIStyle blankScrollbar;
	private GUIStyle selectionStyle;
	
	// GUI content
	private GUIContent versionControlTypeLogo;
	
	// Controls initialization of the GUI style
	private bool initGUIStyle;
	
	// GUISkin
	private GUISkin versionControlSkin;
	
	// The last selected file index
	private int lastSelectedIndex;
	
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
		
		LoadSkin();
	}
	
	void LoadSkin()
	{
		versionControlSkin = EditorGUIUtility.isProSkin ? Resources.Load("_VersionControlSkin_Default_Dark") as GUISkin : Resources.Load("_VersionControlSkin_Default") as GUISkin;
	
		versionControlTypeLogo = VersionControl.versionControlType == VersionControlType.Git ? new GUIContent(Resources.Load("Logos_Git_Small") as Texture2D) : new GUIContent(Resources.Load("Logos_Hg_Small") as Texture2D);
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
		
		blankScrollbar = new GUIStyle(GUI.skin.verticalScrollbar);
		blankScrollbar.fixedWidth = 0;
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
		
		// Show staged files
		showStagedFiles = EditorPrefs.GetBool("UnityVersionControl.ShowStagedFiles", true);
		
		// Show diff
		showDiff = EditorPrefs.GetBool("UnityVersionControl.ShowDiff", true);
			
	}
	
	void OnGUI()
	{	
		GUI.enabled = BrowserUtility.guiEnabled;
		
		if (!initGUIStyle)
		{
			InitializeStyle();
			
			LoadSettings();
		}
		
		scrollPosition1 = GUILayout.BeginScrollView(scrollPosition1, false, false, GUI.skin.horizontalScrollbar, blankScrollbar, GUILayout.Height(110));
				
		#region Main button row
		GUILayout.BeginHorizontal();
	
		DisplayButtons();
		
		GUILayout.EndHorizontal();
		#endregion
		
		GUILayout.Space(12);
		GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(3));
		
		#region Repository location, initialization
		GUILayout.BeginHorizontal();
		if (!string.IsNullOrEmpty(BrowserUtility.repositoryLocation))
		{
			GUILayout.Label(versionControlTypeLogo);
			
			GUILayout.Space(10);
			
			GUILayout.Label(BrowserUtility.repositoryShortName);
			
			
			
			
			GUILayout.FlexibleSpace();
			
			/* TODO: Put this in the settings window
			 *  
			if (viewMode != BrowserViewMode.ArtistMode)
			{
				if (GUILayout.Button("Reinitialize"))
					BrowserUtility.OnButton_Init(this);
			}
			*/
			
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
			
			// Todo, make initialization UI prettier
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

		
		GUILayout.EndScrollView();
		
		
		mainScrollPosition = GUILayout.BeginScrollView(mainScrollPosition);
		
		GUILayout.BeginHorizontal();
		if (viewMode != BrowserViewMode.ArtistMode && showDiff)
		{
			GUILayout.BeginVertical(GUILayout.Width(horizontalResizeWidget1 - 3));
		}
		else
		{
			GUILayout.BeginVertical();
		}
		
		#region First scroll area - Staged files (git)
		if (VersionControl.versionControlType == VersionControlType.Git && viewMode != BrowserViewMode.ArtistMode && showStagedFiles)
		{
			GUILayout.BeginVertical(GUILayout.Height(verticalResizeWidget1 - 3));
			
			GUILayout.BeginHorizontal();
			GUILayout.Label("Staged files", EditorStyles.toolbarButton);
			stagedFilesFilter = (FileState)EditorGUILayout.EnumMaskField(stagedFilesFilter, EditorStyles.toolbarPopup, GUILayout.Width(100));
		
			GUILayout.EndHorizontal();
			
			scrollPosition2 = GUILayout.BeginScrollView(scrollPosition2, false, false);
			GUILayout.BeginVertical();
			
			GUILayout.BeginHorizontal();
			GUI.backgroundColor *= .5f;
			GUILayout.Label("State", EditorStyles.toolbarButton, GUILayout.Width(80));
			GUILayout.Label("File", EditorStyles.toolbarButton, GUILayout.Width(300));
			GUILayout.Label("Path", EditorStyles.toolbarButton, GUILayout.ExpandWidth(true));
			GUI.backgroundColor = Color.white;
			GUILayout.EndHorizontal();
			
			filteredStagedFiles.Clear();
			filteredStagedFiles.AddRange(BrowserUtility.stagedFiles.Values);
			
			FilterFileList(filteredStagedFiles, true);
			
			for(int i = 0; i < filteredStagedFiles.Count; i++)
			{
				DisplayFile(filteredStagedFiles[i], i, true, filteredStagedFiles);
			}
			
			GUILayout.EndVertical();
			GUILayout.EndScrollView();
			
			GUILayout.EndVertical();
			
			BrowserUtility.ProcessArrowKeyEvents(ref lastSelectedIndex, filteredStagedFiles, filteredWorkingTree);

		}
		#endregion
		
		#region Resize widget
		if (VersionControl.versionControlType == VersionControlType.Git && viewMode != BrowserViewMode.ArtistMode && showStagedFiles)
		{
			Rect r = new Rect(position.x, position.y - 220, position.width, position.height);
			bool drag = GUIHelpers.ResizeWidget(drag1, ref verticalResizeWidget1, 60, position.height - 180, 6, true, r, this);
			
			if (drag != drag1)
			{
				drag1 = drag;
				EditorPrefs.SetFloat("UnityVersionControl.VWidget1", verticalResizeWidget1);	
			}
		}
		#endregion
		
		#region Second scroll area - Working tree
		if (VersionControl.versionControlType == VersionControlType.Git && viewMode != BrowserViewMode.ArtistMode && showStagedFiles)
		{
			GUILayout.BeginVertical(GUILayout.Height(position.height - 126 - verticalResizeWidget1 - 3));
		}
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Working tree", EditorStyles.toolbarButton);
		workingTreeFilter = (FileState)EditorGUILayout.EnumMaskField(workingTreeFilter, EditorStyles.toolbarPopup, GUILayout.Width(100));
		GUILayout.EndHorizontal();
		
		scrollPosition3 = GUILayout.BeginScrollView(scrollPosition3, false, false);
		GUILayout.BeginVertical();
		
		GUILayout.BeginHorizontal();
		GUI.backgroundColor *= .5f;
		GUILayout.Label("State", EditorStyles.toolbarButton, GUILayout.Width(80));
		if (viewMode != BrowserViewMode.ArtistMode)
		{
			GUILayout.Label("File", EditorStyles.toolbarButton, GUILayout.Width(300));
		}
		GUILayout.Label("Path", EditorStyles.toolbarButton, GUILayout.ExpandWidth(true));
		GUI.backgroundColor = Color.white;
		GUILayout.EndHorizontal();
		
		filteredWorkingTree.Clear();
		filteredWorkingTree.AddRange(BrowserUtility.workingTree.Values);
		
		FilterFileList(filteredWorkingTree, false);
		
		for(int i = 0; i < filteredWorkingTree.Count; i++)
		{
			DisplayFile(filteredWorkingTree[i], i, false, filteredWorkingTree);
		}
		
		GUILayout.EndVertical();
		GUILayout.EndScrollView();
		if (VersionControl.versionControlType == VersionControlType.Git && viewMode != BrowserViewMode.ArtistMode && showStagedFiles)
		{
			GUILayout.EndVertical();
		}
		#endregion
		
		GUILayout.EndVertical();
		
		#region Resize widget
		if (viewMode != BrowserViewMode.ArtistMode && showDiff)
		{
			bool drag = GUIHelpers.ResizeWidget(drag2, ref horizontalResizeWidget1, 80, position.width - 80, 4, false, position, this);
			
			if (drag != drag2)
			{
				drag2 = drag;
				EditorPrefs.SetFloat("UnityVersionControl.HWidget1", horizontalResizeWidget1);	
			}
		}
		#endregion
		
		#region 3rd scroll area - Diff
		if (viewMode != BrowserViewMode.ArtistMode && showDiff)
		{
			GUILayout.BeginVertical(GUILayout.Width(position.width - horizontalResizeWidget1));
			
			GUILayout.BeginHorizontal();
			GUILayout.Label("Diff", EditorStyles.toolbarButton, GUILayout.ExpandWidth(true));
			GUILayout.EndHorizontal();
			
			scrollPosition4 = GUILayout.BeginScrollView(scrollPosition4, false, false);
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
		
		GUI.enabled = BrowserUtility.guiEnabled;
	}
	
	void DisplayButtons()
	{	
		if (viewMode == BrowserViewMode.ArtistMode)
		{
			GUILayout.FlexibleSpace();
		}
		
		if (GUILayout.Button("Commit", versionControlSkin.GetStyle("Buttons_Main_Commit"), GUILayout.Width(64), GUILayout.Height(64)))
			BrowserUtility.OnButton_Commit(this);
				
		if (viewMode != BrowserViewMode.ArtistMode)
		{
			GUI.enabled = false;
			GUI.color *= .75f;
			
			if (GUILayout.Button("Checkout", versionControlSkin.GetStyle("Buttons_Main_Checkout"), GUILayout.Width(64), GUILayout.Height(64)))
				BrowserUtility.OnButton_Checkout(this);
			
			GUI.color = Color.white;
			GUI.enabled = BrowserUtility.guiEnabled;
		}
				
		if (GUILayout.Button("Reset", versionControlSkin.GetStyle("Buttons_Main_Reset"), GUILayout.Width(64), GUILayout.Height(64)))
			BrowserUtility.OnButton_Reset(this);
		
		if (viewMode != BrowserViewMode.ArtistMode)
		{
			GUI.enabled = false;
			GUI.color *= .75f;
			
			if (GUILayout.Button("Stash", versionControlSkin.GetStyle("Buttons_Main_Stash"), GUILayout.Width(64), GUILayout.Height(64)))
				Debug.Log("not implemented");
			
			GUI.color = Color.white;
			GUI.enabled = BrowserUtility.guiEnabled;
			
			GUI.enabled = BrowserUtility.guiEnabled && BrowserUtility.workingTreeSelected;
			
			if (GUILayout.Button("Add", versionControlSkin.GetStyle("Buttons_Main_Add"), GUILayout.Width(64), GUILayout.Height(64)))
				BrowserUtility.OnButton_Add(this);
			
			GUI.enabled = BrowserUtility.guiEnabled && BrowserUtility.anyFileSelected;
		
			if (GUILayout.Button("Remove", versionControlSkin.GetStyle("Buttons_Main_Remove"), GUILayout.Width(64), GUILayout.Height(64)))
				BrowserUtility.OnButton_Remove(this);
			
			GUI.enabled = BrowserUtility.guiEnabled;
		
			if (GUILayout.Button("Fetch", versionControlSkin.GetStyle("Buttons_Main_Fetch"), GUILayout.Width(64), GUILayout.Height(64)))
				BrowserUtility.OnButton_Fetch(this);
		}
		
		if (GUILayout.Button("Pull", versionControlSkin.GetStyle("Buttons_Main_Pull"), GUILayout.Width(64), GUILayout.Height(64)))
			BrowserUtility.OnButton_Pull(this);
		
		if (GUILayout.Button("Push", versionControlSkin.GetStyle("Buttons_Main_Push"), GUILayout.Width(64), GUILayout.Height(64)))
			BrowserUtility.OnButton_Push(this);
		
		if (viewMode != BrowserViewMode.ArtistMode)
		{	
			if (GUILayout.Button("Branch", versionControlSkin.GetStyle("Buttons_Main_Branch"), GUILayout.Width(64), GUILayout.Height(64)))
				BrowserUtility.OnButton_Branch(this);
			
			GUI.enabled = false;
			GUI.color *= .75f;
			
			if (GUILayout.Button("Merge", versionControlSkin.GetStyle("Buttons_Main_Merge"), GUILayout.Width(64), GUILayout.Height(64)))
				BrowserUtility.OnButton_Merge(this);
			
			if (GUILayout.Button("Tag",  versionControlSkin.GetStyle("Buttons_Main_Tag"), GUILayout.Width(64), GUILayout.Height(64)))
				BrowserUtility.OnButton_Tag(this);
			
			GUI.color = Color.white;
			GUI.enabled = BrowserUtility.guiEnabled;
		}
		
		GUILayout.FlexibleSpace();
		
		if (viewMode != BrowserViewMode.ArtistMode)
		{
			GUI.enabled = false;
			GUI.color *= .75f;
			
			if (GUILayout.Button("Settings", versionControlSkin.GetStyle("Buttons_Main_Settings"), GUILayout.Width(64), GUILayout.Height(64)))
				BrowserUtility.OnButton_Settings(this);
			
			GUI.color = Color.white;
			GUI.enabled = BrowserUtility.guiEnabled;
		}
	}
	
	void FilterFileList(List<VCFile> fileList, bool staged)
	{
		for(int i = fileList.Count - 1; i >= 0; i--)
		{
			// Check filter
			if (staged)
			{
				if (!stagedFilesFilter.Has(fileList[i].fileState1))
				{
					fileList.RemoveAt(i);
					continue;
				}
			}
			else
			{
				if (!workingTreeFilter.Has(fileList[i].fileState2))
				{
					fileList.RemoveAt(i);
					continue;
				}
			}
		}
	}
	
	void DisplayFile(VCFile file, int index, bool staged, List<VCFile> filteredList)
	{	
		if (index % 2 == 0)
			GUI.backgroundColor = Color.gray;
		else
			GUI.backgroundColor = Color.white;
		
		GUILayout.BeginHorizontal();
		string statusString = file.fileState1 != FileState.Unmodified ? file.fileState1.ToString() : file.fileState2.ToString();
		string filePathString = !string.IsNullOrEmpty(file.path2) ? file.path2 : file.path1;
		bool t1 = GUILayout.Toggle(file.selected, statusString, selectionStyle, GUILayout.Width(75));
		bool t2 = false;
		if (viewMode != BrowserViewMode.ArtistMode)
		{
			string fileNameString = !string.IsNullOrEmpty(file.name2) ? file.name2 : file.name1;
			t2 = GUILayout.Toggle(file.selected, fileNameString, selectionStyle, GUILayout.Width(295));
		}
		bool t3 = GUILayout.Toggle(file.selected, filePathString, selectionStyle, GUILayout.ExpandWidth(true));
		GUILayout.EndHorizontal();
		
		GUI.backgroundColor = Color.white;
		
		if (viewMode != BrowserViewMode.ArtistMode && t2 != file.selected)
		{
			BrowserUtility.ValidateSelection(file, t2, index, lastSelectedIndex, filteredList);
			if (file.selected)
			{
				lastSelectedIndex = index;
			}
		}
		else if (t3 != file.selected)
		{
			BrowserUtility.ValidateSelection(file, t3, index, lastSelectedIndex, filteredList);
			lastSelectedIndex = file.selected ? index : -1;
		}
		else if (t1 != file.selected)
		{
			BrowserUtility.ValidateSelection(file, t1, index, lastSelectedIndex, filteredList);
			lastSelectedIndex = file.selected ? index : -1;
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
		
		if (viewMode != BrowserViewMode.ArtistMode)
		{
			if (VersionControl.versionControlType == VersionControlType.Git)
			{
				string stagedString = BrowserUtility.stagedFiles.Count > 0 ? "S (" + BrowserUtility.stagedFiles.Count + ")" : "S";
				bool showStagedFilesTemp = GUILayout.Toggle(showStagedFiles, stagedString, EditorStyles.toolbarButton, GUILayout.Width(40));
				
				if (showStagedFilesTemp != showStagedFiles)
				{
					showStagedFiles = showStagedFilesTemp;
					EditorPrefs.SetBool("UnityVersionControl.ShowStagedFiles", showStagedFiles);
					verticalResizeWidget1 = Mathf.Clamp(verticalResizeWidget1, 60, position.height - 180);
					Repaint();
				}
			}
			bool showDiffTemp = GUILayout.Toggle(showDiff, "D", EditorStyles.toolbarButton, GUILayout.Width(40));
			
			if (showDiffTemp != showDiff)
			{
				showDiff = showDiffTemp;
				EditorPrefs.SetBool("UnityVersionControl.ShowDiff", showDiff);
				horizontalResizeWidget1 = Mathf.Clamp(horizontalResizeWidget1, 80, position.width - 80);
				Repaint();
			}	
		}
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
	
	public void OnProcessStart()
	{
		BrowserUtility.guiEnabled = false;
	}
	
	public void OnProcessStop(int errorCode, string stdout, string stderr)
	{
		BrowserUtility.guiEnabled = true;
		Repaint();
		if (errorCode == 0)
		{
			BrowserUtility.stagedFiles.Clear();
			BrowserUtility.workingTree.Clear();
		}
		BrowserUtility.ForceUpdate();
		AssetDatabase.Refresh();
	}
	
	public void OnClosePopup()
	{
		BrowserUtility.guiEnabled = true;
		Repaint();
	}
	
	void OnProjectChange()
	{
		if (BrowserUtility.guiEnabled)
		{
			BrowserUtility.ForceUpdate();
			Repaint();
		}
	}
}