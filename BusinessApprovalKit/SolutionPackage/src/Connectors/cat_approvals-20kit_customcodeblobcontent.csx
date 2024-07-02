public class Script : ScriptBase
{
    private class RuntimeData
    {
        public string id { get; set; }
        public string value { get; set; }
        public string name { get; set; }
    }
    public override async Task<HttpResponseMessage> ExecuteAsync()
    {
        // Check if the operation ID matches what is specified in the OpenAPI definition of the connector
        if (this.Context.OperationId == "GetApprovalDataFields")
        {
            return await this.ModifySchema().ConfigureAwait(false);
        }
        // Create record in workflowqueue table
        if (this.Context.OperationId == "CreateWorkflowInstance")
        {
            return await this.CreateWorkflowQueue().ConfigureAwait(false);
        }
        // Handle an invalid operation ID
        HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.BadRequest);
        response.Content = CreateJsonContent(
            $"Unknown operation ID '{this.Context.OperationId}'"
        );
        return response;
    }

    // We do a post request to create record in table - businessapprovalworkflowqueues
    private async Task<HttpResponseMessage> CreateWorkflowQueue()
    {
        var processId = this.Context.Request.Headers.GetValues("selectedProcess").First(); //cat_processid
        var dynamicParameters = JObject.Parse(await this.Context.Request.Content.ReadAsStringAsync().ConfigureAwait(false));
        List<RuntimeData> runtimeDatalist = new List<RuntimeData>();
        foreach (var dynamicParameter in dynamicParameters)
        {
            runtimeDatalist.Add(new RuntimeData { id = dynamicParameter.Key, value = dynamicParameter.Value.ToString(), name = dynamicParameter.ToString() });
        }
        RuntimeData[] runtimeDataArray = runtimeDatalist.ToArray<RuntimeData>();
        var newBody = new JObject
            {
                ["cat_processid"] = processId,
                ["cat_runtimedata"] = JsonConvert.SerializeObject(runtimeDataArray)
            };
        // Replace the content with support schema & value
        this.Context.Request.Content = CreateJsonContent(newBody.ToString());
        return await this.Context
                    .SendAsync(this.Context.Request, this.CancellationToken)
                    .ConfigureAwait(false);
    }

    // We modify the schema here to support custom connector reading dynamic schema of OpenAPI Schema
    private async Task<HttpResponseMessage> ModifySchema()
    {
        // Use the context to forward/send an HTTP request
        HttpResponseMessage response = await this.Context
            .SendAsync(this.Context.Request, this.CancellationToken)
            .ConfigureAwait(continueOnCapturedContext: false);

        // Do the transformation if the response was successful, otherwise return error responses as-is
        if (response.IsSuccessStatusCode)
        {
            var responseString = await response.Content
                .ReadAsStringAsync()
                .ConfigureAwait(continueOnCapturedContext: false);
            var result = JObject.Parse(responseString);
            // Intialize variables
            // Maintain list of required fields
            List<string> requiredFieldsList = new List<string>();
            var customizedProperties = new JObject();
            // Get parent property
            JObject parentNodetobModified = (JObject)result.SelectToken("schemavalue.properties");
            // Get property (property/property)
            JObject propertyNodetobModified = (JObject)
                result.SelectToken("schemavalue.properties.properties");
            propertyNodetobModified.Property("@odata.type").Remove();
            foreach (var _property in propertyNodetobModified.Properties())
            {
                // We swap the property name with title id
                // This is required to prepare data for Approval workflow queue creation
                string runtimeId = "";
                foreach (var _object in _property)
                {
                    if (_object["title"].ToString() != "Additional Information")
                    {
                        _object["x-ms-visibility"] = "important";
                        runtimeId = _object["title"].ToString() != "External Reference" ? _object["title"].ToString() : "externalreference";
                        //_object["id"] = _property.Name;
                    }
                    else
                    {
                        _object["x-ms-visibility"] = "advanced";
                        runtimeId = "additionalinformation";
                        _object["id"] = runtimeId;
                    }
                   
                }

                customizedProperties.Add(new JProperty(runtimeId, _property.Value));
                if (runtimeId != "additionalinformation" && runtimeId != "externalreference")
                {
                    requiredFieldsList.Add(runtimeId);
                }

            }
            // Remove old property node
            parentNodetobModified.Property("properties").Remove();
            // Add new property node which has the name swapped by id
            parentNodetobModified.Add("properties", customizedProperties);
            // Add RequiredFields Array
            String[] requiredFieldArray = requiredFieldsList.ToArray();
            parentNodetobModified.Add("required", JToken.FromObject(requiredFieldArray));
            response.Content = CreateJsonContent(result.ToString());
        }

        return response;
    }

}
