using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TelcobrightMediation.SansayWs;

namespace TelcobrightMediation.Sansay
{
    public class SansaySwitch
    {
        private string _protocol;
        private string _ipAddress;
        private int _port;
        private readonly string _username;
        private readonly string _password;
        private readonly SansayWS _wsPort;

        public SansaySwitch(string protocol, string ipAddress, int port, string username, string password)
        {
            _protocol = protocol;
            _ipAddress = ipAddress;
            _port = port;
            _username = username;
            _password = password;
            _wsPort = new SansayWSClient("SansayWSPort", new EndpointAddress($"{protocol}://{ipAddress}:{port}/SSConfig/SansayWS"));
        }

        public bool DownloadXmlFile(string table, string destinationDirectory)
        {
            doDownloadXmlFileResponse result;
            int index = 0;
            do
            {
                downloadParams dparams = new downloadParams
                {
                    table = table,
                    username = _username,
                    password = _password,
                    page = index
                };
                string xmlFileName = $"{destinationDirectory}\\{table}_{index++}.xml";
                doDownloadXmlFileRequest drequest = new doDownloadXmlFileRequest(dparams);
                result = _wsPort.doDownloadXmlFile(drequest);

                if (result.downloadResult.retCode == (int) SansayWsReturnCodes.Ok)
                {
                    StreamWriter file = new StreamWriter(xmlFileName);
                    file.Write(result.downloadResult.xmlfile);
                    file.Close();
                }
                else return false;

            } while (result.downloadResult.hasMore == 1);
            return true;
        }

        private List<String> GetAvailableTrunks(XDocument document)
        {
            List<string> trunkList = new List<string>();
            foreach (XElement descendant in document.Descendants("XBResource"))
            {
                trunkList.Add(descendant.Descendants("trunkId").First().Value);
            }
            return trunkList;
        }

        private string GetCapacity(XDocument document, string trunkId)
        {
            XElement element = document.Descendants("XBResource").First(x => x.Descendants("trunkId").First().Value == trunkId);
            return element.Descendants("capacity").First().Value;
        }
    }
}
