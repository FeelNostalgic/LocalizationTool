
using System;
using UnityEngine;

namespace CustomDebugPlugin
{
	public enum Colors
	{
		White,
		Blue,
		DarkBlue,
		LightBlue,
		Yellow, 
		Green, 
		Red,
		Purple, 
		Magenta,
		Pink
	}
	
	public static class CustomDebug 
	{
		///  <summary>
		/// 		<para>Logs a message to the Unity Console with a title.</para>
		///  </summary>
		///  <param name="title">String to display in uppercase.</param>
		///  <param name="color">Title color. Use CustomDebug.Colors to choose a color.</param>
		///  <param name="message">String or object to be converted to string representation for display.</param>
		public static void Log(string title, Colors color, object message)
		{
			var colorHex = GetColor(color);
			
			var colorOpen = $"<color={colorHex}>";
			
			Debug.Log($"<b>{colorOpen}[{title.ToUpper()}]</color></b> {message}");
		}
		
		///  <summary>
		/// 		<para>Logs a message to the Unity Console with a title.</para>
		///  </summary>
		///  <param name="title">String to display in uppercase. </param>
		///  <param name="color">Title color. Use UnityEngine.Color to choose a RGB color.</param>
		///  <param name="message">String or object to be converted to string representation for display.</param>
		public static void Log(string title, Color color, object message)
		{
			var colorHex = ColorUtility.ToHtmlStringRGB(color);
			
			var colorOpen = $"<color={colorHex}>";
			
			Debug.Log($"<b>{colorOpen}[{title.ToUpper()}]</color></b> {message}");
		}

		///  <summary>
		/// 		<para>A variant of CustomDebug.Log that logs a warning message to the console with a title.</para>
		///  </summary>
		///  <param name="title">String to display in uppercase.</param>
		///  <param name="color">Title color. Use CustomDebug.Colors to choose a color.</param>
		///  <param name="message">String or object to be converted to string representation for display.</param>
		public static void LogWarning(string title, Colors color, object message)
		{
			var colorHex = GetColor(color);
			
			var colorOpen = $"<color={colorHex}>";
			
			Debug.LogWarning($"<b>{colorOpen}[{title.ToUpper()}]</color></b> {message}");
		}
		
		///  <summary>
		/// 		<para>A variant of CustomDebug.Log that logs a warning message to the console with a title.</para>
		///  </summary>
		///  <param name="title">String to display in uppercase.</param>
		///  <param name="color">Title color. Use UnityEngine.Color to choose a RGB color.</param>
		///  <param name="message">String or object to be converted to string representation for display.</param>
		public static void LogWarning(string title, Color color, object message)
		{
			var colorHex = ColorUtility.ToHtmlStringRGB(color);
			
			var colorOpen = $"<color={colorHex}>";
			
			Debug.LogWarning($"<b>{colorOpen}[{title.ToUpper()}]</color></b> {message}");
		}

		///  <summary>
		/// 		<para>A variant of CustomDebug.Log that logs an error message to the console with a title.</para>
		///  </summary>
		///  <param name="title">String to display in uppercase.</param>
		///  <param name="color">Title color. Use CustomDebug.Colors to choose a color.</param>
		///  <param name="message">String or object to be converted to string representation for display.</param>
		public static void LogError(string title, Colors color, object message)
		{
			var colorHex = GetColor(color);
			
			var colorOpen = $"<color={colorHex}>";
			
			Debug.LogError($"<b>{colorOpen}[{title.ToUpper()}]</color></b> {message}");
		}
		
		///  <summary>
		/// 		<para>A variant of CustomDebug.Log that logs an error message to the console with a title.</para>
		///  </summary>
		///  <param name="title">String to display in uppercase.</param>
		///  <param name="color">Title color. Use UnityEngine.Color to choose a RGB color.</param>
		///  <param name="message">String or object to be converted to string representation for display.</param>
		public static void LogError(string title, Color color, object message)
		{
			var colorHex = ColorUtility.ToHtmlStringRGB(color);
			
			var colorOpen = $"<color={colorHex}>";
			
			Debug.LogError($"<b>{colorOpen}[{title.ToUpper()}]</color></b> {message}");
		}

		private static string GetColor(Colors color)
		{
			return color switch
			{
				Colors.White => "white",
				Colors.Blue => "#33afff",
				Colors.DarkBlue => "#1d699a",
				Colors.LightBlue => "#83ceff",
				Colors.Yellow => "#f7dc6f",
				Colors.Green => "#58d68d",
				Colors.Red => "#e74c3c",
				Colors.Purple => "#8e44ad",
				Colors.Magenta => "#ff00ff",
				Colors.Pink => "#ff83f6",
				_ => throw new ArgumentOutOfRangeException(nameof(color), color, null)
			};
		}
	}
}