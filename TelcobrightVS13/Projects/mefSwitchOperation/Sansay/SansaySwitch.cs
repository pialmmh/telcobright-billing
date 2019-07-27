using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TelcobrightMediation.SansayWs;

namespace TelcobrightMediation.Sansay
{
    public class SansaySwitch : ISwitchAction
    {
        private readonly string _ipAddress;
        private readonly string _username;
        private readonly string _password;
        private readonly SansayWS _wsPort;
        private string WorkingDirectory = "C:\\temp\\vsxi_ws_client1_3_1_15";

        public SansaySwitch(string protocol, string ipAddress, int port, string username, string password)
        {
            _ipAddress = ipAddress;
            _username = username;
            _password = password;
            _wsPort = new SansayWSClient("SansayWSPort", new EndpointAddress($"{protocol}://{ipAddress}:{port}/SSConfig/SansayWS"));
        }

        public List<String> DownloadXmlFileUsingClient(string table, string trunkId)
        {
            List<string> files = new List<string>();
            string xmlFileName = Path.Combine(WorkingDirectory, $"{table}.xml");
            ProcessStartInfo startInfo = new ProcessStartInfo($"java")
            {
                WorkingDirectory = WorkingDirectory,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };
            startInfo.Arguments = $"-jar VSXi_WS_Client.jar -a{_ipAddress} -u{_username} -p{_password} query {xmlFileName} {table} \"trunk_id=\'{trunkId}\'\"";
            Process process = Process.Start(startInfo);
            process?.WaitForExit();
            files.Add(xmlFileName);
            return files;
        }

        public List<string> DownloadXmlFileUsingSoap(string table)
        {
            List<string> files = new List<string>();
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
                string xmlFileName = Path.Combine(WorkingDirectory, $"{table}_{index++}.xml");
                doDownloadXmlFileRequest drequest = new doDownloadXmlFileRequest(dparams);
                result = _wsPort.doDownloadXmlFile(drequest);

                if (result.downloadResult.retCode == (int) SansayWsReturnCodes.Ok)
                {
                    StreamWriter file = new StreamWriter(xmlFileName);
                    file.Write(result.downloadResult.xmlfile);
                    file.Close();
                    files.Add(xmlFileName);
                }
                else throw new Exception(result.downloadResult.msg);

            } while (result.downloadResult.hasMore == 1);
            return files;
        }

        public bool UploadXmlFileUsingClient(string table, string xmlFileName)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo($"java")
            {
                WorkingDirectory = WorkingDirectory,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };
            startInfo.Arguments = $"-jar VSXi_WS_Client.jar -a{_ipAddress} -u{_username} -p{_password} upload {xmlFileName} {table}";
            Process process = Process.Start(startInfo);
            process?.WaitForExit();
            return true;
        }

        public bool UploadXmlFileUsingSoap(string table, string xmlFileName)
        {
            if (!File.Exists(xmlFileName)) throw new Exception("File do not exists.");
            string xmlfile = File.ReadAllText(xmlFileName);
            uploadParams uparams = new uploadParams()
            {
                table = table,
                username = _username,
                password = _password,
                xmlfile = xmlfile
            };
            doUploadXmlFileRequest request = new doUploadXmlFileRequest(uparams);
            doUploadXmlFileResponse response = _wsPort.doUploadXmlFile(request);
            if (response.uploadResult.retCode == (int) SansayWsReturnCodes.Ok) return true;
            else throw new Exception(response.uploadResult.msg);
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

        private XDocument GetFileContent(string xmlFileName)
        {
            if (!File.Exists(xmlFileName)) throw new Exception("File do not exists.");
            return XDocument.Load(xmlFileName);
        }

        private XDocument ChangeCapacity(XDocument document, string trunkId, string newCapacity)
        {
            XElement element = document.Descendants("XBResource").First(x => x.Descendants("trunkId").First().Value == trunkId);
            element.Descendants("capacity").First().Value = newCapacity;
            foreach (XElement node in element.Descendants("node"))
            {
                node.Descendants("capacity").First().Value = newCapacity;
            }
            return document;
        }

        public bool Block(string trunkId)
        {
            string table = "resource";
            List<string> files = this.DownloadXmlFileUsingClient(table, trunkId);
            if (files.Count > 0)
            {
                foreach (string file in files)
                {
                    XDocument document = this.ChangeCapacity(this.GetFileContent(file), trunkId, "0");
                    document.Save(file);
                    this.UploadXmlFileUsingClient(table, file);
                }
                return true;
            }
            return false;
        }
    }
}
