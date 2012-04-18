// Git command line wrapper methods
// Git.cs
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