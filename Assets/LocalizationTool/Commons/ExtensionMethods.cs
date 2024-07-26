
using System.Collections;

namespace LocalizationTool.Commons
{ 
	public static class ExtensionMethods 
	{
		public static bool IsNull(this string obj) {
			return obj is null or "";
		}
		
		public static bool IsNull(this object obj) {
			return obj == null;
		}
		
		public static bool IsNull(this object[] obj) {
			return obj is not { Length: > 0 };
		}
		
		public static bool IsNull(this string[] obj) {
			return obj is not { Length: > 0 };
		}
		
		public static bool IsNull(this ICollection obj) {
			return obj == null;
		}

		public static bool IsEmpty(this ICollection obj)
		{
			return obj.Count <= 0;
		}
		
		public static bool IsNotNull(this string obj) {
			return !obj.IsNull();
		}
		
		public static bool IsNotNull(this object obj) {
			return !obj.IsNull();
		}
		
		public static bool IsNotNull(this object[] obj) {
			return !obj.IsNull();
		}
		
		public static bool IsNotNull(this ICollection obj) {
			return !obj.IsNull();
		}

		public static bool IsNotEmpty(this ICollection obj)
		{
			return obj.IsEmpty();
		}
	}
}