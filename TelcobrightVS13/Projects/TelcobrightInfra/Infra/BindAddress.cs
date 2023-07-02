namespace TelcobrightInfra
{
    public enum BindAddressRealmOrZone
    {
        Private,
        Public,
        Demilitarized,
        Militarized
    }

    public enum BindAddressType
    {
        Network,
        NamedPipeOrSocket
    }

    public class BindAddress
    {
        public string Name { get; set; } = "default";
        public BindAddressType BindAddressType { get; set; }
        public BindAddressRealmOrZone ReamOrZone { get; set; }
        public IpAddress IpAddress { get; set; }
        public int Port { get; set; }
        public TransportProtocol TransportProtocol { get; set; }

    }
}