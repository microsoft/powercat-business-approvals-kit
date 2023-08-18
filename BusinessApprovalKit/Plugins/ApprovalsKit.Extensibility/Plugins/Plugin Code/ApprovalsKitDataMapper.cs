// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using ApprovalsKit.Extensibility.Plugins.Helpers;
using Microsoft.Xrm.Sdk;
using System;

namespace ApprovalsKit.Extensibility.Plugins
{
    /// <summary>
    /// Dataverse unbound plugin that maps JSON data to create an instance Approval Workflow process and associated data
    /// </summary>
    public class ApprovalsKitDataMapper : PluginBase
    {
        private ApprovalProcessLookup _lookup;
        private IOrganizationService _service;
        private ApprovalDataMapper _mapper;

        
        public ApprovalProcessLookup Lookup
        {
            get
            {
                if (_lookup == null)
                {
                    _lookup = new ApprovalProcessLookup(_service);
                }
                return _lookup;
            }
            set
            {
                _lookup = value;
            }
        }

        public ApprovalDataMapper Mapper
        {
            get
            {
                if (_mapper == null)
                {
                    _mapper = new ApprovalDataMapper(_service);
                }
                return _mapper;
            }
            set
            {
                _mapper = value;
            }
        }


        public ApprovalsKitDataMapper(string unsecureConfiguration, string secureConfiguration)
            : base(typeof(ApprovalsKitDataMapper))
        {
            // TODO: Implement your custom configuration handling
            // https://docs.microsoft.com/powerapps/developer/common-data-service/register-plug-in#set-configuration-data
        }

        // Entry point for custom business logic execution
        protected override void ExecuteDataversePlugin(ILocalPluginContext localPluginContext)
        {
            if (localPluginContext == null)
            {
                throw new ArgumentNullException(nameof(localPluginContext));
            }

            var context = localPluginContext.ServiceProvider.GetService(typeof(IPluginExecutionContext)) as IPluginExecutionContext;

            // TODO add logging
            
            // TODO: Check for the entity on which the plugin would be registered

            // TODO: Check expected input parameters Approval 

            if (context.InputParameters.Contains("Approval") && context.InputParameters["Approval"] is string)
            {
                var factory = localPluginContext.ServiceProvider.GetService(typeof(IOrganizationServiceFactory)) as IOrganizationServiceFactory;
                var service = factory.CreateOrganizationService(null);

                this._service = service;

                if (Lookup == null)
                {
                    Lookup = new ApprovalProcessLookup(service);
                }

                var approvalName = (string)context.InputParameters["Approval"];

                var match = Lookup.Retreive(approvalName);

                if ( match != null )
                {
                    var work = new Entity("cat_businessapprovalworkflow");
                    work.Attributes["workflow"] = match.BusinessApprovalPublishedWorkflow.GetAttributeValue<Guid>("cat_businessapprovalpublishedworkflowid");
                    service.Create(work);

                    if ( Mapper == null )
                    {

                    }

                    // TODO create Business Approval Runtime from Data input parameter
                }
            }
        }
    }
}
