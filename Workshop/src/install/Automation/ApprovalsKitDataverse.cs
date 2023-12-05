/*
 * This class is experimental and very likely to change until the demo feature is completed.
 * Use at your own risk.
 *
 * Ideal end state: Ability to automate the Power Automate Portal
 * to create and edit Power Automate Cloud flows for the Approvals Kit
 *
 */
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Query;

namespace Microsoft.PowerPlatform.Demo {
    public class ApprovalsKitDataverse {
        private Dictionary<string, string> _values;
        private ILogger _logger;

        public ApprovalsKitDataverse(Dictionary<string, string> values, ILogger logger) {
            _values = values;
            _logger = logger;
        }

        public async Task<bool> IsTwoStageWorkflowPublishedAndCloudFlowSaved(string workflowId, string cloudFlowId) {
            var client = new ServiceClient(new Uri(_values["environmentUrl"]), (env) => Task.FromResult(_values["token"]), true, _logger);

            // Use logical names
            QueryExpression query = new QueryExpression("cat_businessapprovalversion")
            {
                ColumnSet = new ColumnSet("cat_businessapprovalversionid", "cat_version")
            };
            FilterExpression childFilter = query.Criteria.AddFilter(LogicalOperator.Or);   
            query.AddOrder("cat_version", OrderType.Descending);
            query.TopCount = 1;
            childFilter.AddCondition("cat_process", ConditionOperator.Equal, workflowId);   

            var version = await client.RetrieveMultipleAsync(query);

            if (version.Entities.Count == 0)
            {
                return false;
            }

            query = new QueryExpression("cat_businessapprovalruntimestage")
            {
                ColumnSet = new ColumnSet("cat_businessapprovalruntimestageid")
            };
            query.Criteria.AddCondition("cat_processversion", ConditionOperator.Equal, version.Entities[0]["cat_businessapprovalversionid"]);   

            var stages = await client.RetrieveMultipleAsync(query);

            if (stages.Entities.Count < 2)
            {
                return false;
            }

            query = new QueryExpression("cat_businessapprovalruntimedata")
            {
                ColumnSet = new ColumnSet("cat_name", "cat_datatype")
            };
            query.Criteria.AddCondition("cat_processversion", ConditionOperator.Equal, version.Entities[0]["cat_businessapprovalversionid"]);   

            var dataItems = await client.RetrieveMultipleAsync(query);

            if (dataItems.Entities.Count < 2)
            {
                return false;
            }

            //TODO: Check if cloud flow setup

            return false;
        }

        /// <summary>
        /// Search for the Power Platform solution
        /// </summary>
        /// <param name="solutionName">The unique name or friendly name of the solution</param>
        /// <returns>The solution id if found</returns>
        public async Task<string> GetSolutionId(string solutionName) {
            var client = new ServiceClient(new Uri(_values["environmentUrl"]), (env) => Task.FromResult(_values["token"]), true, _logger);

            // Use logical names
            QueryExpression query = new QueryExpression("solution")
            {
                ColumnSet = new ColumnSet("solutionid", "uniquename")
            };
            FilterExpression childFilter = query.Criteria.AddFilter(LogicalOperator.Or);   
            childFilter.AddCondition("friendlyname", ConditionOperator.Equal, solutionName);   
            childFilter.AddCondition("uniquename", ConditionOperator.Equal, solutionName);

            var process = await client.RetrieveMultipleAsync(query);

            if (process.Entities.Count > 0)
            {
                var match = process.Entities[0];
                var solutionId = match["solutionid"]?.ToString();
                if (!String.IsNullOrEmpty(solutionId))
                {
                    return solutionId;
                }
            }

            return String.Empty;
        }

        /// <summary>
        /// Search for the Cloud Flow
        /// </summary>
        /// <param name="name">The cloud flows</param>
        /// <param name="solutionId">The solution id that the cloud flow</param>
        /// <returns>The workflowidunique if found</returns>
        public async Task<string> GetCloudFlowId(string name, string solutionId) {
            var client = new ServiceClient(new Uri(_values["environmentUrl"]), (env) => Task.FromResult(_values["token"]), true, _logger);

            // Use logical names
            QueryExpression query = new QueryExpression("solutioncomponent")
            {
                ColumnSet = new ColumnSet("objectid")
            };
            query.Criteria.AddCondition("solutionid", ConditionOperator.Equal, solutionId);
            query.Criteria.AddCondition("componenttype", ConditionOperator.Equal, 29); // Workflow

            var workflowComponents = await client.RetrieveMultipleAsync(query);

            if (workflowComponents.Entities.Count == 0)
            {
                return String.Empty;
            }
            
            foreach ( var objectid in workflowComponents.Entities ) {
                query = new QueryExpression("workflow")
                {
                    ColumnSet = new ColumnSet("workflowidunique")
                };
                var childFilter = query.Criteria.AddFilter(LogicalOperator.And);
                childFilter.AddCondition("workflowid", ConditionOperator.Equal, objectid["objectid"]);
                
                FilterExpression nameFilter = childFilter.AddFilter(LogicalOperator.Or);   
                nameFilter.AddCondition("name", ConditionOperator.Equal, name);   
                nameFilter.AddCondition("uniquename", ConditionOperator.Equal, name);
                childFilter.AddFilter(nameFilter);

                var process = await client.RetrieveMultipleAsync(query);

                if (process.Entities.Count > 0)
                {
                    var match = process.Entities[0];
                    var processId = match["workflowidunique"]?.ToString();
                    if (!String.IsNullOrEmpty(processId))
                    {
                        return processId;
                    }
                }
            }

            return String.Empty;
        }

        public async Task<string> GetApprovalWorkflowId(string workflowName) {
            if ( _values.ContainsKey("workflowId")) {
                return _values["workflowId"];
            }

            var client = new ServiceClient(new Uri(_values["environmentUrl"]), (env) => Task.FromResult(_values["token"]), true, _logger);

            // Use logical names
            QueryExpression query = new QueryExpression("cat_businessapprovalprocess")
            {
                ColumnSet = new ColumnSet("cat_businessapprovalprocessid", "cat_name")
            };
            query.AddOrder("createdon", OrderType.Descending);
            query.TopCount = 1;
            query.Criteria.AddCondition("cat_name", ConditionOperator.Equal, workflowName);

            var process = await client.RetrieveMultipleAsync(query);

            if (process.Entities.Count > 0)
            {
                var match = process.Entities[0];
                var workflowId = match["cat_businessapprovalprocessid"]?.ToString();
                if (!String.IsNullOrEmpty(workflowId))
                {
                    _values.Add("workflowId", workflowId);
                    return workflowId;
                }
            }

            return String.Empty;
        }
    }
}