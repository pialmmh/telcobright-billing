using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Renci.SshNet;
using System.Diagnostics;
namespace FormSSHTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void connect_Click(object sender, EventArgs e)
        {
            //SshClient cSSH = new SshClient("192.168.2.7", 22, "root", "Habib321");
            using (SshClient ssh = new SshClient("192.168.2.7",22,
    "root", "Habib321"))
            {
                ssh.Connect();
                var cmd = ssh.CreateCommand("asterisk -rx 'sip show peers'");
                var result = cmd.Execute();
                List<string> lines = result.Split(Convert.ToChar(10)).ToList();
                List<string> withoutFirstLine = lines.Skip(1).ToList();
                var WihoutFirstAndLastLine = withoutFirstLine.Take(withoutFirstLine.Count - 2).ToList();
                List<Phone> phones = PhonesGenerator(WihoutFirstAndLastLine);
                string display = string.Join(Environment.NewLine, phones.Select(c => c.name + " (" + c.extension + ")" + "\t\t\t" + c.GetStatus()));
                MessageBox.Show(display);
               // Debug.WriteLine(result);
                ssh.Disconnect();
            }
 
        }
        public List<Phone> PhonesGenerator(List<string> lines)
        {
            List<Phone> phones = new List<FormSSHTest.Phone>();
            lines.ToList().ForEach(line =>
            {
                if(string.IsNullOrWhiteSpace(line)==false)
                {
                    string[] arr = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    string[] extName = arr[0].Split('/');
                    int arrUsualLength = 6;
                    bool dynamic = arr.GetLength(0) >= arrUsualLength ? (arr[2] == "D" ? true : false) : false;
                    string statusText = arr.GetLength(0) >= arrUsualLength ? arr[5] : arr[4];
                    phones.Add(
                        new Phone()
                        {
                            extension = extName.GetLength(0) == 2 ? extName[0] : "",
                            name = extName.GetLength(0) == 2 ? extName[1] : extName[0],
                            host = arr[1],
                            dynamic = dynamic,
                            statusText = statusText,
                        }
                        );
                }
                
            }
            );
            return phones;
        }

    }

}
