using FuzzySharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApprovalsKit.Extensibility.Plugins.Helpers
{
    public class FuzzyDataTableMapper
    {
        public string Match(string name, DataTable table)
        {
            List<string> columns = new List<string>();
            foreach ( DataColumn col in table.Columns )
            {
                // Exact match
                if (col.ColumnName.ToLower() == name.ToLower() )
                {
                    return col.ColumnName;
                }
                columns.Add( col.ColumnName );
            }

            var match = Process.ExtractTop(name, columns.ToArray()).Where(s => s.Score > 50);

            if ( match.Count() > 0 )
            {
                return match.First().Value;
            }

            return String.Empty;
        }
    }
}
