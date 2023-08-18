// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using FuzzySharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

namespace ApprovalsKit.Extensibility.Plugins.Helpers
{
    /// <summary>
    /// The Fuzzy Data Table Mapper attemps to match a column name against columns in a DataTable
    /// 

    /// </summary>
    public class FuzzyDataTableMapper
    {
        /// <summary>
        /// 
        /// NOTE: The Match make use of FuzzyWuzzy algorithm to match columns
        /// 
        /// The FuzzyWuzzy algorithm is based on the Levenshtein distance, which is the minimum number of edits (insertions, deletions, or substitutions) 
        /// required to transform one string into another. The FuzzyWuzzy algorithm computes a similarity score between 0 and 100.
        /// Where 100 means the strings are identical and 0 means they have nothing in common.
        /// 
        /// The FuzzyWuzzy algorithm can handle spelling errors, typos, abbreviations, and different word orders.
        /// 
        /// However, it does not take into account the meaning or the context of the words
        /// </summary>
        /// <param name="name">The input column name to search for using fuzzy search</param>
        /// <param name="table">The data table to search</param>
        /// <returns>The matching column name if it can be found. A empty string if no match found with a match scor of 70 or better</returns>
        public string Match(string name, DataTable table)
        {
            Dictionary<string,string> columns = new Dictionary<string, string>();
            foreach ( DataColumn col in table.Columns )
            {
                // Exact match
                if (col.ColumnName.ToLower() == name.ToLower() )
                {
                    return col.ColumnName;
                }
                // Remove Camel Case, Snake Case and hypen seperator so that increases Fuzzy Match
                columns.Add(col.ColumnName, SplitTextIntoWords(col.ColumnName));
            }

            var match = Process.ExtractTop(name, columns.Select(c => c.Value)).Where(s => s.Score > 70);

            if ( match.Count() > 0 )
            {
                // Lookup the orginal column name
                return columns.FirstOrDefault(c => c.Value.Equals(match.First().Value)).Key;
            }

            return String.Empty;
        }

        /// <summary>
        /// Parse the input string and work break on CamelCase, snake_case and hyphen-delimited items
        /// </summary>
        /// <param name="input">The text to split into words</param>
        /// <returns>The split and joined text with space delimiter</returns>
        public static string SplitTextIntoWords(string input)
        {
            // Split on CamelCase
            string camelCasePattern = "(?<=.)(?=[A-Z])";
            string updated = String.Join(" ", Regex.Split(input, camelCasePattern));

            // Split on snake_case
            string snakeCasePattern = "_";
            updated = String.Join(" ", updated.Split(new string[] { snakeCasePattern }, StringSplitOptions.RemoveEmptyEntries));

            // Split on hyphen-separation
            string hyphenPattern = "-";
            updated = String.Join(" ", updated.Split(new string[] { hyphenPattern }, StringSplitOptions.RemoveEmptyEntries));

            return updated;
        }
    }
}
