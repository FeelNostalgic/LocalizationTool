
using System.Collections;

namespace LocalizationTool.Commons
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

		#endregion

		#region OBJECT

		public static bool IsNull(this object obj) {
			return obj == null;
		}
		
		public static bool IsNotNull(this object obj) {
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
			return obj == null;
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
			return obj == null;
		}
		
		public static bool IsNotNull(this string[] obj) {
			return !obj.IsNull();
		}

		#endregion

		#region ICOLLECTION

		public static bool IsNull(this ICollection obj) {
			return obj == null;
		}

		public static bool IsNotNull(this ICollection obj) {
			return !obj.IsNull();
		}

		public static bool IsEmpty(this ICollection obj)
		{
			return obj.Count <= 0;
		}

		public static bool IsNotEmpty(this ICollection obj)
		{
			return obj.IsEmpty();
		}

		#endregion
	}
}