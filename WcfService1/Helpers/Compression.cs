using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Web;

namespace WS_Posdata_PMKT_Fotos.Helpers
{
    public class Compression
    {
        public  byte[] Compress(byte[] data)

        {

            MemoryStream output = new MemoryStream();

            using (DeflateStream dstream =
            new DeflateStream(output, CompressionLevel.Optimal))

            {

                dstream.Write(data, 0, data.Length);

            }

            return output.ToArray();

        }


        public byte[] Decompress(byte[] data)

        {

            MemoryStream input = new MemoryStream(data);

            MemoryStream output = new MemoryStream();

            using (DeflateStream dstream =
            new DeflateStream(input, CompressionMode.Decompress))

            {

                dstream.CopyTo(output);

            }

            return output.ToArray();

        }


    }
}