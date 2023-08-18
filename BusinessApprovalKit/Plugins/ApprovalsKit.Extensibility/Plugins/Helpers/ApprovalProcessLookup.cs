// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using ApprovalsKit.Extensibility.Plugins.Models;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace ApprovalsKit.Extensibility.Plugins.Helpers
{
    /// <summary>
    /// Helper class to lookup a active published Approval process
    /// </summary>
    public class ApprovalProcessLookup
    {
        private IOrganizationService _service { get; set; }
        public ApprovalProcessLookup(IOrganizationService service)
        {
            _service = service;
        }

        /// <summary>
        /// Retreive information related to the matching Approval process
        /// </summary>
        /// <param name="approvalName">The active published workflow to retreive</param>
        /// <returns>The matching Approval Process version and data</returns>
        /// <exception cref="ApplicationException"></exception>
        public virtual ApprovalProcessLookupResult Retreive(string approvalName)
        {
            ApprovalProcessLookupResult result = new ApprovalProcessLookupResult();

            QueryExpression qryProcess = new QueryExpression("cat_businessapprovalprocess")
            {
                ColumnSet = new ColumnSet("businessapprovalprocess", "name")
            };
            qryProcess.Criteria.AddCondition("name", ConditionOperator.Equal, approvalName);

            var results = _service.RetrieveMultiple(qryProcess);

            if (results.TotalRecordCount > 0)
            {
                result.BusinessApprovalProcess = results[0];
            }
            else
            {
                throw new ApplicationException($"Unable to find process {approvalName}");
            }

            const int Active = 0;

            qryProcess = new QueryExpression("cat_businessapprovalprocess")
            {
                ColumnSet = new ColumnSet("businessapprovalprocess", "name")
            };
            qryProcess.Criteria.AddCondition("Process", ConditionOperator.Equal, result.BusinessApprovalProcess.Id);
            qryProcess.Criteria.AddCondition("Status", ConditionOperator.Equal, Active);
            qryProcess.AddOrder("Version", OrderType.Descending);

            results = _service.RetrieveMultiple(qryProcess);

            if (results.TotalRecordCount > 0)
            {
                result.BusinessApprovalVersion = results[0];
            }
            else
            {
                throw new ApplicationException($"Unable to find active process version for {approvalName}");
            }

            // TODO Get Data collection, stages, conditions, nodes

            return result;
        }
    }
}
