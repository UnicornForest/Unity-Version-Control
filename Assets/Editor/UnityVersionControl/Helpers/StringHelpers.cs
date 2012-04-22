// String helper methods
// StringHelpers.cs
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
using UnityEditor;
using UnityEngine;
using System.CodeDom;
using System.Text;
using System.IO;
using Microsoft.CSharp;

namespace ThinksquirrelSoftware.UnityVersionControl.Helpers
{
	public static class StringHelpers
	{
		public static string ToLiteral(this string input)
	    {
	        var writer = new StringWriter();
	        CSharpCodeProvider provider = new CSharpCodeProvider();
	        provider.GenerateCodeFromExpression(new CodePrimitiveExpression(input), writer, null);
	        return writer.GetStringBuilder().ToString();
	    }
	}
}