// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using ApprovalsKit.Extensibility.Plugins.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Reflection;

namespace ApprovalsKit.Extensibility.Tests.Helpers
{
    [TestClass]
    public class ApprovalDataMapperTests
    {
        static ApprovalDataMapperTests()
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

        [DataTestMethod]
        [DataRow(@"[{""A"":1}]", "A", 1 )]
        [DataRow(@"[{""A"":""text""}]", "A", "text" )]
        [DataRow(@"[{""A"":{""B"":1}}]", "A_B", 1 )]
        public void MapJson(string json, string delimitedColumnNames, object expectedValue)
        {
            // Arrange
            var mapper = new ApprovalDataMapper(null);
            var columns = delimitedColumnNames.Split(',');

            // Act
            var results = mapper.Convert(json);

            // Assert
            Assert.AreEqual(columns.Count(), results.Columns.Count);

            if ( columns.Count() == 1 && results.Rows.Count == 1 )
            {
                Assert.AreEqual(expectedValue.ToString(), results.Rows[0][0].ToString());
            }
        }
    }
}
