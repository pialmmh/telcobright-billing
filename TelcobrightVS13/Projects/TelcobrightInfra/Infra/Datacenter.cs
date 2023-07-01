using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions.ConfigHelper;

namespace TelcobrightInfra
{
    public class FacilityAddress
    {
        public string Name { get; set; } = "default";
        public FacilityAddress(string name)
        {
            Name = name;
        }
    }

    public class Cloud
    {
        public string Name { get; set; } = "default";
        public List<Datacenter> Datacenters { get; set; }= new List<Datacenter>();

        public Cloud(string name)
        {
            Name = name;
        }
    }

    public class Datacenter
    {
        public string Name { get; set; } = "default";
        public FacilityAddress FacilityAddress { get;}
        public Cloud Cloud { get; }
        public Dictionary<string, Server> PhysicalServers { get; set; }= new Dictionary<string, Server>();

        public Datacenter(string name, FacilityAddress facilityAddress, Cloud cloud)
        {
            Name = name;
            FacilityAddress = facilityAddress;
            Cloud = cloud;
        }
    }

    public class Vm:Server
    {
        public Server Host { get; set; }
        public Vm(string name) : base(name)
        {
        }
    }

    public enum ApplicationServiceCategory
    {
        Db,
        Webserver,
        FtpServer,
        DotnetApp,
        JavaApp
    }
    public enum ApplicationServiceType
    {
        MySql,
        IIS_Web,
        IIS_Ftp,
        Linux_FtpD
    }

    public enum ApplicationContainerType
    {
        Docker,
        LXD
    }

    public class ApplicationContainer
    {
        public string Name { get; set; }
        public ApplicationContainerType ApplicationContainerType { get; set; }
        public Server ServerOrVm { get; }

        public ApplicationContainer(ApplicationContainerType applicationContainerType,
            Server serveOrVm)
        {
            ApplicationContainerType = applicationContainerType;
            this.ServerOrVm = serveOrVm;
        }
    }

    public class ApplicationService
    {
        public string Name { get; set; }
        public ApplicationServiceCategory ApplicationServiceCategory { get; set; }
        public ApplicationServiceType ApplicationServiceType { get; set; }
        public Server HostVmOrServer { get; set; }
        public ApplicationContainer ApplicationContainer { get; set; }
        public List<BindAddress> BindAddresses { get; } = new List<BindAddress>();
    }
}
