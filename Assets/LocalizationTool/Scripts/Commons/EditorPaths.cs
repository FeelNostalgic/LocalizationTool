
namespace LocalizationTool.Scripts.Commons
{ 
	public abstract class EditorPaths
	{
		public const string BASE_PATH = "Assets/LocalizationTool/Scripts/Data/DoNotTouch/";
		
		public const string JSON_DICTIONARY_PATH = BASE_PATH + "LocalizationDataLanguage.json";
		public const string BINARY_LANGUAGES_PATH = BASE_PATH + "LocalizationLanguages.bin";
		public const string BINARY_CATEGORIES_PATH = BASE_PATH + "LocalizationCategories.bin";
		public const string BINARY_CONFIGURATION_PATH = BASE_PATH + "LocalizationConfiguration.bin";
		
		public const string README_PATH = BASE_PATH + "Info/Readme.txt";
		public const string LICENSE_PATH = BASE_PATH + "Info/License.txt";
	}
}