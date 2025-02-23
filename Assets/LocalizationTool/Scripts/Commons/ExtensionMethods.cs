
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LocalizationTool.Scripts.Commons
{ 
	public static class ExtensionMethods 
	{
		#region STRING

		public static bool IsNull(this string obj) {
			return obj is null;
		}

		public static bool IsNotNull(this string obj) {
			return !obj.IsNull();
		}

		public static bool IsEmpty(this string obj)
		{
			return obj is { Length: <= 0 } or "";
		}

		public static bool IsNotEmpty(this string obj)
		{
			return !obj.IsEmpty();
		}

		public static bool NotEquals(this string str, string other)
		{
			return !str.Equals(other);
		}

		#endregion

		#region OBJECT

		public static bool IsNull(this object obj) {
			return obj is null;
		}
		
		public static bool IsNotNull(this object obj) {
			return !obj.IsNull();
		}

		#endregion

		#region UNITY OBJECT

		public static bool IsNull(this UnityEngine.Object obj)
		{
			return obj is null;
		}

		public static bool IsNotNull(this UnityEngine.Object obj)
		{
			return !obj.IsNull();
		}

		#endregion
		
		#region OBJECT[]

		public static bool IsEmpty(this object[] obj) {
			return obj is not { Length: > 0 };
		}

		public static bool IsNotEmpty(this object[] obj)
		{
			return !obj.IsEmpty();
		}
		
		public static bool IsNull(this object[] obj) {
			return obj is not { Length: > 0 };
		}

		public static bool IsNotNull(this object[] obj) {
			return !obj.IsEmpty();
		}
		
		#endregion

		#region STRING[]

		public static bool IsEmpty(this string[] obj) {
			return obj is not { Length: > 0 };
		}
		
		public static bool IsNotEmpty(this string[] obj) {
			return !obj.IsEmpty();
		}
		
		public static bool IsNull(this string[] obj) {
			return obj is null;
		}
		
		public static bool IsNotNull(this string[] obj) {
			return !obj.IsNull();
		}

		#endregion

		#region ICOLLECTION
		
		public static bool IsEmpty(this ICollection collection)
		{
			return collection.Count == 0;
		}

		public static bool IsNull(this ICollection collection)
		{
			return collection is null;
		}
        
		public static bool IsNotEmpty(this ICollection collection)
		{
			return !collection.IsEmpty();
		}

		public static bool IsNotNull(this ICollection collection)
		{
			return !collection.IsNull();
		}
		
		// public static bool IsEmpty<T>(this ICollection<T> collection)
		// {
		// 	return collection.Count == 0;
		// }
		//
		// public static bool IsNull<T>(this ICollection<T> collection)
		// {
		// 	return collection is null;
		// }
  //       
		// public static bool IsNotEmpty<T>(this ICollection<T> collection)
		// {
		// 	return !collection.IsEmpty();
		// }
		//
		// public static bool IsNotNull<T>(this ICollection<T> collection)
		// {
		// 	return !collection.IsNull();
		// }

		#endregion
		
		#region bool

		public static bool Not(this bool b)
		{
			return !b;
		}

		#endregion
		
		#region Dictionary

		public static void Print<T1,T2>(this Dictionary<T1,T2> dictionary)
		{
			Debug.Log($"---Dictionary<{dictionary.Types()}>---");
			foreach (var (key,value) in dictionary)
			{
				Debug.Log($"{key} : {value}");
			}
		}

		public static string Types<T1, T2>(this Dictionary<T1, T2> dictionary)
		{
			return $"{typeof(T1)},{typeof(T2)}";
		}

		#endregion
	}
}