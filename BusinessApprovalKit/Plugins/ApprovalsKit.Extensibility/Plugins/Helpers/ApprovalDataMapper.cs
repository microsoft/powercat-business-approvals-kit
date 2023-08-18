using ApprovalsKit.Extensibility.Plugins.Models;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
