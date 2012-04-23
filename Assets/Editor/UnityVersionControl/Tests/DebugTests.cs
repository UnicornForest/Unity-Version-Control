// Debug test commands
// DebugTests.cs
// Unity Version Control
//  
// Authors:
//       Josh Montoute <josh@thinksquirrel.com>
// 
// Copyright (c) 2012, Thinksquirrel Software, LLC
// All rights reserved.
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
#if DEBUG
using UnityEditor;
using UnityEngine;
using ThinksquirrelSoftware.UnityVersionControl.Core;

namespace ThinksquirrelSoftware.UnityVersionControl.Tests
{
	public static class DebugTests
	{	
		[MenuItem ("Version Control/Debug/Core Tests/Init")]
	    static void CoreTest_Init()
		{
			VersionControl.Initialize(OnProcessExit);
		}
		
		[MenuItem ("Version Control/Debug/Core Tests/File List")]
	    static void CoreTest_FileList()
		{
			Debug.Log("Getting file list...");
			VersionControl.FindFiles(OnFindFiles);
		}
		
		[MenuItem ("Version Control/Debug/Core Tests/Branch List")]
	    static void CoreTest_BranchList()
		{
			Debug.Log("Getting branch list...");
			VersionControl.FindBranches(OnFindBranches);
		}
		
		[MenuItem ("Version Control/Debug/Core Tests/Bad Command")]
	    static void CoreTest_BadCommand()
		{
			if (VersionControl.versionControlType == VersionControlType.Git)
			{
				Git.RunGit("notarealcommand", OnProcessExit);
			}
			else
			{
				Hg.RunHg("notarealcommand", OnProcessExit);
			}
		}
		
		[MenuItem ("Version Control/Debug/Core Tests/Repository Location")]
	    static void CoreTest_RepositoryLocation()
		{
			string message = VersionControl.RepositoryLocation();
			
			EditorUtility.DisplayDialog("Repository Location", message, "Ok", "Cancel");
		}
		
		[MenuItem ("Version Control/Debug/GUI Tests/Init")]
		static void GUITest_Init()
		{
			UVCProcessPopup.Init(VersionControl.Initialize(CommandLine.EmptyHandler), false, true, null, false);
		}
		
		[MenuItem ("Version Control/Debug/GUI Tests/File List")]
		static void GUITest_FileList()
		{
			UVCProcessPopup.Init(VersionControl.FindFiles(CommandLine.EmptyHandler), false, true, OnFindFiles_GUI, false);
		}
		
		[MenuItem ("Version Control/Debug/GUI Tests/Branch List")]
		static void GUITest_BranchList()
		{
			UVCProcessPopup.Init(VersionControl.FindBranches(CommandLine.EmptyHandler), false, true, OnFindBranches_GUI, false);
		}
		
		[MenuItem ("Version Control/Debug/GUI Tests/Bad Command")]
		static void GUITest_BadCommand()
		{
			if (VersionControl.versionControlType == VersionControlType.Git)
			{
				UVCProcessPopup.Init(Git.RunGit("notarealcommand", CommandLine.EmptyHandler), false, true, null, false);
			}
			else
			{
				UVCProcessPopup.Init(Hg.RunHg("notarealcommand", CommandLine.EmptyHandler), false, true, null, false);
			}
			
			
		}
		
		[MenuItem ("Version Control/Debug/Other Tests/Re-open Project")]
		static void OtherTest_ReopenProject()
		{	
			VersionControl.RestartUnity();
		}
		
		static void OnProcessExit(object sender, System.EventArgs e)
		{
			var process = sender as System.Diagnostics.Process;
			
			string output = process.StandardOutput.ReadToEnd();
			
			if (!string.IsNullOrEmpty(output))
			{
				Debug.Log(output);
			}
			
			string error = process.StandardError.ReadToEnd();
			
			if (!string.IsNullOrEmpty(error))
			{
				Debug.LogError(error);
			}
		}
		
		static void OnFindFiles(object sender, System.EventArgs e)
		{
			OnFindFiles_INTERNAL((sender as System.Diagnostics.Process).StandardOutput.ReadToEnd());
		}
		
		static void OnFindFiles_GUI(int exitCode, string stdout, string stderr)
		{
			OnFindFiles_INTERNAL(stdout);
		}
		
		static void OnFindFiles_INTERNAL(string input)
		{
			var files = VersionControl.ParseFiles(input);
			
			Debug.Log("Staged Files:");
			
			foreach(var file in files)
			{
				if (file.fileState1 != FileState.Unmodified && file.fileState1 != FileState.Untracked && file.fileState1 != FileState.Ignored)
				{
					if (string.IsNullOrEmpty(file.name2))
					{
						Debug.Log(file.name1);
					}
					else
					{
						Debug.Log(file.name2);
					}
				}
			}
			
			Debug.Log("Working tree:");
			
			foreach(var file in files)
			{
				if (file.fileState2 != FileState.Unmodified && file.fileState2 != FileState.Untracked && file.fileState2 != FileState.Ignored)
				{
					if (string.IsNullOrEmpty(file.name2))
					{
						Debug.Log(file.name1);
					}
					else
					{
						Debug.Log(file.name2);
					}
				}
			}
			
			Debug.Log("Untracked:");
			
			foreach(var file in files)
			{
				if (file.fileState1 == FileState.Untracked && file.fileState2 == FileState.Untracked)
				{
					Debug.Log(file.name1);
				}
			}
			
			Debug.Log("Ignored:");
			
			foreach(var file in files)
			{
				if (file.fileState1 == FileState.Ignored && file.fileState2 == FileState.Ignored)
				{
					Debug.Log(file.name1);
				}
			}
		}
		
		static void OnFindBranches(object sender, System.EventArgs e)
		{
			OnFindBranches_INTERNAL((sender as System.Diagnostics.Process).StandardOutput.ReadToEnd());
		}
		
		static void OnFindBranches_GUI(int exitCode, string stdout, string stderr)
		{
			OnFindBranches_INTERNAL(stdout);
		}
		
		static void OnFindBranches_INTERNAL(string input)
		{
			var branches = VersionControl.ParseBranches(input);
			
			Debug.Log("Local Branches:");
			
			foreach(var branch in branches)
			{
				if (!branch.isRemote)
				{
					if (branch.isCurrent)
					{
						Debug.Log(branch.name + " (current branch)");
					}
					else
					{
						Debug.Log(branch.name);
					}
				}
			}
			
			Debug.Log("Remote Branches:");
			
			foreach(var branch in branches)
			{
				if (branch.isRemote)
				{
					Debug.Log(branch.name + " (" + branch.remoteName + ")");
				}
			}
		}
	}
}
#endif