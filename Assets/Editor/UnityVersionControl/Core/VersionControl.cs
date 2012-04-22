// Version Control abstraction layer
// VersionControl.cs
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
	/// <summary>
	/// Represents a version control type.
	/// </summary>
	public enum VersionControlType
	{
		Git,
		Hg
	}
	
	/// <summary>
	/// Version control abstraction layer.
	/// </summary>
	public static class VersionControl
	{
		private static VersionControlType mVersionControlType = VersionControlType.Git;
		
		public static VersionControlType versionControlType
		{
			get
			{
				return mVersionControlType;
			}
			set
			{
				mVersionControlType = value;
			}
		}
		
		/// <summary>
		/// Checks to see if the current project has a repository.
		/// </summary>
		/// <returns>
		/// True if the project has a repository, otherwise false.
		/// </returns>
		public static bool ProjectHasRepository()
		{
			switch(mVersionControlType)
			{
			case VersionControlType.Git:
				return Git.ProjectHasRepository();
			case VersionControlType.Hg:
				return Hg.ProjectHasRepository();
			}
			
			return false;
		}
		
		/// <summary>
		/// Returns the location of the project's repository.
		/// </summary>
		public static string RepositoryLocation()
		{
			switch(mVersionControlType)
			{
			case VersionControlType.Git:
				return Git.RepositoryLocation();
			case VersionControlType.Hg:
				return Hg.RepositoryLocation();
			}
			
			return null;
		}
		
		/// <summary>
		/// Initializes/Reinitializes a project repository.
		/// </summary>
		/// TODO: Implement Hg
		public static Process Initialize(System.EventHandler exitEventHandler)
		{
			switch(mVersionControlType)
			{
			case VersionControlType.Git:
				return Git.RunGit("init", exitEventHandler);
			case VersionControlType.Hg:
				throw new System.NotImplementedException();
			}
			
			return null;
		}
		
		/// <summary>
		/// Finds all repository files, asynchronously. Use ParseFiles to parse the result.
		/// </summary>
		/// TODO: Implement Hg
		public static Process FindFiles(System.EventHandler exitEventHandler)
		{
			switch(mVersionControlType)
			{
			case VersionControlType.Git:
				return Git.RunGit("status --porcelain --ignored --untracked-files -z", exitEventHandler);
			case VersionControlType.Hg:
				throw new System.NotImplementedException();
			}
			
			return null;
		}
		
		/// <summary>
		/// Parses all repository files.
		/// </summary>
		public static VCFile[] ParseFiles(string input)
		{
			switch(mVersionControlType)
			{
			case VersionControlType.Git:	
				return Git.ParseFiles(input);
			case VersionControlType.Hg:
				return Hg.ParseFiles(input);
			}
			
			return null;
		}
		
		/// <summary>
		/// Gets the difference between a file and the last commit.
		/// </summary>
		/// TODO: Implement Hg
		public static Process GetDiff(System.EventHandler exitEventHandler, params VCFile[] files)
		{
			switch(mVersionControlType)
			{
			case VersionControlType.Git:
				return Git.GetDiff(exitEventHandler, files);
			case VersionControlType.Hg:
				throw new System.NotImplementedException();
			}
			
			return null;
		}
		
		/// <summary>
		/// Add the specified file(s).
		/// </summary>
		/// TODO: Implement Hg
		public static Process Add(System.EventHandler exitEventHandler, params VCFile[] files)
		{
			switch(mVersionControlType)
			{
			case VersionControlType.Git:
				return Git.Add(exitEventHandler, files);
			case VersionControlType.Hg:
				throw new System.NotImplementedException();
			}
			
			return null;
		}
		
		/// <summary>
		/// Remove the specified file(s).
		/// </summary>
		/// TODO: Implement Hg
		public static Process Remove(System.EventHandler exitEventHandler, params VCFile[] files)
		{
			switch(mVersionControlType)
			{
			case VersionControlType.Git:
				return Git.Remove(exitEventHandler, files);
			case VersionControlType.Hg:
				throw new System.NotImplementedException();
			}
			
			return null;
		}
		
		/// <summary>
		/// Remove the specified file(s).
		/// </summary>
		/// TODO: Implement Hg
		public static Process Reset(System.EventHandler exitEventHandler, string branch, params VCFile[] files)
		{
			switch(mVersionControlType)
			{
			case VersionControlType.Git:
				return Git.Reset(exitEventHandler, branch, files);
			case VersionControlType.Hg:
				throw new System.NotImplementedException();
			}
			
			return null;
		}
		
		/// <summary>
		/// Commits a change. Takes a string literal as the message.
		/// </summary>
		public static Process Commit(System.EventHandler exitEventHandler, string messageStringLiteral, bool amend, params VCFile[] files)
		{
			switch(mVersionControlType)
			{
			case VersionControlType.Git:
				return Git.Commit(exitEventHandler, messageStringLiteral, amend, files);
			case VersionControlType.Hg:
				throw new System.NotImplementedException();
			}
			
			return null;
		}
		
	}
}