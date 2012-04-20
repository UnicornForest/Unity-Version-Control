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
	/// TODO: Should this class be static?
	public static class BrowserUtility
	{	
		#region Member fields
		/// The frame count and update rate for updates
		private static int mFrameCount;
		private const int mUpdateRate = 600;
		
		/// The repository location
		private static string mRepositoryLocation;
		
		// Staged files and working tree
		private static Dictionary<string, VCFile> mStagedFiles = new Dictionary<string, VCFile>();
		private static Dictionary<string, VCFile> mWorkingTree = new Dictionary<string, VCFile>();
		
		// Diff
		private static string mDiffString;
		
		// Stats
		private static int mModifiedFileCount;
		private static int mAddedFileCount;
		private static int mDeletedFileCount;
		private static int mRenamedFileCount;
		private static int mCopiedFileCount;
		private static int mUnmergedFileCount;
		private static int mUntrackedFileCount;
		#endregion
		
		#region Public properties
		public static string repositoryLocation
		{
			get
			{
				return mRepositoryLocation;
			}
		}
		public static Dictionary<string, VCFile> stagedFiles
		{
			get
			{
				return mStagedFiles;
			}
		}
		public static Dictionary<string, VCFile> workingTree
		{
			get
			{
				return mWorkingTree;
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
		#endregion
		
		#region Public methods
		/// <summary>
		/// Runs on EditorWindow.OnEnable().
		/// </summary>
		public static void OnEnable()
		{
			mFrameCount = 0;
			mRepositoryLocation = Git.RepositoryLocation();
		}
		
		/// <summary>
		/// Runs on EditorWindow.Update().
		/// </summary>
		public static void Update()
		{
			if (mFrameCount % mUpdateRate == 0)
			{
				UpdateTrees();	
			}
			
			mFrameCount++;
		}
		 
		/// <summary>
		/// Updates the diff panel by running git diff on selected files.
		/// </summary>
		// TODO: Update for new system
		public static void UpdateDiffPanel()
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
		#endregion
		
		#region Private methods
		/// Updates all tree listings by running three git commands.
		private static void UpdateTrees()
		{
			if (string.IsNullOrEmpty(mRepositoryLocation))
			{
				mRepositoryLocation = Git.RepositoryLocation();
			}
			
			// Get list of files
			VersionControl.FindFiles(OnFindFiles);
		}
		
		// TODO: Handle git errors
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
			
			// Addition
			foreach(var file in files)
			{
				// Staged files
				if (file.fileState1 != FileState.Unmodified && file.fileState1 != FileState.Untracked && file.fileState1 != FileState.Ignored)
				{
					// Check for duplicate
					if (!stagedFiles.ContainsKey(file.path1 + file.path2))
					{
						// Add value
						stagedFiles.Add(file.path1 + file.path2, file);
					}
				}
				
				// Working tree
				if (file.fileState2 != FileState.Unmodified && file.fileState2 != FileState.Untracked && file.fileState2 != FileState.Ignored)
				{
					// Check for duplicate
					if (!workingTree.ContainsKey(file.path1 + file.path2))
					{
						// Add value
						workingTree.Add(file.path1 + file.path2, file);
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
			
			// Update stats
			UpdateStats();
		}
		
		// TODO: Handle git errors
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
		#endregion
	}
}