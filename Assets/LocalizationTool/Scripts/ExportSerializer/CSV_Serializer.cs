using System.Collections.Generic;
using System.Text;

namespace LocalizationTool.Scripts.ExportSerializer
{
    public class CSV_Serializer
    {
        private string _separator;

        private readonly StringBuilder _sb;
        
        public CSV_Serializer()
        {
            _sb = new StringBuilder();
        }

        public void AddTitle(IEnumerable<string> languages)
        {
            var titles = new List<string> { "Key", "Category" };
            titles.AddRange(languages);
            AddLine(titles);
        }

        public void AddTitle(string language)
        {
            var titles = new List<string> { "Key", "Category", $"{language}" };
            AddLine(titles);
        }
        
        public void SetSeparator(string separator)
        {
            _separator = separator;
        }
        
        public void AddLine(IEnumerable<string> items)
        {
            _sb.AppendJoin(_separator, items);
            _sb.AppendLine();
        }
        
        public string File()
        {
            return _sb.ToString();
        }
    }
}