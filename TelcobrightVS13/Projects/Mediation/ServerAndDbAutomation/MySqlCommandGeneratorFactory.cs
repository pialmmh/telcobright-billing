namespace TelcobrightMediation
{
    public class MySqlCommandGeneratorFactory
    {
        public MySqlVersion version { get; set; }

        public MySqlCommandGeneratorFactory(MySqlVersion version)
        {
            this.version = version;
        }

        public MySqlCommandGenerator getInstance()
        {
            switch (this.version)
            {
                case MySqlVersion.MySql57:
                    return new MySql7CommandGenerator();
                case MySqlVersion.MySql8:
                    return new MySql8CommandGenerator();
                default:
                    return new MySqlCommandGenerator();
            }
        }
    }
}