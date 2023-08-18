using ApprovalsKit.Extensibility.Plugins;
using ApprovalsKit.Extensibility.Plugins.Helpers;
using ApprovalsKit.Extensibility.Plugins.Models;
using FakeItEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace ApprovalsKit.Extensibility.Tests
{
    [TestClass]
    public class ApprovalsKitDataMapperTests : PluginTestBase
    {
        static ApprovalsKitDataMapperTests()
        {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var name = new AssemblyName(args.Name);
            if (name.Name.Contains("System.Runtime.CompilerServices.Unsafe"))
            {
                return typeof(System.Runtime.CompilerServices.Unsafe).Assembly;
            }
            return null;
        }

        [TestMethod]
        public void CreatesBusinessApprovalRunTimeInstance()
        {
            // Arrange
            var fakeServiceProvider = A.Fake<IServiceProvider>();
            var fakeLookup = A.Fake<ApprovalProcessLookup>();

            var created = new List<Entity>();

            var (fakePluginExecutionContext, fakeOrganizationService) = SetupPluginFakes(fakeServiceProvider);

            var data = new {
                A = "Test",
                B = DateTime.Now,
                C = 12.5
            };

            var newId = Guid.NewGuid();

            const string WORKFLOW_NAME = "Test Workflow";

            var parameters = new ParameterCollection();
            parameters.Add("Approval", WORKFLOW_NAME);
            parameters.Add("Data", JsonSerializer.Serialize(data));

            var lookupResult = new ApprovalProcessLookupResult();
            lookupResult.BusinessApprovalPublishedWorkflow = new Entity("cat_businessapprovalpublishedworkflow");

            A.CallTo(() => fakeLookup.Retreive(WORKFLOW_NAME)).Returns(lookupResult);

            EntityCollection results = new EntityCollection();
            var attribs = new AttributeCollection();
            attribs.Add("businessapprovalprocess", Guid.NewGuid());
            results.Entities.Add(new Entity("foo") { Id = Guid.NewGuid(), Attributes = attribs });
            results.TotalRecordCount = 1;

            A.CallTo(() => fakePluginExecutionContext.InputParameters).Returns(parameters);
           

            A.CallTo(
               () => fakeOrganizationService.Create(A<Entity>.Ignored)).ReturnsLazily(x => { 
                   
                   var newItem = x.Arguments.Get<Entity>(0);
                   newItem.Id = newId;
                   created.Add(newItem);
                   return newId;
                }
            );    

            // Act
            var plugin = new ApprovalsKitDataMapper("", "");
            plugin.Lookup = fakeLookup;
            plugin.Execute(fakeServiceProvider);

            // Assert
            Assert.AreEqual(1, created.Count(c => c.LogicalName == "cat_businessapprovalworkflow"));
            Assert.AreEqual(newId, created.FirstOrDefault(c => c.LogicalName == "cat_businessapprovalworkflow").Id);

        }
    }
}
