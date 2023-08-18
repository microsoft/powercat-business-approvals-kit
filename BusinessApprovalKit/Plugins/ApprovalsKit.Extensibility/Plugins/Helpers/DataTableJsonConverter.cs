// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApprovalsKit.Extensibility.Plugins.Helpers
{
    /// <summary>
    /// Convert from JSON to a <seealso cref="DataTable"/> using DataTable extensions
    /// </summary>
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
