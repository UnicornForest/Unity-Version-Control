// GUI helper methods
// GUIHelpers.cs
// Unity Version Control
//  
// Authors:
//       Josh Montoute <josh@thinksquirrel.com>
//       Dafu <http://forum.unity3d.com/members/950-Dafu>
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

namespace ThinksquirrelSoftware.UnityVersionControl.Helpers
{
	public static class GUIHelpers
	{
		private static GUIStyle vertWidgetStyle;
		
		static GUIHelpers()
		{
			vertWidgetStyle = new GUIStyle(EditorStyles.toolbarButton);
			vertWidgetStyle.fixedHeight = 0;
		}
		
		/// <summary>
		/// Draws a resize widget.
		/// </summary>
		/// <returns>
		/// The new widget state (true if resizing).
		/// </returns>
		/// <param name='state'>
		/// The current widget state.
		/// </param>
		/// <param name='resizeValue'>
		/// The current resize value.
		/// </param>
		/// <param name='minValue'>
		/// The minimum resize value.
		/// </param>
		/// <param name='maxValue'>
		/// The maximum resize value.
		/// </param>
		/// <param name='size'>
		/// The widget size.
		/// </param>
		/// <param name='resizeVertical'>
		/// If set to <c>true</c>, draw a vertical resize widget.
		/// </param>
		/// <param name='resizeArea'>
		/// The area to resize within.
		/// </param>
		public static bool ResizeWidget(
			bool state, 
			ref float resizeValue, float minValue, float maxValue, float size, 
			bool resizeVertical, Rect resizeArea, 
			EditorWindow editorWindow)
		{
			Color oldColor = GUI.backgroundColor;
			
			GUI.backgroundColor = new Color(.5f, .5f, .5f, 1);
			bool s = state;
			
			// Detect a mouse up event
			if (Event.current.type == EventType.MouseUp)
			{
				s = false;
			}
			
			// Draw the widget
			if (resizeVertical)
			{
				if (GUILayout.RepeatButton("", EditorStyles.toolbarButton, GUILayout.Height(size), GUILayout.ExpandWidth(true)))
				{
					s = true;
				}
				
				EditorGUIUtility.AddCursorRect (GUILayoutUtility.GetLastRect(), MouseCursor.ResizeVertical);
			}
			else
			{
				if (GUILayout.RepeatButton("", vertWidgetStyle, GUILayout.Width(size), GUILayout.ExpandHeight(true)))
				{
					s = true;
				}
				
				EditorGUIUtility.AddCursorRect (GUILayoutUtility.GetLastRect(), MouseCursor.ResizeHorizontal);
			}
			
			GUI.backgroundColor = oldColor;
			
			// Process events
			if (s)
			{
				if (Event.current.type == EventType.MouseDrag)
				{
					Vector2 delta = Event.current.delta;
					Vector2 pos = Event.current.mousePosition;
					
					if (!resizeArea.Contains(pos))
					{
						s = false;
						if (editorWindow)
							editorWindow.Repaint();
						return s;
					}
					
					if (resizeVertical)
					{
						resizeValue = Mathf.Clamp(resizeValue += delta.y, minValue, maxValue);
						EditorGUIUtility.AddCursorRect (resizeArea, MouseCursor.ResizeVertical);
						if (editorWindow)
							editorWindow.Repaint();
					}
					else
					{
						resizeValue = Mathf.Clamp(resizeValue += delta.x, minValue, maxValue);
						EditorGUIUtility.AddCursorRect (resizeArea, MouseCursor.ResizeHorizontal);
						if (editorWindow)
							editorWindow.Repaint();
					}
					
				}	
			}
			
			return s;
		}
		
		/// <summary>
		/// Draws a formatted label.
		/// </summary>
		/// <param name='text'>
		/// The text to format.
		/// </param>
		/// <param name='normalFont'>
		/// Normal font.
		/// </param>
		/// <param name='boldFont'>
		/// Bold font.
		/// </param>
		/// <param name='italicFont'>
		/// Italic font.
		/// </param>
		/// <param name='alignment'>
		/// Alignment.
		/// </param>
		/// TODO: Causes a layout error if the text ends in a newLine.
		public static void FormattedLabel(string text, Font normalFont, Font boldFont, Font italicFont, TextAlignment alignment )
		{
		    int     i1 = 0, i2 = 0;
		    bool    done = false;
		    bool    newLine = false;
		    Color   originalColor = GUI.contentColor;
		    Color   textColor = new Color( originalColor.r,
		                                   originalColor.g,
		                                   originalColor.b,
		                                   originalColor.a );
	
		    Font    defaultFont = GUI.skin.font;
		    Font    newFont = null;
		    GUIStyle fontStyle = new GUIStyle(EditorStyles.label);
		
		    // Start with normal font
		
		    if ( normalFont != null ) {
		        fontStyle.font = normalFont;
		    } else {
		        fontStyle.font = defaultFont;
		    }
			
		    // NOTE: Lowering this padding reduces the line spacing
		    // May need to adjust per font
		    fontStyle.padding.bottom = -2;
	
			
		    GUILayout.BeginVertical( GUILayout.ExpandHeight( true ));
		    GUILayout.BeginHorizontal( GUILayout.ExpandWidth( true ));
		
		    // Insert flexible space on the left if Center or Right aligned
		    if ( alignment == TextAlignment.Right || alignment == TextAlignment.Center) {
		         GUILayout.FlexibleSpace();
		    }
		
		    while ( !done ) {
		        int skipChars = 0;
		        int firstEscape, firstNewline;
		
		        firstEscape = text.IndexOf("\a", i2);
		        firstNewline = text.IndexOf("\n", i2);
		
		        if ( firstEscape != -1 && ( firstNewline == -1 || firstEscape < firstNewline ) ) {
		            i1 = firstEscape;
		        } else {
		            i1 = firstNewline;
		        }
		
		        // We're at the end, set the index to the end of the
		        // string and signal an end
		        if ( i1 == -1 ) {
		            i1 = text.Length - 1;
		            done = true;
		        }
		
				GUI.contentColor = textColor;
				
		        if ( newFont != null ) {
		            fontStyle.font = newFont;
		            newFont = null;
		        }
		
		        // If the next character is \a then we have a \a\a sequence
		        // We want to point one of the # so advance the index by
		        // one to include the first #
		        if ( !done ) {
		
		            if ( text.Substring( i1, 1 ) == "\a" ) {
		                if ( (text.Length - i1) >= 2 && text.Substring(i1 + 1, 1) == "\a" ) {
		                    skipChars = 2;
		                }
		
		                // Revert to original color sequence
		                else if (  (text.Length - i1) >= 2 && text.Substring(i1 + 1, 1) == "!" ) {
		                    textColor = new Color( originalColor.r,
		                                           originalColor.g,
		                                           originalColor.b,
		                                           originalColor.a );
		                    i1--;
		                    skipChars = 3;
		                }
		
		                // Set normal font
		                else if (  (text.Length - i1) >= 2 && text.Substring(i1 + 1, 1) == "n" ) {
		                    if ( normalFont != null ) {
		                        newFont = normalFont;
		                    } else {
		                        newFont = defaultFont;
		                    }
		                    i1--;
		                    skipChars = 3;
		
		                }
		
		                // Set bold font
		                else if (  (text.Length - i1) >= 2 && text.Substring(i1 + 1, 1) == "x" ) {
		                    if ( boldFont != null ) {
		                        newFont = boldFont;
		                    } else {
		                        newFont = defaultFont;
		                    }
		                    i1--;
		                    skipChars = 3;
		                }
		
		                // Set italic font
		                else if (  (text.Length - i1) >= 2 && text.Substring(i1 + 1, 1) == "i" ) {
		                    if ( italicFont != null ) {
		                        newFont = italicFont;
		                    } else {
		                        newFont = defaultFont;
		                    }
		                    i1--;
		                    skipChars = 3;
		                }
		
		                //  New color sequence
		                else if ( (text.Length - i1) >= 10 ) { 
		                    string rText = text.Substring( i1 + 1, 2 );
		                    string gText = text.Substring( i1 + 3, 2 );
		                    string bText = text.Substring( i1 + 5, 2 );
		                    string aText = text.Substring( i1 + 7, 2 );
		
		                    float r = StringHelpers.HexStringToInt( rText ) / 255.0f;
		                    float g = StringHelpers.HexStringToInt( gText ) / 255.0f;
		                    float b = StringHelpers.HexStringToInt( bText ) / 255.0f;
		                    float a = StringHelpers.HexStringToInt( aText ) / 255.0f;
							
		                    if ( r < 0 || g < 0 || b < 0 || a < 0 ) {
		                        Debug.Log("Invalid color sequence");
		                        return;
		                    }
		
		                    textColor = new Color( r, g, b, a );
		                    skipChars = 10;
	
		                    // Move back one character so that we don't print the #		
		                    i1--;
		                } else {
		                    Debug.Log("Invalid \\a escape sequence");
		                    return;
		                }
		            } else if ( (text.Length - i1) >= 1 && text.Substring( i1, 1 ) == "\n" ) {
		                newLine = true;
		                i1--;
		                skipChars = 2;
		            } else {
		                Debug.Log("Invalid escape sequence");
		                return;
		            }	
		        }
		
		        string textPiece = text.Substring( i2, i1 - i2 + 1 );           
	
		        GUILayout.Label( textPiece, fontStyle );
		
		        // Unity seems to cut off the trailing spaces in the label, he have
		        // to add them manually here
		        // Figure out how many trailing spaces there are
		        int spaces = textPiece.Length - textPiece.TrimEnd(' ').Length;
		
		        // NOTE: Add the proper amount of gap for trailing spaces.
		        // the length of space is a questimate here,
		        // may need to be adjusted for different fonts
		        GUILayout.Space( spaces * 5.0f );
		
		        if ( newLine ) {
		            // Create a new line by ending the horizontal layout
		            if ( alignment == TextAlignment.Left || alignment == TextAlignment.Center) {
		                GUILayout.FlexibleSpace();
		            }
					
		            GUILayout.EndHorizontal();
	
		            GUILayout.BeginHorizontal( GUILayout.ExpandWidth( true ));          
		            if ( alignment == TextAlignment.Right || alignment == TextAlignment.Center) {
		                GUILayout.FlexibleSpace();
		            }
		            newLine = false;
		        }
		
		        // Store the last index
		        i2 = i1 + skipChars;
		    }
		
		    if ( alignment == TextAlignment.Left || alignment == TextAlignment.Center) {
		        GUILayout.FlexibleSpace();
		    }
		    GUILayout.EndHorizontal();
		    GUILayout.FlexibleSpace();
		    GUILayout.EndVertical();
			
			GUI.contentColor = originalColor;
		
		}		
	}
}