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
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ThinksquirrelSoftware.UnityVersionControl.Core
{
	public static class Git
	{
		// Default - "git"
		const string mGitCommand = "git";
		// Configuration argument to add to every command
		const string mGitConfig = "-c core.quotepath=false ";
		
		/// <summary>
		/// Runs git asynchronously with the specified arguments.
		/// </summary>
		/// <remarks>
		/// Returns the process (with output and error streams redirected) to be handled on an exit event.
		/// </remarks>
		internal static Process RunGit(string args, System.EventHandler exitEventHandler)
		{
			return CommandLine.RunCommand(mGitCommand, mGitConfig + args, exitEventHandler);
		}
		
		/// <summary>
		/// Checks to see if the current project has a repository.
		/// </summary>
		/// <returns>
		/// True if the project has a git repository, otherwise false.
		/// </returns>
		internal static bool ProjectHasRepository()
		{
			var gitProcess = RunGit("rev-parse --is-inside-work-tree", CommandLine.EmptyHandler);
			bool exited = gitProcess.WaitForExit(5000);
			
			if (!exited)
			{
				// TODO: This needs to fail and throw an exception. (Git.ProjectHasRepository)
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
		internal static string RepositoryLocation()
		{
			if (!ProjectHasRepository())
				return null;
			
			var gitProcess = RunGit("rev-parse --show-toplevel", CommandLine.EmptyHandler);
			bool exited = gitProcess.WaitForExit(5000);
			
			if (!exited)
			{
				// TODO: This needs to fail and throw an exception. (Git.RepositoryLocation)
				return null;
			}
			
			if (gitProcess.ExitCode == 0)
			{
				return gitProcess.StandardOutput.ReadToEnd();	
			}
			else
			{
				// TODO: This needs to fail and throw an exception. (Git.RepositoryLocation)
				return null;
			}
		}
	
		internal static Process FindFiles(System.EventHandler exitEventHandler)
		{
			return Git.RunGit("status --porcelain --ignored --untracked-files -z", exitEventHandler);
		}
		
		internal static Process FindBranches(System.EventHandler exitEventHandler)
		{
			return Git.RunGit("for-each-ref --format=\"%(refname)\" refs/heads refs/remotes", exitEventHandler);
		}
		
		internal static VCFile[] ParseFiles(string input)
		{
			// Split the input.
			string[] inputSplit = input.Split('\0');
			
			List<VCFile> files = new List<VCFile>();
			
			for(int i = 0; i < inputSplit.Length; i++)
			{
				if (string.IsNullOrEmpty(inputSplit[i]))
					continue;
					
				// Create the file
				var file = new VCFile();
				
				// Add it to the list
				files.Add(file);
				
				bool increase = false;
				
				#region parse - staged
				switch(inputSplit[i][0])
				{
				case ' ':
					file.fileState1 = FileState.Unmodified;
					file.path1 = inputSplit[i].Substring(3);
					break;
				case 'M':
					file.fileState1 = FileState.Modified;
					file.path1 = inputSplit[i].Substring(3);
					break;
				case 'A':
					file.fileState1 = FileState.Added;
					file.path1 = inputSplit[i].Substring(3);
					break;
				case 'D':
					file.fileState1 = FileState.Deleted;
					file.path1 = inputSplit[i].Substring(3);
					break;
				case 'R':
					file.fileState1 = FileState.Renamed;
					file.path1 = inputSplit[i+1];
					file.path2 = inputSplit[i].Substring(3);
					increase = true;
					break;
				case 'C':
					file.fileState1 = FileState.Copied;
					file.path1 = inputSplit[i+1];
					file.path2 = inputSplit[i].Substring(3);
					i++;
					break;
				case 'U':
					file.fileState1 = FileState.Unmerged;
					file.path1 = inputSplit[i].Substring(3);
					break;
				case '?':
					file.fileState1 = FileState.Untracked;
					file.path1 = inputSplit[i].Substring(3);
					break;
				case '!':
					file.fileState1 = FileState.Ignored;
					file.path1 = inputSplit[i].Substring(3);
					break;
				}
				#endregion
				#region parse - index
				switch(inputSplit[i][1])
				{
				case ' ':
					file.fileState2 = FileState.Unmodified;
					break;
				case 'M':
					file.fileState2 = FileState.Modified;
					break;
				case 'A':
					file.fileState2 = FileState.Added;
					break;
				case 'D':
					file.fileState2 = FileState.Deleted;
					break;
				case 'R':
					file.fileState2 = FileState.Renamed;
					break;
				case 'C':
					file.fileState2 = FileState.Copied;
					break;
				case 'U':
					file.fileState2 = FileState.Unmerged;
					break;
				case '?':
					file.fileState2 = FileState.Untracked;
					break;
				case '!':
					file.fileState2 = FileState.Ignored;
					break;
				}
				#endregion
				
				#region parse - file name
				string sep = System.IO.Path.DirectorySeparatorChar.ToString();
				if (file.path1.Contains(sep))
				{
					if (file.path1.LastIndexOf(sep) != file.path1.Length - 1)
					{
						file.name1 = file.path1.Substring(file.path1.LastIndexOf(sep) + 1);
					}
					else
					{
						file.name1 = file.path1;
					}
				}
				else
				{
					file.name1 = file.path1;
				}
				if (file.path2.Contains(sep))
				{
					if (file.path2.LastIndexOf(sep) != file.path2.Length - 1)
					{
						file.name2 = file.path2.Substring(file.path2.LastIndexOf(sep) + 1);
					}
					else
					{
						file.name2 = file.path2;
					}
				}
				else
				{
					file.name2 = file.path2;
				}
				#endregion
				
				// Increase by 1 if we renamed or copied
				if (increase)
					i++;
				
			}
			
			return files.ToArray();
		}
		
		// TODO: Error handling (ParseBranches)
		internal static VCBranch[] ParseBranches(string input)
		{
			
			var branchList = new List<VCBranch>();
			
			// Split input
			var inputSplit = input.Split('\n');
			
			foreach(var str in inputSplit)
			{	
				int i = str.LastIndexOf("/") + 1;
				
				// Local branch
				if (str.StartsWith("refs/heads"))
				{
					branchList.Add(new VCBranch(str.Substring(i, str.Length - i), null, false, false));
				}
				// Remote branch
				else if (str.StartsWith("refs/remotes"))
				{
					branchList.Add(new VCBranch(str.Substring(i, str.Length - i), str.Substring(13, str.LastIndexOf("/") - 13), true, false));
				}
			}
			
			var proc = Git.RunGit("symbolic-ref HEAD", CommandLine.EmptyHandler);
			proc.WaitForExit(6000);
			
			if (proc.HasExited)
			{
				if (proc.ExitCode == 0)
				{
					string b = proc.StandardOutput.ReadToEnd();
					int j = b.LastIndexOf("/") + 1;
						
					b = b.Substring(j, b.Length - j - 1);
					
					foreach(var branch in branchList)
					{
						if (!branch.isRemote && b.Equals(branch.name))
						{
							branch.isCurrent = true;
						}
					}	
				}
			}
			else
			{
				// This is an error situation
				return null;
			}
			
			return branchList.ToArray();
		}
		
		internal static Process GetDiff(System.EventHandler exitEventHandler, params VCFile[] files)
		{
			StringBuilder f = new StringBuilder();
				
			foreach(var file in files)
			{
				if (string.IsNullOrEmpty(file.path2))
					f.Append('"').Append(file.path1).Append('"').Append(' ');
				else
					f.Append(file.path2).Append(' ');
			}
			
			return Git.RunGit("diff -no-ext-diff " + f.ToString(), exitEventHandler);
		}
		
		/// TODO: (Git) Files that are detected as deleted aren't getting added to the index.
		internal static Process Add(System.EventHandler exitEventHandler, params VCFile[] files)
		{
			var f = new StringBuilder().Append("add -A ");
			
			foreach(var file in files)
			{
				if (string.IsNullOrEmpty(file.path2))
				{
					f.Append('"').Append(file.path1).Append('"').Append(' ');
				}
				else
				{
					f.Append(file.path2).Append(' ');
				}
			}
			
			return RunGit(f.ToString(), exitEventHandler);
		}
		
		internal static Process Remove(System.EventHandler exitEventHandler, params VCFile[] files)
		{
			var f = new StringBuilder().Append("rm -f ");
			
			foreach(var file in files)
			{
				if (string.IsNullOrEmpty(file.path2))
				{
					f.Append('"').Append(file.path1).Append('"').Append(' ');
				}
				else
				{
					f.Append(file.path2).Append(' ');
				}
			}
			
			return RunGit(f.ToString(), exitEventHandler);
		}
		
		internal static Process Commit(System.EventHandler exitEventHandler, string messageStringLiteral, bool amend, params VCFile[] files)
		{
			var f = new StringBuilder().Append("commit ");
			var f2 = new StringBuilder().Append("add ");
			
			if (amend)
			{
				f.Append("--amend ");
			}
			
			f.Append("-m ").Append(messageStringLiteral);
			
			bool add = false;
			
			foreach(var file in files)
			{
				if (string.IsNullOrEmpty(file.path2))
				{
					f.Append('"').Append(file.path1).Append('"').Append(' ');
					if (file.fileState1 == FileState.Untracked)
					{
						add = true;
						f2.Append('"').Append(file.path1).Append('"').Append(' ');
					}
					
				}
				else
				{
					f.Append('"').Append(file.path2).Append('"').Append(' ');
					if (file.fileState1 == FileState.Untracked)
					{
						add = true;
						f2.Append('"').Append(file.path1).Append('"').Append(' ');
					}
				}
			}
			
			if (add)
			{
				// Run git add for untracked files
				Git.RunGit(f2.ToString(), null).WaitForExit(6000);
			}
			
			return RunGit(f.ToString(), exitEventHandler);
		}
		
		internal static Process Reset(System.EventHandler exitEventHandler, string branch, params VCFile[] files)
		{
			var f = new StringBuilder().Append("reset ").Append(branch).Append(' ');
			
			foreach(var file in files)
			{
				if (string.IsNullOrEmpty(file.path2))
				{
					f.Append('"').Append(file.path1).Append('"').Append(' ');
				}
				else
				{
					f.Append(file.path2).Append(' ');
				}
			}
			
			return RunGit(f.ToString(), exitEventHandler);
		}
		
		internal static Process Checkout(System.EventHandler exitEventHandler, string item, bool force)
		{
			if (force)
			{
				return RunGit("checkout -f " + item, exitEventHandler);
			}
			else
			{
				return RunGit("checkout " + item, exitEventHandler);
			}
		}
	}
}