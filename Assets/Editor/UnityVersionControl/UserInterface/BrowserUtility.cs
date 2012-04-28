// File Browser Editor Window Functionality
// BrowserUtility.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ThinksquirrelSoftware.UnityVersionControl.Core;

namespace ThinksquirrelSoftware.UnityVersionControl.UserInterface
{
	/// <summary>
	/// Provides functionality for the browser editor window.
	/// </summary>
	/// <remarks>
	/// This seperates interface functionality from design/display.
	/// </remarks>
	public static class BrowserUtility
	{	
		#region Member fields
		/// The frame count and update rate for updates
		private static int mFrameCount;
		private const int mUpdateRate = 250;
		
		// Keyboard delays
		private const int mKeyboardDelay = 5;
		private const int mKeyboardRepeatRate = 1;
		private static int mKeyboardTimer = 0;
		
		/// Repository location
		private static string mRepositoryLocation;
		private static string mRepositoryShortName;
		
		// Branches
		private static VCBranch[] mBranches;
		private static string[] mLocalBranchNames;
		private static string[] mRemoteNames;
		private static int mLocalBranchIndex;
		
		// Staged files and working tree
		private static SortedList<string, VCFile> mStagedFiles = new SortedList<string, VCFile>();
		private static SortedList<string, VCFile> mWorkingTree = new SortedList<string, VCFile>();
		private static bool mStagedFileSelected;
		private static bool mWorkingTreeSelected;
		private static bool mAnyFileSelected;
		
		// Selected files
		private static VCFile[] mSelectedFileCache;
		
		// Diff
		private static string mDiffString = string.Empty;
		
		// Stats
		private static int mModifiedFileCount;
		private static int mAddedFileCount;
		private static int mDeletedFileCount;
		private static int mRenamedFileCount;
		private static int mCopiedFileCount;
		private static int mUnmergedFileCount;
		private static int mUntrackedFileCount;
		
		// GUI
		private static bool mGuiEnabled = true;
		
		#endregion
		
		#region Public properties
		public static string repositoryLocation
		{
			get
			{
				return mRepositoryLocation;
			}
		}
		public static string repositoryShortName
		{
			get
			{
				return mRepositoryShortName;
			}
		}
		public static VCBranch[] branches
		{
			get
			{
				return mBranches;
			}
		}
		public static string[] localBranchNames
		{
			get
			{
				return mLocalBranchNames;
			}
		}
		public static string[] remoteNames
		{
			get
			{
				return mRemoteNames;
			}
		}
		public static int localBranchIndex
		{
			get
			{
				return mLocalBranchIndex;
			}
			set
			{
				mLocalBranchIndex = value;
			}
		}
		public static SortedList<string, VCFile> stagedFiles
		{
			get
			{
				return mStagedFiles;
			}
		}
		public static SortedList<string, VCFile> workingTree
		{
			get
			{
				return mWorkingTree;
			}
		}
		public static VCFile[] selectedFileCache
		{
			get
			{
				return mSelectedFileCache;
			}
		}
		public static bool stagedFileSelected
		{
			get
			{
				return mStagedFileSelected;
			}
		}
		public static bool workingTreeSelected
		{
			get
			{
				return mWorkingTreeSelected;
			}
		}
		public static bool anyFileSelected
		{
			get
			{
				return mAnyFileSelected;
			}
		}
		public static string diffString
		{
			get
			{
				return mDiffString;
			}
		}
		public static int modifiedFileCount
		{
			get
			{
				return mModifiedFileCount;
			}
		}
		public static int addedFileCount
		{
			get
			{
				return mAddedFileCount;
			}
		}
		public static int deletedFileCount
		{
			get
			{
				return mDeletedFileCount;
			}
		}
		public static int renamedFileCount
		{
			get
			{
				return mRenamedFileCount;
			}
		}
		public static int copiedFileCount
		{
			get
			{
				return mCopiedFileCount;
			}
		}
		public static int unmergedFileCount
		{
			get
			{
				return mUnmergedFileCount;
			}
		}
		public static int untrackedFileCount
		{
			get
			{
				return mUntrackedFileCount;
			}
		}
		public static bool guiEnabled
		{
			get
			{
				return mGuiEnabled;
			}
			set
			{
				mGuiEnabled = value;
			}
		}
		#endregion
		
		#region Public methods
		/// <summary>
		/// Runs on EditorWindow.OnEnable().
		/// </summary>
		public static void OnEnable()
		{
			mFrameCount = 0;
			mRepositoryLocation = Git.RepositoryLocation();
			if (!string.IsNullOrEmpty(mRepositoryLocation))
			{
				mRepositoryShortName = mRepositoryLocation.Substring(mRepositoryLocation.LastIndexOf(System.IO.Path.DirectorySeparatorChar) + 1);
			}
		}
		
		/// <summary>
		/// Runs on EditorWindow.Update().
		/// </summary>
		public static void Update()
		{
			if (mFrameCount % mUpdateRate == 0 && mGuiEnabled)
			{
				UpdateTrees();	
			}
			
			mFrameCount++;
		}
		
		/// <summary>
		/// Forces an update.
		/// </summary>
		public static void ForceUpdate()
		{
			mFrameCount = 0;
			Update();
		}
		
		/// <summary>
		/// Raises the Init Button event.
		/// </summary>
		/// TODO: Implement button
		public static void OnButton_Init(UVCBrowser browser)
		{
			browser.OnProcessStart();
			UVCProcessPopup.Init(VersionControl.Initialize(CommandLine.EmptyHandler), true, true, browser.OnProcessStop, true);
		}
		
		/// <summary>
		/// Raises the Commit Button event.
		/// </summary>
		/// TODO: Implement button
		public static void OnButton_Commit(UVCBrowser browser)
		{
			browser.OnProcessStart();
			UVCCommitPopup.Init(browser);
		}
		
		/// <summary>
		/// Raises the Checkout Button event.
		/// </summary>
		/// TODO: Implement button
		public static void OnButton_Checkout(UVCBrowser browser)
		{
		}
		
		/// <summary>
		/// Raises the Checkout Branch Button event.
		/// </summary>
		public static void OnButton_CheckoutBranch(UVCBrowser browser)
		{
			browser.OnProcessStart();
			UVCProcessPopup.Init(VersionControl.Checkout(CommandLine.EmptyHandler, mLocalBranchNames[mLocalBranchIndex], true), false, true, OnCheckoutBranch, true);
		}
		
		/// <summary>
		/// Raises the Reset Button event.
		/// </summary>
		/// TODO: Implement button
		public static void OnButton_Reset(UVCBrowser browser)
		{
			string confirmation = mAnyFileSelected ? "This will undo all changes to the selected files and reset them to the last commit." : "This will undo all changes and reset to the last commit.";
			if (EditorUtility.DisplayDialog(
						"Confirm Reset?",
			            confirmation, "Ok", "Cancel"))
			{
				browser.OnProcessStart();
				UVCProcessPopup.Init(VersionControl.ResetLast(CommandLine.EmptyHandler, mSelectedFileCache), true, true, browser.OnProcessStop, true);
			}
		}
		
		/// <summary>
		/// Raises the Add Button event.
		/// </summary>
		public static void OnButton_Add(UVCBrowser browser)
		{
			browser.OnProcessStart();
			UVCProcessPopup.Init(VersionControl.Add(CommandLine.EmptyHandler, mSelectedFileCache), true, true, browser.OnProcessStop, true);
		}
		
		/// <summary>
		/// Raises the Remove Button event.
		/// </summary>
		/// TODO: <<IMPORTANT>> Implement confirmation before removing modified files from the index
		public static void OnButton_Remove(UVCBrowser browser)
		{
			browser.OnProcessStart();
			if (VersionControl.versionControlType == VersionControlType.Git && mStagedFileSelected)
			{
				// Git only - Clicking the remove button for a staged file will unstage it
				UVCProcessPopup.Init(VersionControl.Reset(CommandLine.EmptyHandler, "HEAD", mSelectedFileCache), true, true, browser.OnProcessStop, true);
			}
			else
			{
				bool dialog = false;
				var modFiles = new System.Text.StringBuilder();
				
				foreach(var file in mSelectedFileCache)
				{
					if (file.fileState2 == FileState.Modified || file.fileState2 == FileState.Untracked)
					{
						dialog = true;
						if (string.IsNullOrEmpty(file.path2))
						{
							modFiles.Append(file.path1).Append('\n');
						}
						else
						{
							modFiles.Append(file.path2).Append('\n');
						}
					}
				}
				
				if (dialog)
				{
					if (EditorUtility.DisplayDialog(
						"Confirm Remove Modified or Untracked Files?",
			            "The following files contain changes or information which is not in source control, and will be irretrievably lost if you remove them:\n" + 
						modFiles.ToString(0, modFiles.Length -1), "Ok", "Cancel"))
					{
						UVCProcessPopup.Init(VersionControl.Remove(CommandLine.EmptyHandler, true, mSelectedFileCache), true, true, browser.OnProcessStop, true);
					}
					else
					{
						browser.OnProcessStop(-9999, null, null);
					}
				}
				else
				{
					UVCProcessPopup.Init(VersionControl.Remove(CommandLine.EmptyHandler, false, mSelectedFileCache), true, true, browser.OnProcessStop, true);
				}
				
			}
		}
		
		/// <summary>
		/// Raises the Fetch Button event.
		/// </summary>
		public static void OnButton_Fetch(UVCBrowser browser)
		{
			browser.OnProcessStart();
			UVCFetchPopup.Init(browser);
		}
		
		/// <summary>
		/// Raises the Pull Button event.
		/// </summary>
		public static void OnButton_Pull(UVCBrowser browser)
		{
			browser.OnProcessStart();
			UVCPullPopup.Init(browser);
		}
		
		/// <summary>
		/// Raises the Push Button event.
		/// </summary>
		public static void OnButton_Push(UVCBrowser browser)
		{
			browser.OnProcessStart();
			UVCPushPopup.Init(browser);
		}
		
		/// <summary>
		/// Raises the Branch Button event.
		/// </summary>
		/// TODO: Implement button
		public static void OnButton_Branch(UVCBrowser browser)
		{
			browser.OnProcessStart();
			UVCBranchPopup.Init(browser);		
		}
		
		/// <summary>
		/// Raises the Merge Button event.
		/// </summary>
		/// TODO: Implement button
		public static void OnButton_Merge(UVCBrowser browser)
		{
		}
		
		/// <summary>
		/// Raises the Tag Button event.
		/// </summary>
		/// TODO: Implement button
		public static void OnButton_Tag(UVCBrowser browser)
		{
		}
		
		/// <summary>
		/// Raises the Settings Button event.
		/// </summary>
		/// TODO: Implement button
		public static void OnButton_Settings(UVCBrowser browser)
		{
		}
		
		/// <summary>
		/// Validates the selection.
		/// </summary>
		public static void ValidateSelection(VCFile file, bool toggle, bool overrideMultiSelect, int index, int lastSelectedIndex, List<VCFile> filteredList)
		{
			if (file != null)
				file.selected = toggle;
			
			foreach(var f in stagedFiles.Values)
			{
				if (f.selected)
				{
					mStagedFileSelected = true;
					break;
				}
			}
			
			foreach(var f in workingTree.Values)
			{
				if (f.selected)
				{
					mWorkingTreeSelected = true;
					break;
				}
			}
			
			bool ctrlPressed = false || overrideMultiSelect;
			
			if (!overrideMultiSelect)
			{
				if (Event.current != null)
				{
					if (Application.platform == RuntimePlatform.WindowsEditor)
						ctrlPressed = Event.current.control;
					else if (Application.platform == RuntimePlatform.OSXEditor)
						ctrlPressed = Event.current.command;
				}
			}
			
			// Only run if selecting a file
			if (file != null && toggle)
			{				
				bool switched = false;
				
				if (stagedFiles.ContainsValue(file))
				{		
					if (!ctrlPressed)
					{
						foreach(var f in stagedFiles.Values)
						{
							if (f != file)
								f.selected = false;
						}	
					}
					
					// Selecting a staged file with working tree files selected (not allowed)
					if (mWorkingTreeSelected)
					{
						switched = true;
						
						foreach(var f in workingTree.Values)
						{
							f.selected = false;
						}
						mWorkingTreeSelected = false;
					}
				}
				else if (workingTree.ContainsValue(file))
				{	
					if (!ctrlPressed)
					{
						foreach(var f in workingTree.Values)
						{
							if (f != file)
								f.selected = false;
						}	
					}
					
					// Selecting a working tree file with staged files selected (not allowed)	
					if (mStagedFileSelected)
					{
						switched = true;
						
						foreach(var f in stagedFiles.Values)
						{
							f.selected = false;
						}
						mStagedFileSelected = false;
					}
				}
				
				// Detect shift clicks
				if (lastSelectedIndex != -1)
				{
					if (Event.current != null && !switched)
					{
						if (Event.current.shift)
						{
							bool goBackwards = lastSelectedIndex > index;
							if (goBackwards)
							{
								for(int i = lastSelectedIndex; i >= index; i--)
								{
									if (i < filteredList.Count)
									{
										filteredList[i].selected = true;
									}
								}
							}
							else
							{
								for(int i = lastSelectedIndex; i <= index; i++)
								{
									if (i < filteredList.Count)
									{
										filteredList[i].selected = true;
									}
								}
							}
						}
					}
				}
			}
			
			mAnyFileSelected = mStagedFileSelected || mWorkingTreeSelected;
			
			CacheSelectedFiles();
			UpdateDiffPanel();
		}
		
		/// <summary>
		/// Processes various keyboard events.
		/// </summary>
		public static void ProcessKeyboardEvents(ref int lastSelectedIndex, List<VCFile> filteredStagedFiles, List<VCFile> filteredWorkingTree)
		{
			if (Event.current != null && lastSelectedIndex != -1)
			{
				// Commands
				if (Event.current.type == EventType.ValidateCommand)
				{
					if (Event.current.commandName == "SelectAll")
					{
						Event.current.Use();
					}
				}
				else if (Event.current.type == EventType.ExecuteCommand)
				{
					// Select all
					if (Event.current.commandName == "SelectAll")
					{	
						if (mStagedFileSelected)
						{
							foreach(var file in filteredStagedFiles)
							{
								file.selected = true;
							}
							lastSelectedIndex = filteredStagedFiles.Count - 1;
							
							Event.current.Use();
						}
						else if (mWorkingTreeSelected)
						{
							foreach(var file in filteredWorkingTree)
							{
								file.selected = true;
							}
							lastSelectedIndex = filteredWorkingTree.Count - 1;
							
							Event.current.Use();
						}
					}
				}
				else if (Event.current.isKey)
				{
					// Released a key
					if (Event.current.type == EventType.KeyUp)
					{
						// Released an arrow key
						if (Event.current.keyCode == KeyCode.UpArrow || Event.current.keyCode == KeyCode.DownArrow)
						{
							Event.current.Use();
							mKeyboardTimer = 0;
						}
					}
					// Up and down
					else if (Event.current.keyCode == KeyCode.UpArrow)
					{
						if (mKeyboardTimer == 0 || (mKeyboardTimer >= mKeyboardDelay && (mKeyboardTimer % mKeyboardRepeatRate == 0)))
						{
							if (mStagedFileSelected)
							{
								if (lastSelectedIndex > 0)
								{
									ValidateSelection(filteredStagedFiles[lastSelectedIndex - 1], true, false, lastSelectedIndex - 1, lastSelectedIndex, filteredStagedFiles);
									lastSelectedIndex--;
								}	
							}
							else if (mWorkingTreeSelected)
							{
								if (lastSelectedIndex > 0)
								{
									ValidateSelection(filteredWorkingTree[lastSelectedIndex - 1], true, false, lastSelectedIndex - 1, lastSelectedIndex, filteredWorkingTree);	
									lastSelectedIndex--;
								}
							}
						}
						
						Event.current.Use();
						mKeyboardTimer++;
					}
					else if (Event.current.keyCode == KeyCode.DownArrow)
					{
						if (mKeyboardTimer == 0 || (mKeyboardTimer >= mKeyboardDelay && (mKeyboardTimer % mKeyboardRepeatRate == 0)))
						{
							if (mStagedFileSelected)
							{
								if (lastSelectedIndex < filteredStagedFiles.Count - 1)
								{
									ValidateSelection(filteredStagedFiles[lastSelectedIndex + 1], true, false, lastSelectedIndex + 1, lastSelectedIndex, filteredStagedFiles);
									lastSelectedIndex++;
								}
									
							}
							else if (mWorkingTreeSelected)
							{
								if (lastSelectedIndex < filteredWorkingTree.Count - 1)
								{
									ValidateSelection(filteredWorkingTree[lastSelectedIndex + 1], true, false, lastSelectedIndex + 1, lastSelectedIndex, filteredWorkingTree);
									lastSelectedIndex++;
								}
							}
						}
						
						Event.current.Use();
						mKeyboardTimer++;
					}
				}
			}
		}
		#endregion
		
		#region Private methods
		private static void CacheSelectedFiles()
		{
			var fileList = new List<VCFile>();
			
			foreach(var file in workingTree.Values)
			{
				if (file.selected)
					fileList.Add(file);
			}
			
			foreach(var file in stagedFiles.Values)
			{
				if (file.selected)
					fileList.Add(file);
			}
			
			mSelectedFileCache = fileList.ToArray();
		}
		
		/// Updates all tree listings by running three git commands.
		private static void UpdateTrees()
		{
			if (string.IsNullOrEmpty(mRepositoryLocation))
			{
				mRepositoryLocation = Git.RepositoryLocation();
			}
			
			// Get list of files
			VersionControl.FindFiles(OnFindFiles);
			
			// Get list of branches
			VersionControl.FindBranches(OnFindBranches);
		}
		
		// TODO: Handle errors (BrowserUtility.OnFindFiles)
		private static void OnFindFiles(object sender, System.EventArgs e)
		{
			var process = sender as System.Diagnostics.Process;
			
			var files = VersionControl.ParseFiles(process.StandardOutput.ReadToEnd());
			
			#region removal
			var toRemove = new List<string>();
			
			// Staged files
			foreach(var kvp in stagedFiles)
			{
				bool keep = false;
				foreach(var file in files)
				{
					if (file.fileState1 != FileState.Unmodified && file.fileState1 != FileState.Untracked && file.fileState1 != FileState.Ignored)
					{
						if (kvp.Key.Equals(file.path1 + file.path2))
						{
							// Found a match
							keep = true;
							break;
						}
					}
				}
				
				// Add to removal queue
				if (!keep)
					toRemove.Add(kvp.Key);
			}
			
			// Perform removal
			foreach(var str in toRemove)
			{
				stagedFiles.Remove(str);
			}
			
			toRemove.Clear();
			
			// Working tree
			foreach(var kvp in workingTree)
			{
				bool keep = false;
				foreach(var file in files)
				{
					if ((file.fileState2 != FileState.Unmodified && file.fileState2 != FileState.Untracked && file.fileState2 != FileState.Ignored) ||
						(file.fileState1 == FileState.Untracked && file.fileState2 == FileState.Untracked) ||
						(file.fileState1 == FileState.Ignored && file.fileState2 == FileState.Ignored))
					{
						if (kvp.Key.Equals(file.path1 + file.path2))
						{
							// Found a match
							keep = true;
							break;
						}
					}
				}
				
				// Add to removal queue
				if (!keep)
					toRemove.Add(kvp.Key);
			}
			
			// Perform removal
			foreach(var str in toRemove)
			{
				workingTree.Remove(str);
			}
			#endregion
			
			#region addition
			foreach(var file in files)
			{
				// Staged files
				if (file.fileState1 != FileState.Unmodified && file.fileState1 != FileState.Untracked && file.fileState1 != FileState.Ignored)
				{
					// Check for duplicate
					if (!stagedFiles.ContainsKey(file.path1 + file.path2))
					{
						// Add file
						stagedFiles.Add(file.path1 + file.path2, file);
					}
				}
				
				// Working tree
				if (file.fileState2 != FileState.Unmodified && file.fileState2 != FileState.Untracked && file.fileState2 != FileState.Ignored)
				{
					// Check for duplicate
					if (!workingTree.ContainsKey(file.path1 + file.path2))
					{
						// Check for duplicate (stagedFiles)
						if (stagedFiles.ContainsValue(file))
						{
							// Add file (new instance)
							workingTree.Add(file.path1 + file.path2, new VCFile(file));
						}
						else
						{
							// Add file
							workingTree.Add(file.path1 + file.path2, file);
						}
					}
				}
				
				// Untracked (added to working tree)
				if (file.fileState1 == FileState.Untracked && file.fileState2 == FileState.Untracked)
				{
					// Check for duplicate
					if (!workingTree.ContainsKey(file.path1 + file.path2))
					{
						// Add value
						workingTree.Add(file.path1 + file.path2, file);
					}
				}
				
				// Ignored (added to working tree)
				if (file.fileState1 == FileState.Ignored && file.fileState2 == FileState.Ignored)
				{
					// Check for duplicate
					if (!workingTree.ContainsKey(file.path1 + file.path2))
					{
						// Add value
						workingTree.Add(file.path1 + file.path2, file);
					}
				}
			}
			#endregion
			
			// Validate selection
			ValidateSelection(null, false, false, -1, -1, null);
			
			// Update stats
			UpdateStats();
		}
		
		// TODO: Handle errors (BrowserUtility.OnFindBranches)
		private static void OnFindBranches(object sender, System.EventArgs e)
		{
			mBranches = VersionControl.ParseBranches((sender as System.Diagnostics.Process).StandardOutput.ReadToEnd());
			
			var tempList = new List<string>();
			var tempList2 = new List<string>();
			
			for(int i = 0; i < mBranches.Length; i++)
			{
				if (!mBranches[i].isRemote)
				{
					tempList.Add(mBranches[i].name);
					if (mBranches[i].isCurrent)
					{
						mLocalBranchIndex = i;
					}
				}
				else
				{
					if (!tempList2.Contains(mBranches[i].remoteName))
					{
						tempList2.Add(mBranches[i].remoteName);
					}
				}
			}
			
			mLocalBranchNames = tempList.ToArray();
			
			mRemoteNames = tempList2.ToArray();
		}
		
		// TODO: "Pretty" diff parsing
		// TODO: Handle errors
		private static void OnGetDiff(object sender, System.EventArgs e)
		{
			mDiffString = (sender as System.Diagnostics.Process).StandardOutput.ReadToEnd();
		}
		
		private static void UpdateStats()
		{
			mAddedFileCount = 0;
			mCopiedFileCount = 0;
			mDeletedFileCount = 0;
			mModifiedFileCount = 0;
			mRenamedFileCount = 0;
			mUnmergedFileCount = 0;
			mUntrackedFileCount = 0;
			
			foreach(var file in mWorkingTree.Values)
			{
				switch(file.fileState2)
				{
				case FileState.Added:
					mAddedFileCount++;
					break;
				case FileState.Copied:
					mCopiedFileCount++;
					break;
				case FileState.Deleted:
					mDeletedFileCount++;
					break;
				case FileState.Modified:
					mModifiedFileCount++;
					break;
				case FileState.Renamed:
					mRenamedFileCount++;
					break;
				case FileState.Unmerged:
					mUnmergedFileCount++;
					break;
				case FileState.Untracked:
					mUntrackedFileCount++;
					break;
				}
			}
		}
		
		private static void UpdateDiffPanel()
		{
			mDiffString = string.Empty;
			
			var fileList = new List<VCFile>();
			
			foreach(var file in mWorkingTree.Values)
			{
				if (file.selected)
				{
					fileList.Add(file);
				}
			}
			
			if (fileList.Count > 0)
				VersionControl.GetDiff(OnGetDiff, fileList.ToArray());
		}
		
		private static void OnCheckoutBranch(int exitCode, string stdout, string stderr)
		{
			VersionControl.RestartUnity();
		}
		#endregion
	}
}