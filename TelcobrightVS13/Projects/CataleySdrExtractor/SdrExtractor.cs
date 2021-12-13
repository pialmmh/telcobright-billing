using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using ProtoBuf;

namespace CataleySdrExtractor
{
    public class SdrExtractor
    {
        public List<PbSdrRecord> GetSdrs(string filename)
        {
            PbSdrBlock block;
            byte[] fileBytes = File.ReadAllBytes(filename);
            StringBuilder sb = new StringBuilder();

            foreach (byte b in fileBytes)
            {
                sb.Append(Convert.ToString(b, 2).PadLeft(8, '0'));
            }

            int length = IPAddress.NetworkToHostOrder((fileBytes[3] << 24) | (fileBytes[2] << 16) | (fileBytes[1] << 8) | fileBytes[0]);
            var newArr = fileBytes.Skip(4).Take(length).ToArray();
            using (var stream = new MemoryStream(newArr))
            {
                block = Serializer.Deserialize<PbSdrBlock>(stream);
            }

            return block.Sdrs;
        }
    }
}
