// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using Microsoft.Xrm.Sdk;
using System.Data;
using System.Text.Json;

namespace ApprovalsKit.Extensibility.Plugins.Helpers
{
    public class ApprovalDataMapper
    {
        private IOrganizationService _service;

        public ApprovalDataMapper(IOrganizationService service)
        {
            _service = service;
            
        }

        public DataTable Convert(string json)
        {
            JsonSerializerOptions options = new JsonSerializerOptions() { Converters = { new DataTableJsonConverter() }, WriteIndented = true };

            return JsonSerializer.Deserialize<DataTable>(json, options);
        }
    }
}
