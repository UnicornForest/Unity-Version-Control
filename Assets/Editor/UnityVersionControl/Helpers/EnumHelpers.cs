// Enum helper methods
// EnumHelpers.cs
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
namespace ThinksquirrelSoftware.UnityVersionControl.Helpers
{

    public static class EnumHelpers
	{

        //checks if the value contains the provided type
        public static bool Has<T>(this System.Enum type, T value) {
            try {
                return (((int)(object)type & (int)(object)value) == (int)(object)value);
            } 
            catch {
                return false;
            }
        }

        //checks if the value is only the provided type
        public static bool Is<T>(this System.Enum type, T value) {
            try {
                return (int)(object)type == (int)(object)value;
            }
            catch {
                return false;
            }    
        }

        //appends a value
        public static T Add<T>(this System.Enum type, T value) {
            try {
                return (T)(object)(((int)(object)type | (int)(object)value));
            }
            catch(System.Exception ex) {
                throw new System.ArgumentException(
                    string.Format(
                        "Could not append value from enumerated type '{0}'.",
                        typeof(T).Name
                        ), ex);
            }    
        }

        //completely removes the value
        public static T Remove<T>(this System.Enum type, T value) {
            try {
                return (T)(object)(((int)(object)type & ~(int)(object)value));
            }
            catch (System.Exception ex) {
                throw new System.ArgumentException(
                    string.Format(
                        "Could not remove value from enumerated type '{0}'.",
                        typeof(T).Name
                        ), ex);
            }  
        }

    }
}