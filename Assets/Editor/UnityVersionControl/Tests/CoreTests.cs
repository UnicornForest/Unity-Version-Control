// Test setup for core commands
// CoreTests.cs
// Unity Version Control
//  
// Authors:
//       Josh Montoute <josh@thinksquirrel.com>
// 
// Copyright (c) 2011-2012, Thinksquirrel Software, LLC
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without modification, 
// are permitted provided that the following conditions are met:
//
// Redistributions of source code must retain the above copyright notice,
// this list of conditions and the following disclaimer.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
// ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES 
// OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT 
// SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, 
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT 
// OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) 
// HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, 
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, 
// EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#define DEBUG
#if DEBUG
using UnityEngine;
using UnityEditor;
using ThinksquirrelSoftware.UnityVersionControl.Core;

namespace ThinksquirrelSoftware.UnityVersionControl.Tests
{
	public class CoreTests
	{
		private static System.Diagnostics.Process gitProcess;
		
		[MenuItem ("Version Control/Debug/Core Tests/Test 1")]
	    static void Test1()
		{
			gitProcess = Git.RunGit("init", OnProcessExit);
		}
		
		[MenuItem ("Version Control/Debug/Core Tests/Test 2")]
	    static void Test2()
		{
			gitProcess = Git.RunGit("status", OnProcessExit);
		}
		
		[MenuItem ("Version Control/Debug/Core Tests/Test 3")]
	    static void Test3()
		{
			gitProcess = Git.RunGit("notarealcommand", OnProcessExit);
		}
		
		static void OnProcessExit(object sender, System.EventArgs e)
		{
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