using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLDataObfuscator
{
    public class Query
    {
        public Query()
        {
            Parameters = new QueryParameter[0];
            Columns = new string[0];
            ForceDateFields = new string[0];
            ColumnObfucsationRules = new List<ColumnObfucsationRule>();
        }

        public bool Enabled { get; set; }

        public string[] Columns { get; set; }

        public string[] ForceDateFields { get; set; }

        public string Name { get; set; }

        public string ConnectionStringName { get; set; }

        public string Sql { get; set; }

        public string Type { get; set; }

        public int CommandTimeout { get; set; }

        public QueryParameter[] Parameters { get; set; }

        public List<ColumnObfucsationRule> ColumnObfucsationRules { get; set; }

    }
}
