using System.IO;
using System.IO.Compression;

namespace Spider.Exports
{
    public static class Unzipper
    {
        public static string Unzip(string gzipFilePath)
        {
            if (!File.Exists(gzipFilePath))
            {
                throw new FileNotFoundException($"Archive file was not found at \"{gzipFilePath}\"");
            }

            string decompressedFilePath;

            var fileToDecompress = new FileInfo(gzipFilePath);
            using (var originalFileStream = fileToDecompress.OpenRead())
            {
                string currentFileName = fileToDecompress.FullName;
                decompressedFilePath = currentFileName.Remove(currentFileName.Length - fileToDecompress.Extension.Length);

                using (var decompressedFileStream = File.Create(decompressedFilePath))
                {
                    using (var decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
                    {
                        decompressionStream.CopyTo(decompressedFileStream);
                    }
                }
            }

            return decompressedFilePath;
        }
    }
}