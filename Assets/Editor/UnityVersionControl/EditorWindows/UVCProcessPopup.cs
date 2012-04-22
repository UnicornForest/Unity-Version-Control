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
using System.Text;

/// <summary>
/// The process popup editor window.
/// </summary>
public class UVCProcessPopup : EditorWindow
{
	private Vector2 scrollPosition;
	private System.Diagnostics.Process process;
	private bool exitOnCompletion = false;
	private bool logErrors = true;
	private System.Action<int> exitCallback;
	private string command;
	private string cancelString = "Cancel";
	private bool exited = false;
	private StringBuilder output = new StringBuilder();
	private StringBuilder error = new StringBuilder();
	
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
	/// An exit callback. This callback must accept an integer parameter for the exit code. If the process is null or not completed, the code -9999 is passed to the callback.
	/// </param>
	public static void Init(System.Diagnostics.Process process, bool exitOnCompletion, bool logErrors, System.Action<int> exitCallback)
	{
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
		
		// Workaround for Unity bug
		GUIUtility.ExitGUI();
	}
	
	void OnEnable()
	{
		this.minSize = new Vector2(350, 200);
	}
	
	void Update()
	{
		if (process != null)
		{
			output.Append(process.StandardOutput.ReadToEnd());
			
			if (!exited)
			{
				if (process.HasExited)
				{
					exited = true;
					cancelString = "Done";
					
					if (logErrors)
					{
						error.Append(process.StandardError.ReadToEnd());
						
						if (error.Length > 0)
							Debug.LogError(error.ToString() + " (" + command + ")");
					}
					
					if (exitOnCompletion)
						this.Close();
				}
			}
		}
	}
	
	void OnGUI()
	{
		if (process != null)
		{
			GUILayout.Label(command);
			
			scrollPosition = GUILayout.BeginScrollView(scrollPosition);
			
			GUI.enabled = false;
			GUILayout.TextArea(output.ToString());
			GUI.enabled = true;
			
			GUILayout.EndScrollView();
			
			if (GUILayout.Button(cancelString))
				this.Close();
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
				exitCallback(-9999);
			}
			else if (!process.HasExited)
			{
				process.Kill();
				exitCallback(-9999);
			}
			else
			{
				exitCallback(process.ExitCode);
			}
		}
	}
}