using System.IO;
using System.IO.Compression;

namespace Insight.WCF
{
    public enum CompressType
    {
        Gzip,
        Deflate,
        None
    }

    public class Util
    {
        /// <summary>
        /// GZip/Deflate压缩
        /// </summary>
        /// <param name="data">输入字节数组</param>
        /// <param name="model">压缩模式，默认Gzip</param>
        /// <returns>byte[] 压缩后的字节数组</returns>
        public static byte[] Compress(byte[] data, CompressType model = CompressType.Gzip)
        {
            using (var ms = new MemoryStream())
            {
                switch (model)
                {
                    case CompressType.Gzip:
                        using (var stream = new GZipStream(ms, CompressionMode.Compress, true))
                        {
                            stream.Write(data, 0, data.Length);
                        }
                        break;
                    case CompressType.Deflate:
                        using (var stream = new DeflateStream(ms, CompressionMode.Compress, true))
                        {
                            stream.Write(data, 0, data.Length);
                        }
                        break;
                    case CompressType.None:
                        return data;
                    default:
                        return data;
                }
                return ms.ToArray();
            }
        }

    }
}
