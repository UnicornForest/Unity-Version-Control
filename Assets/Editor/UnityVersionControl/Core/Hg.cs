// Mercurial command line wrapper methods
// Hg.cs
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

namespace ThinksquirrelSoftware.UnityVersionControl.Core
{
	/// <summary>
	/// Mercurial command line wrapper methods.
	/// </summary>
	internal static class Hg
	{
		// Default - "hg"
		const string mHgCommand = "hg";
		
		/// <summary>
		/// Runs hg asynchronously with the specified arguments.
		/// </summary>
		/// <remarks>
		/// Returns the process (with output and error streams redirected) to be handled on an exit event.
		/// </remarks>
		internal static Process RunHg(string args, System.EventHandler exitEventHandler)
		{
			return CommandLine.RunCommand(mHgCommand, args, exitEventHandler);
		}
		
		/// TODO: Implement Hg
		internal static bool ProjectHasRepository()
		{
			throw new System.NotImplementedException();
		}
		
		/// TODO: Implement Hg
		internal static string RepositoryLocation()
		{
			throw new System.NotImplementedException();
		}
		
		/// <summary>
		/// An empty process handler. Used when waiting for RunHg to complete, in order to still get standard output and error streams.
		/// </summary>
		internal static void EmptyHandler(object sender, System.EventArgs e) { }
		
		// TODO: Implement Hg
		internal static VCFile[] ParseFiles(string input)
		{
			throw new System.NotImplementedException();
		}
	}
}