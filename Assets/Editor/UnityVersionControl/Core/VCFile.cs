// Version control file
// VCFile.cs
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
	/// Represents the state of a file.
	/// </summary>
	public enum FileState
	{
		Unmodified,
		Modified,
		Added,
		Deleted,
		Renamed,
		Copied,
		Unmerged,
		Untracked,
		Ignored
	}
	
	/// <summary>
	/// Represents a version control file.
	/// </summary>
	public class VCFile
	{
		#region Member fields
		private FileState mFileState1;
		private FileState mFileState2;
		private string mName1 = string.Empty;
		private string mName2 = string.Empty;
		private string mPath1 = string.Empty;
		private string mPath2 = string.Empty;
		private bool mSelected = false;
		#endregion
		
		#region Constructors
		public VCFile() {}
		public VCFile(VCFile template)
		{
			mFileState1 = template.fileState1;
			mFileState2 = template.fileState2;
			mName1 = template.name1;
			mName2 = template.name2;
			mPath1 = template.path1;
			mPath2 = template.path2;
		}
		#endregion
		#region Public properties
		public FileState fileState1
		{
			get
			{
				return mFileState1;
			}
			set
			{
				mFileState1 = value;
			}
		}
		public FileState fileState2
		{
			get
			{
				return mFileState2;
			}
			set
			{
				mFileState2 = value;
			}
		}
		public string name1
		{
			get
			{
				return mName1;
			}
			set
			{
				mName1 = value;
			}
		}
		public string name2
		{
			get
			{
				return mName2;
			}
			set
			{
				mName2 = value;
			}
		}
		public string path1
		{
			get
			{
				return mPath1;
			}
			set
			{
				mPath1 = value;
			}
		}
		public string path2
		{
			get
			{
				return mPath2;
			}
			set
			{
				mPath2 = value;
			}
		}
		public bool selected
		{
			get
			{
				return mSelected;
			}
			set
			{
				mSelected = value;
			}
		}
		#endregion
		
	}
}