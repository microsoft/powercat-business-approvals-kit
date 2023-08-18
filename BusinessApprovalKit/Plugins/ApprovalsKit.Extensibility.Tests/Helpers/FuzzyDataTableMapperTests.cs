using ApprovalsKit.Extensibility.Plugins.Helpers;
using Microsoft.SqlServer.Server;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
