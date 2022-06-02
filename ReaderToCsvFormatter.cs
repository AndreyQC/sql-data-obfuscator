using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Globalization;

namespace SQLDataObfuscator
{

    public class ReaderToCsvFormatter : IDisposable
    {
        private readonly SqlDataReader _reader;
        private readonly Query _queryDefinition;
        private readonly SqlTransaction _tx;
        private static readonly Random getrandom = new Random();
        int _rndsign;
        int _rndnumber;

        public ReaderToCsvFormatter(Query query, DateTime? startDate, DateTime? endDate, string startDateName, string endDateName)
        {
            _queryDefinition = query;
            var connectionString = ConfigurationManager.ConnectionStrings["Obfuscator:SourceDatabase"].ConnectionString;
            var connection = new SqlConnection(connectionString);
            var command = connection.CreateCommand();
            command.CommandText = _queryDefinition.Sql;
            command.CommandType = CommandType.Text;
            command.CommandTimeout = Int32.MaxValue;

            if (startDate.HasValue && endDate.HasValue)
            {
                var sqlParameterStart = new SqlParameter(startDateName, startDate.Value);
                command.Parameters.Add(sqlParameterStart);
                var sqlParameterEnd = new SqlParameter(endDateName, endDate.Value);
                command.Parameters.Add(sqlParameterEnd);
            }

            connection.Open();
            _tx = connection.BeginTransaction(IsolationLevel.ReadUncommitted);
            command.Transaction = _tx;
            _reader = command.ExecuteReader(CommandBehavior.CloseConnection);
        }

        public bool ReadLine()
        {
            //regenrate random sign and number 
            _rndsign = GetRandomSign();
            _rndnumber = GetRandomNumber(0, 1000);
            return _reader.Read();
        }

        public string MapHeader()
        {
            char b = '\u0001';
            var sb = new StringBuilder();
            for (var i = 0; i < _queryDefinition.Columns.Length; i++)
            {
                var columnName = _queryDefinition.Columns[i];
                sb.Append(columnName);
                if (i + 1 != _queryDefinition.Columns.Length)
                {
                    sb.Append(b);
                }
            }
            return sb.ToString();
        }

        public string MapRecordLine()
        {
            var sb = new StringBuilder();
            for (var i = 0; i < _queryDefinition.Columns.Length; i++)
            {
                var columnName = _queryDefinition.Columns[i];
                var value = _reader.GetValue(_reader.GetOrdinal(columnName));
                // generate new rnd sign
                var obf_value = GetObfuscatedValue(columnName, _queryDefinition.ColumnObfucsationRules, value);
                // apply rules 
                value = obf_value;

                if (_queryDefinition.ForceDateFields.Any(x => x == columnName))
                {
                    if (value == null || value == DBNull.Value)
                    {
                        sb.Append(ObfuscatorConfiguration.Current.NullString);
                    }
                    else
                    {
                        var date = DateTime.ParseExact(value.ToString(), "yyyyMMdd", CultureInfo.InvariantCulture,
                            DateTimeStyles.None);
                        var valueString = ((DateTime)date).ToString("yyyy-MM-dd HH:mm:ss");
                        sb.Append(valueString);
                    }
                }
                else
                {
                    if (value == null || value == DBNull.Value)
                    {
                        sb.Append(ObfuscatorConfiguration.Current.NullString);
                    }
                    else
                    {
                        if (value is DateTime)
                        {
                            value = ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss");
                        }
                        var valueString = value?.ToString().Replace(",", " ") ?? "";
                        sb.Append(valueString);
                    }
                }

                if (i + 1 != _queryDefinition.Columns.Length)
                {
                    char b = '\u0001';
                    sb.Append(b);
                }
            }
            return sb.ToString();
        }

        private object GetObfuscatedValue(string columnName, List<ColumnObfucsationRule> columnObfucsationRules, object value)
        {
            var obf_value = value;
            if (value == null || value == DBNull.Value || value is DateTime)
            {
                return obf_value;
            }
            else
            {
                ObfuscationRule obf_rule = GetRule(columnName, columnObfucsationRules);
                if (obf_rule != null)
                {
                    var rnd_order = 0 ;
                    // find sys column in reader
                    if ((obf_rule.SysRndColumn != null))
                    {
                        try
                        {
                            
                            rnd_order = Convert.ToInt32(_reader.GetValue(_reader.GetOrdinal(obf_rule.SysRndColumn)));
                            

                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("{0} Exception caught.", e);
                            throw;
                        }
                    }

                    switch (obf_rule.RuleType)
                    {
                        case "ReplaceBySysRnd":

                            obf_value = obf_rule.Prefix + rnd_order.ToString();

                            break;
                        case "NumericVariance":
                            if (Convert.ToInt32(obf_value) != 0)
                            {
                                obf_value = Math.Abs(Convert.ToInt32(obf_value) + _rndsign * _rndnumber);
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            return obf_value;
        }

        private ObfuscationRule GetRule(string columnName, List<ColumnObfucsationRule> columnObfucsationRules)
        {
            ObfuscationRule _obf_rule = null;
            foreach (var obf_rule in columnObfucsationRules)
            {
                if (obf_rule.Column == columnName)
                {
                    _obf_rule = obf_rule.ObfuscationRule;
                }
            }
            return _obf_rule;
        }

        public void Dispose()
        {
            _tx.Dispose();
            _reader.Close();
        }

        public int GetRandomSign()
        {
            if (GetRandomNumber(0, 1) == 0)
            {
                return -1;
            }
            return 1;
        }

        public static int GetRandomNumber(int min, int max)
        {
            lock (getrandom) // synchronize
            {
                return getrandom.Next(min, max);
            }
        }

    }

}
