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
		/// <summary>
		/// Converts a string to a C sharp string literal.
		/// </summary>
		public static string ToLiteral(this string input)
	    {
	        var writer = new StringWriter();
	        CSharpCodeProvider provider = new CSharpCodeProvider();
	        provider.GenerateCodeFromExpression(new CodePrimitiveExpression(input), writer, null);
	        return writer.GetStringBuilder().ToString();
	    }
		
		/// <summary>
		/// Converts a byte array to a hex string.
		/// </summary>
		public static string UnicodeToHexString(string input)
		{
			byte[] barray = Encoding.Unicode.GetBytes(input);
		    char[] c = new char[barray.Length * 2];
		    byte b;
		    for (int i = 0; i < barray.Length; ++i)
		    {
		        b = ((byte)(barray[i] >> 4));
		        c[i * 2] = (char)(b > 9 ? b + 0x37 : b + 0x30);
		        b = ((byte)(barray[i] & 0xF));
		        c[i * 2 + 1] = (char)(b > 9 ? b + 0x37 : b + 0x30);
		    }
		
		    return new string(c);
		}
		
		/// <summary>
		/// Converts a hex string to a Unicode string.
		/// </summary>
		public static string HexStringToUnicode(string hexString)
		{
			int NumberChars = hexString.Length;
			byte[] bytes = new byte[NumberChars / 2];
			
			for (int i = 0; i < NumberChars; i += 2)
			bytes[i / 2] = System.Convert.ToByte(hexString.Substring(i, 2), 16);
			return Encoding.Unicode.GetString(bytes);
		}
		
		/// <summary>
		/// Converts a hex string to an integer.
		/// </summary>
		public static int HexStringToInt(string hexString)
		{
			return int.Parse(hexString, System.Globalization.NumberStyles.HexNumber);
		}
	}
}