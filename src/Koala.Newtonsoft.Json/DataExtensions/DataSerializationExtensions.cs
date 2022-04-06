using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Newtonsoft.Json.DataExtensions
{
    public static class DataSerializationExtensions
    {
        public static Stream Base64EncodeStream(this string plainText)
        {
            return new MemoryStream(plainText.Base64EncodeBytes());
        }

        public static byte[] Base64EncodeBytes(this string plainText)
        {
            var base64EncodedString = plainText.Base64Encode();
            return Encoding.UTF8.GetBytes(base64EncodedString);
        }

        public static string Base64Encode(this string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Base64Encode(plainTextBytes);
        }

        public static string Base64Encode(this byte[] bytes)
        {
            return Convert.ToBase64String(bytes);
        }

        public static string Base64Decode(this Stream base64EncodedStream)
        {
            var streamReader = new StreamReader(base64EncodedStream);
            var base64EncodedString = streamReader.ReadToEnd();
            return base64EncodedString.Base64Decode();
        }

        public static string Base64Decode(this string base64EncodedString)
        {
            var plainTextBytes = Convert.FromBase64String(base64EncodedString);
            var plainText = Encoding.UTF8.GetString(plainTextBytes);
            return plainText;
        }

        public static byte[] Zip(this string plainText)
        {
            var bytes = Encoding.UTF8.GetBytes(plainText);

            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    CopyTo(msi, gs); //msi.CopyTo(gs);
                }

                return mso.ToArray();
            }
        }

        public static string Unzip(this byte[] zippedBytes)
        {
            using (var msi = new MemoryStream(zippedBytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    CopyTo(gs, mso); //gs.CopyTo(mso);
                }

                return Encoding.UTF8.GetString(mso.ToArray());
            }
        }

        public static void CopyTo(this Stream source, Stream destination)
        {
            byte[] bytes = new byte[4096];

            int cnt;

            while ((cnt = source.Read(bytes, 0, bytes.Length)) != 0)
            {
                destination.Write(bytes, 0, cnt);
            }
        }
    }
}