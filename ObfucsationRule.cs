namespace SQLDataObfuscator
{
    public class ColumnObfucsationRule
    {
        public string Column { get; set; }
        public bool Enabled { get; set; }
        public ObfuscationRule ObfuscationRule { get; set; }
    }

    public class ObfuscationRule
    {
        public ObfuscationRule()
        {
            RuleType = "new";
            SysRndColumn = null;
            Prefix = null;
        }

        public string RuleType { get; set; }
        public string SysRndColumn { get; set; }
        public string Prefix { get; set; }


    }
}
