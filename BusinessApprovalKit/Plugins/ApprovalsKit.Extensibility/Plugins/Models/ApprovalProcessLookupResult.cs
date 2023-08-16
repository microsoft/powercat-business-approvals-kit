using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApprovalsKit.Extensibility.Plugins.Models
{
    /// <summary>
    /// POCO class to wrap related Approval Process lookup results
    /// </summary>
    public class ApprovalProcessLookupResult
    {
        /// <summary>
        /// The matching process
        /// </summary>
        public Entity BusinessApprovalProcess { get; set; }

        /// <summary>
        /// The matching deploy version
        /// </summary>
        public Entity BusinessApprovalVersion { get; set; }

        /// <summary>
        /// The versioned approval workflow
        /// </summary>
        public Entity BusinessApprovalPublishedWorkflow { get; set; }

        /// <summary>
        /// Data inputs for the published workflow
        /// </summary>
        public EntityCollection BusinessApprovalPublishedRuntimeData { get; set; }
    }
}
