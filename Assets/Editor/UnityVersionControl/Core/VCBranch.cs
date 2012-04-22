// Version control branch
// VCBranch.cs
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
namespace ThinksquirrelSoftware.UnityVersionControl.Core
{
	
	/// <summary>
	/// Represents a version control branch.
	/// </summary>
	public class VCBranch
	{
		#region Member fields
		private string mName;
		private string mRemoteName;
		private bool mIsRemote = false;
		private bool mIsCurrent = false;
		#endregion
		
		#region Constructors
		public VCBranch(string name, string remoteName, bool isRemote, bool isCurrent)
		{
			this.mName = name;
			this.mRemoteName = remoteName;
			this.mIsRemote = isRemote;
			this.mIsCurrent = isCurrent;
		}
		#endregion
		
		#region Public properties
		public string name
		{
			get
			{
				return mName;
			}
			set
			{
				mName = value;
			}
		}
		public string remoteName
		{
			get
			{
				return mRemoteName;
			}
			set
			{
				mRemoteName = value;
			}
		}
		public bool isRemote
		{
			get
			{
				return mIsRemote;
			}
			set
			{
				mIsRemote = value;
			}
		}
		public bool isCurrent
		{
			get
			{
				return mIsCurrent;
			}
			set
			{
				mIsCurrent = value;
			}
		}
		#endregion
		
	}
}