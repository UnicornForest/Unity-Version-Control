// Process Popup Editor Window
// UVCProcessPopup.cs
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
using UnityEditor;
using UnityEngine;
using ThinksquirrelSoftware.UnityVersionControl.Core;
using ThinksquirrelSoftware.UnityVersionControl.Helpers;
using System.Text;

/// <summary>
/// The process popup editor window.
/// </summary>
/// TODO: Handle user input (passwords)
/// TODO: Multicolored GUI label for output and error streams
/// TODO: Detect only true errors
public class UVCProcessPopup : EditorWindow
{
	private Vector2 scrollPosition;
	private System.Diagnostics.Process process;
	private bool exitOnCompletion = false;
	private bool logErrors = true;
	private System.Action<int, string, string> exitCallback;
	private string command;
	private string cancelString = "Cancel";
	private bool exited = false;
	private StringBuilder output = new StringBuilder();
	private StringBuilder error = new StringBuilder();
	private StringBuilder outerr = new StringBuilder();
	private static GUIStyle labelStyle;
	
	/// <summary>
	/// Initialize the process popup.
	/// </summary>
	/// <param name='process'>
	/// The process to control.
	/// </param>
	/// <param name='exitOnCompletion'>
	/// Should this window close on completion?
	/// </param>
	/// <param name='logErrors'>
	/// Should errors be logged to the Unity console?
	/// </param>
	/// <param name='exitCallback'>
	/// An exit callback. This callback must accept an integer parameter for the exit code, and two string parameters for the output and error streams.
	/// If the process is null or not completed, the arguments (-9999, null, null) are passed.
	/// </param>
	///<param name='unityWorkaround'>
	/// A workaround for a Unity bug. Toggle this if Unity throws OnGUI errors.
	/// </param>
	public static void Init(System.Diagnostics.Process process, bool exitOnCompletion, bool logErrors, System.Action<int, string, string> exitCallback, bool unityWorkaround)
	{
		labelStyle = new GUIStyle(EditorStyles.label);
		labelStyle.wordWrap = true;
		
		var window = EditorWindow.CreateInstance<UVCProcessPopup>();
		window.title = "Running Process";
		window.process = process;
		if (process != null)
		{
			window.command = process.StartInfo.FileName + " " + process.StartInfo.Arguments;
		}
#if !DEBUG		
		window.exitOnCompletion = exitOnCompletion;
#endif
		window.exitCallback = exitCallback;
		
		window.ShowPopup();
		
		if (unityWorkaround)
		{
			// Workaround for Unity bug
			GUIUtility.ExitGUI();
		}
	}
	
	void OnEnable()
	{
		this.minSize = new Vector2(600, 200);
		position = new Rect(position.x, position.y, this.minSize.x, this.minSize.y);
	}
	
	void Update()
	{
		if (process != null)
		{
			string o = process.StandardOutput.ReadToEnd();
			string e = process.StandardError.ReadToEnd();
			
			output.Append(o);
			error.Append(e);
			
			if (o.Length > 0)
				outerr.Append("\aFFFFFFFF").Append(o);
			
			if (e.Length > 0)
				outerr.Append("\aFF0000FF").Append(e);
			
			if (!exited)
			{
				if (process.HasExited)
				{
					exited = true;
					cancelString = "Done";
					
					if (logErrors)
					{		
						if (process.ExitCode != 0)
						{
							exitOnCompletion = false;
						}
					}
					
					if (exitOnCompletion)
						this.Close();
					else
						Repaint();
				}
			}
		}
	}
	
	void OnGUI()
	{
		if (process != null)
		{
			scrollPosition = GUILayout.BeginScrollView(scrollPosition);
			
			GUILayout.BeginVertical();
			
			EditorGUILayout.SelectableLabel(command, labelStyle, GUILayout.MinHeight(50), GUILayout.ExpandHeight(true));
			
			GUIHelpers.FormattedLabel(outerr.ToString(), labelStyle.font, labelStyle.font, labelStyle.font, TextAlignment.Left);
			
			GUILayout.FlexibleSpace();
			
			GUILayout.EndVertical();
			
			GUILayout.EndScrollView();
			
			if (exited)
			{
				GUI.color = process.ExitCode != 0 ? Color.red : Color.green;
			}
			
			if (GUILayout.Button(cancelString))
				this.Close();
			
			GUI.color = Color.white;
		}
		else
		{
			this.Close();
		}
		
	}
	
	void OnDestroy()
	{
		if (exitCallback != null)
		{
			if (process == null)
			{
				exitCallback(-9999, null, null);
			}
			else if (!process.HasExited)
			{
				process.Kill();
				exitCallback(-9999, null, null);
			}
			else
			{
				exitCallback(process.ExitCode, output.ToString(), error.ToString());
			}
		}
	}
}