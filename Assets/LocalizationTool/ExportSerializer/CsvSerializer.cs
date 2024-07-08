using System.Collections.Generic;
using System.IO;
using System.Text;
using LocalizationTool.Manager;

namespace LocalizationTool.ExportSerializer
{
    public class CsvSerializer
    {
        private string _separator;

        private readonly StringBuilder _sb;
        
        public CsvSerializer()
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