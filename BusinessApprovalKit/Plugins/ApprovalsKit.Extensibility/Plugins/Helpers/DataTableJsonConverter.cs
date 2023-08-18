using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ApprovalsKit.Extensibility.Plugins.Helpers
{
    public class DataTableJsonConverter : JsonConverter<DataTable>
    {
        public override DataTable Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using (var jsonDoc = JsonDocument.ParseValue(ref reader))
            {
                JsonElement rootElement = jsonDoc.RootElement;
                DataTable dataTable = rootElement.JsonElementToDataTable();
                return dataTable;
            }
        }

        public override void Write(Utf8JsonWriter writer, DataTable value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
