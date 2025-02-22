using System;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace CustomDebug
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
        #region Log

        ///  <summary>
        /// 		<para>Logs a message to the Unity Console with a title.</para>
        ///  </summary>
        ///  <param name="title">String to display in uppercase.</param>
        ///  <param name="color">Title color. Use CustomDebug.Colors to choose a color.</param>
        ///  <param name="message">String or object to be converted to string representation for display.</param>
        public static void Log(string title, Colors color, object message)
        {
            var colorOpen = GetColorOpen(color);

            Debug.Log($"<b>{colorOpen}[{title.ToUpper()}]</color></b> {message}");
        }

        ///  <summary>
        /// 		<para>Logs a message to the Unity Console using script name as title + line.</para>
        ///  </summary>
        ///  <param name="color">Title color. Use CustomDebug.Colors to choose a color.</param>
        ///  <param name="message">String or object to be converted to string representation for display.</param>
        public static void Log(Colors color, object message)
        {
            var colorOpen = GetColorOpen(color);

            var (callingScript, lineNumber) = GetTraceScript();

            Debug.Log($"<b>{colorOpen}[{callingScript}:{lineNumber}]</color></b> {message}");
        }


        ///  <summary>
        /// 		<para>Logs a message to the Unity Console with a title.</para>
        ///  </summary>
        ///  <param name="title">String to display in uppercase. </param>
        ///  <param name="color">Title color. Use UnityEngine.Color to choose a RGB color.</param>
        ///  <param name="message">String or object to be converted to string representation for display.</param>
        public static void Log(string title, Color color, object message)
        {
            var colorOpen = $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>";

            Debug.Log($"<b>{colorOpen}[{title.ToUpper()}]</color></b> {message}");
        }

        ///  <summary>
        /// 		<para>Logs a message to the Unity Console using script name as title + line.</para>
        ///  </summary>
        ///  <param name="color">Title color. Use UnityEngine.Color to choose a RGB color.</param>
        ///  <param name="message">String or object to be converted to string representation for display.</param>
        public static void Log(Color color, object message)
        {
            var colorOpen = $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>";

            var (callingScript, lineNumber) = GetTraceScript();

            Debug.Log($"<b>{colorOpen}[{callingScript}:{lineNumber}]</color></b> {message}");
        }

        #endregion

        #region LogWarning

        ///  <summary>
        /// 		<para>A variant of CustomDebug.Log that logs a warning message to the console with a title.</para>
        ///  </summary>
        ///  <param name="title">String to display in uppercase.</param>
        ///  <param name="color">Title color. Use CustomDebug.Colors to choose a color.</param>
        ///  <param name="message">String or object to be converted to string representation for display.</param>
        public static void LogWarning(string title, Colors color, object message)
        {
            var colorOpen = GetColorOpen(color);

            Debug.LogWarning($"<b>{colorOpen}[{title.ToUpper()}]</color></b> {message}");
        }
        
        ///  <summary>
        /// 		<para>A variant of CustomDebug.Log that logs a warning message to the console using script name as title + line.</para>
        ///  </summary>
        ///  <param name="title">String to display in uppercase.</param>
        ///  <param name="color">Title color. Use CustomDebug.Colors to choose a color.</param>
        ///  <param name="message">String or object to be converted to string representation for display.</param>
        public static void LogWarning(Colors color, object message)
        {
            var colorOpen = GetColorOpen(color);
            var (callingScript, lineNumber) = GetTraceScript();
            
            Debug.LogWarning($"<b>{colorOpen}[{callingScript}:{lineNumber}]</color></b> {message}");
        }

        ///  <summary>
        /// 		<para>A variant of CustomDebug.Log that logs a warning message to the console with a title.</para>
        ///  </summary>
        ///  <param name="title">String to display in uppercase.</param>
        ///  <param name="color">Title color. Use UnityEngine.Color to choose a RGB color.</param>
        ///  <param name="message">String or object to be converted to string representation for display.</param>
        public static void LogWarning(string title, Color color, object message)
        {
            var colorOpen = $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>";

            Debug.LogWarning($"<b>{colorOpen}[{title.ToUpper()}]</color></b> {message}");
        }

        //TODO: add trace options
        
        #endregion

        #region LogError

        ///  <summary>
        /// 		<para>A variant of CustomDebug.Log that logs an error message to the console with a title.</para>
        ///  </summary>
        ///  <param name="title">String to display in uppercase.</param>
        ///  <param name="color">Title color. Use CustomDebug.Colors to choose a color.</param>
        ///  <param name="message">String or object to be converted to string representation for display.</param>
        public static void LogError(string title, Colors color, object message)
        {
            var colorOpen = GetColorOpen(color);

            Debug.LogError($"<b>{colorOpen}[{title.ToUpper()}]</color></b> {message}");
        }
        
        ///  <summary>
        /// 		<para>A variant of CustomDebug.Log that logs an error message to the console with a title.</para>
        ///  </summary>
        ///  <param name="title">String to display in uppercase.</param>
        ///  <param name="color">Title color. Use CustomDebug.Colors to choose a color.</param>
        ///  <param name="message">String or object to be converted to string representation for display.</param>
        public static void LogError(Colors color, object message)
        {
            var colorOpen = GetColorOpen(color);
            var (callingScript, lineNumber) = GetTraceScript();

            Debug.LogError($"<b>{colorOpen}[{callingScript}:{lineNumber}]</color></b> {message}");
        }

        ///  <summary>
        /// 		<para>A variant of CustomDebug.Log that logs an error message to the console using script name as title + line.</para>
        ///  </summary>
        ///  <param name="title">String to display in uppercase.</param>
        ///  <param name="color">Title color. Use UnityEngine.Color to choose a RGB color.</param>
        ///  <param name="message">String or object to be converted to string representation for display.</param>
        public static void LogError(string title, Color color, object message)
        {
            var colorOpen = $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>";

            Debug.LogError($"<b>{colorOpen}[{title.ToUpper()}]</color></b> {message}");
        }
        
        //TODO: add trace options

        #endregion

        #region Aux Methods

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

        private static string GetColorOpen(Colors color)
        {
            var colorHex = GetColor(color);

            var colorOpen = $"<color={colorHex}>";
            return colorOpen;
        }

        private static (string script, int number) GetTraceScript()
        {
            var stackTrace = new StackTrace(true);
            var frame = stackTrace.GetFrame(2);

            var callingScript = frame.GetFileName();
            var lineNumber = frame.GetFileLineNumber();

            callingScript = callingScript != null ? System.IO.Path.GetFileNameWithoutExtension(callingScript) : "Unknown";

            return (callingScript, lineNumber);
        }

        #endregion
    }
}