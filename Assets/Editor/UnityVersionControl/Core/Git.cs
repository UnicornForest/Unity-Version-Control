// Git command line wrapper methods
// Git.cs
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
using System.Diagnostics;
using System.Text;

namespace ThinksquirrelSoftware.UnityVersionControl.Core
{
	public static class Git
	{
		/// <summary>
		/// Runs git asynchronously with the specified arguments.
		/// </summary>
		/// <remarks>
		/// Returns the process (with output and error streams redirected) to be handled on an exit event.
		/// </remarks>
		public static Process RunGit(string args, System.EventHandler exitEventHandler)
		{
			var process = new System.Diagnostics.Process();
			
			process.StartInfo.FileName = "git";
			process.StartInfo.Arguments = args;
			
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.CreateNoWindow = true;
			
			if (exitEventHandler != null)
			{
				process.EnableRaisingEvents = true;
				process.StartInfo.RedirectStandardOutput = true;
				process.StartInfo.RedirectStandardError = true;
				process.Exited += exitEventHandler;
			}
			
			process.Start();
			
			return process;
		}
		
		/// <summary>
		/// Checks to see if the current project has a repository.
		/// </summary>
		/// <returns>
		/// True if the project has a git repository, otherwise false.
		/// </returns>
		public static bool ProjectHasRepository()
		{
			var gitProcess = RunGit("rev-parse --is-inside-work-tree", EmptyHandler);
			bool exited = gitProcess.WaitForExit(5000);
			
			if (!exited)
			{
				// TODO: This needs to fail and throw an exception.
				return false;
			}
			
			if (gitProcess.ExitCode == 0)
			{
				string output = gitProcess.StandardOutput.ReadToEnd();
				return output.Contains("true");
			}
			else
			{
				return false;
			}
		}
		
		/// <summary>
		/// Returns the location of the project's repository.
		/// </summary>
		public static string RepositoryLocation()
		{
			if (!ProjectHasRepository())
				return null;
			
			var gitProcess = RunGit("rev-parse --show-toplevel", EmptyHandler);
			bool exited = gitProcess.WaitForExit(5000);
			
			if (!exited)
			{
				// TODO: This needs to fail and throw an exception.
				return null;
			}
			
			if (gitProcess.ExitCode == 0)
			{
				return gitProcess.StandardOutput.ReadToEnd();	
			}
			else
			{
				// TODO: This needs to fail and throw an exception.
				return null;
			}
		}
		
		private static void EmptyHandler(object sender, System.EventArgs e)
		{
			// Empty process handler. Used when waiting for a process to exit, in order to still get standard output and error streams.
		}
	}
}