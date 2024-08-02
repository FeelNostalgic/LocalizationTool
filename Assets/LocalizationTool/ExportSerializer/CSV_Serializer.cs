using System.Collections.Generic;
using System.Text;

namespace LocalizationTool.ExportSerializer
{
    public class CSV_Serializer
    {
        private string _separator;

        private readonly StringBuilder _sb;
        
        public CSV_Serializer()
        {
            _sb = new StringBuilder();
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