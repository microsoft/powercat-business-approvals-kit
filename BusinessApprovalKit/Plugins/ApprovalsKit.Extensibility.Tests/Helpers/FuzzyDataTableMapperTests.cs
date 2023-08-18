// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using ApprovalsKit.Extensibility.Plugins.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;

namespace ApprovalsKit.Extensibility.Tests.Helpers
{
    [TestClass]
    public class FuzzyDataTableMapperTests
    {
        [DataTestMethod]
        [DataRow("A", "A", "A")]
        [DataRow("Your name", "Name", "Name")]
        [DataRow("A", "X,Y,Z", "")]
        [DataRow("Phone number", "xxx_PhoneNumber", "xxx_PhoneNumber")]
        public void MatchColumn(string input, string columns, string match)
        {

            Assert.AreEqual(match, new FuzzyDataTableMapper().Match(input, Create(columns)));
        }

        [DataTestMethod]
        [DataRow("CamelCase", "Camel Case")]
        [DataRow("snake_case", "snake case")]
        [DataRow("hypen-delimited", "hypen delimited")]
        [DataRow("CamelCase and snake_case", "Camel Case and snake case")]
        public void RemoveConventionTest(string input, string expected)
        {
            Assert.AreEqual(expected, FuzzyDataTableMapper.SplitTextIntoWords(input));
        }

        private DataTable Create(string columns)
        {
            var result = new DataTable();
            foreach (var col in columns.Split(','))
            {
                result.Columns.Add(col);
            }
            return result;
        }
    }
}
