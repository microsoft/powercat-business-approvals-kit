using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ApprovalsKit.Extensibility.Plugins.Helpers
{
    public static class Extensions
    {
        public static DataTable JsonElementToDataTable(this JsonElement dataRoot)
        {
            var dataTable = new DataTable();
            Dictionary<string, JsonValueKind> customerMapper = new Dictionary<string, JsonValueKind>();
            bool firstPass = true;
            foreach (JsonElement element in dataRoot.EnumerateArray())
            {
                DataRow row = dataTable.NewRow();
                dataTable.Rows.Add(row);
                foreach (JsonProperty col in element.EnumerateObject())
                {
                    var colName = col.Name;
                    //TODO handle objects and array
                    if (firstPass)
                    {
                        JsonElement colValue = col.Value;
                        switch ( colValue.ValueKind )
                        {
                            case JsonValueKind.Object:
                                customerMapper.Add(colName, JsonValueKind.Object);
                                AddJsonObjectTableColumns(col.ToString(), colName, dataTable);
                                break;
                            case JsonValueKind.Array:
                                customerMapper.Add(colName, JsonValueKind.Array);
                                break;
                            default:
                                dataTable.Columns.Add(new DataColumn(col.Name, colValue.ValueKind.ValueKindToType(colValue.ToString())));
                                break;
                        }
                    }

                    JsonValueKind colValueKind = JsonValueKind.Undefined;
                    if ( customerMapper.TryGetValue(colName, out colValueKind) )
                    {
                        switch (colValueKind)
                        {
                            case JsonValueKind.Object:
                                AddJsonObjectTableValues(col.ToString(), colName, dataTable);
                                break;
                            case JsonValueKind.Array:
                                
                                break;
                            default:

                                break;
                        }
                    } 
                    else
                    {
                        row[colName] = col.Value.JsonElementToTypedValue();
                    } 
                }
                firstPass = false;
            }

            return dataTable;
        }

        private static void AddJsonObjectTableColumns(string text, string parent, DataTable dataTable)
        {
            var options = new JsonDocumentOptions
            {
                AllowTrailingCommas = true,
                CommentHandling = JsonCommentHandling.Skip
            };
            var start = text.IndexOf(":") + 1;
            var json = text.Substring(start, text.Length - start);
            using (JsonDocument document = JsonDocument.Parse(json, options))
            {
                foreach (JsonProperty property in document.RootElement.EnumerateObject())
                {
                    string key = property.Name;
                    JsonElement valueElement = property.Value;
                    dataTable.Columns.Add(new DataColumn(parent + "_" + key, valueElement.ValueKind.ValueKindToType(valueElement.ToString())));
                }
            }
        }

        private static void AddJsonObjectTableValues(string text, string parent, DataTable dataTable)
        {
            var options = new JsonDocumentOptions
            {
                AllowTrailingCommas = true,
                CommentHandling = JsonCommentHandling.Skip
            };
            var start = text.IndexOf(":") + 1;
            var json = text.Substring(start, text.Length - start);
            using (JsonDocument document = JsonDocument.Parse(json, options))
            {
                var row = dataTable.NewRow();
                foreach (JsonProperty property in document.RootElement.EnumerateObject())
                {
                    string key = property.Name;
                    JsonElement valueElement = property.Value;
                    var colName = parent + "_" + key;
                    row[colName] = valueElement.JsonElementToTypedValue();;
                }
                dataTable.Rows.Add(row);
            }
        }

        private static Type ValueKindToType(this JsonValueKind valueKind, string value)
        {
            switch (valueKind)
            {
                case JsonValueKind.String:
                    return typeof(string);
                case JsonValueKind.Number:
                    if (long.TryParse(value, out _))
                    {
                        return typeof(long);
                    }
                    else
                    {
                        return typeof(double);
                    }
                case JsonValueKind.True:
                case JsonValueKind.False:
                    return typeof(bool);
                case JsonValueKind.Undefined:
                    throw new NotSupportedException();
                case JsonValueKind.Object:
                    return typeof(object);
                case JsonValueKind.Array:
                    return typeof(System.Array);
                case JsonValueKind.Null:
                    throw new NotSupportedException();
                default:
                    return typeof(object);
            }
        }

        private static object JsonElementToTypedValue(this JsonElement jsonElement)
        {
            switch (jsonElement.ValueKind)
            {
                case JsonValueKind.Object:
                case JsonValueKind.Array:
                    throw new NotSupportedException();
                case JsonValueKind.String:
                    if (jsonElement.TryGetGuid(out Guid guidValue))
                    {
                        return guidValue;
                    }
                    else
                    {
                        if (jsonElement.TryGetDateTime(out DateTime datetime))
                        {
                            // If an offset was provided, use DateTimeOffset.
                            if (datetime.Kind == DateTimeKind.Local)
                            {
                                if (jsonElement.TryGetDateTimeOffset(out DateTimeOffset datetimeOffset))
                                {
                                    return datetimeOffset;
                                }
                            }
                            return datetime;
                        }
                        return jsonElement.ToString();
                    }
                case JsonValueKind.Number:
                    if (jsonElement.TryGetInt64(out long longValue))
                    {
                        return longValue;
                    }
                    else
                    {
                        return jsonElement.GetDouble();
                    }
                case JsonValueKind.True:
                case JsonValueKind.False:
                    return jsonElement.GetBoolean();
                case JsonValueKind.Undefined:
                case JsonValueKind.Null:
                    return null;
                default:
                    return jsonElement.ToString();
            }
        }
    }
}
