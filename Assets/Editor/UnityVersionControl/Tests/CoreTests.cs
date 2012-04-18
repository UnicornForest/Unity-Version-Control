// Test setup for core commands
// CoreTests.cs
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
#define DEBUG
#if DEBUG
using UnityEngine;
using UnityEditor;
using ThinksquirrelSoftware.UnityVersionControl.Core;

namespace ThinksquirrelSoftware.UnityVersionControl.Tests
{
	public class CoreTests
	{	
		[MenuItem ("Version Control/Debug/Core Tests/Init")]
	    static void Test1()
		{
			Git.RunGit("init", OnProcessExit);
		}
		
		[MenuItem ("Version Control/Debug/Core Tests/Status")]
	    static void Test2()
		{
			Git.RunGit("status", OnProcessExit);
		}
		
		[MenuItem ("Version Control/Debug/Core Tests/Bad Command")]
	    static void Test3()
		{
			Git.RunGit("notarealcommand", OnProcessExit);
		}
		
		[MenuItem ("Version Control/Debug/Core Tests/Repository Location")]
	    static void Test4()
		{
			string message = Git.RepositoryLocation();
			
			EditorUtility.DisplayDialog("Repository Location", message, "Ok", "Cancel");
		}
		
		static void OnProcessExit(object sender, System.EventArgs e)
		{
			var gitProcess = sender as System.Diagnostics.Process;
			
			string output = gitProcess.StandardOutput.ReadToEnd();
			
			if (!string.IsNullOrEmpty(output))
			{
				Debug.Log(output);
			}
			
			string error = gitProcess.StandardError.ReadToEnd();
			
			if (!string.IsNullOrEmpty(error))
			{
				Debug.LogError(error);
			}
		}
	}
}
#endif