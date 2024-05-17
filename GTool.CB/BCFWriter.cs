using K4os.Compression.LZ4;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*

-- Header --
HeaderText - byte[4] - 4 bytes
Version - ushort - 2 bytes
DataOffset - ulong - 8 bytes
Content - FileData[] - ?? bytes
ContentDir - Directory - ?? bytes
External - Directory - ?? bytes --unused

-- FileInfo --
Name - string - ?? bytes
Type - byte - 1 bytes

-- Directory : FileInfo --
Files - ushort - 2 bytes
FilesWithin - FileInfo[] -- ?? bytes

-- File : FileInfo --
Offset - ulong - 8 bytes
Length - ulong - 8 bytes

-- FileData --
Compressed - bool - 1 bytes
OriginalSize - int - 4 bytes
Data - byte[] - ?? bytes

*/

namespace GTool.CB
{
    internal static class BCFWriter
    {
        public const ushort Version = 1;

        public static void WriteBCF(Stream stream, TreeNode content, TreeNode external)
        {
            LZ4Level codecLevel = LZ4Level.L00_FAST;
            switch (FilePostProcess.Quality)
            {
                case FilePostProcess.ProcessQuality.Medium:
                    codecLevel = LZ4Level.L04_HC;
                    break;
                case FilePostProcess.ProcessQuality.High:
                    codecLevel = LZ4Level.L07_HC;
                    break;
                case FilePostProcess.ProcessQuality.Max:
                    codecLevel = LZ4Level.L12_MAX;
                    break;
                default:
                    break;
            }

            using (BinaryWriter bw = new BinaryWriter(stream, Encoding.UTF8, true))
            {
                List<FileData> fileDatas = new List<FileData>();
                Dictionary<string, long> fileSizes = new Dictionary<string, long>();

                //write header
                bw.Write(Encoding.UTF8.GetBytes("BCF\0"));
                bw.Write(Version);
                bw.Write(ulong.MaxValue);

                ulong recordedOffset = 0; //unused for now
                WriteDirectory(bw, fileDatas, fileSizes, ref recordedOffset, content, false);
                //WriteDirectory(bw, fileDatas, fileSizes, ref recordedOffset, external, false);

                foreach (FileData fileData in fileDatas)
                {
                    if (fileData.IsDirectory || fileData.IsFake)
                        continue;

                    string path = fileData.PPFPath == null ? fileData.FullPath : fileData.PPFPath;

                    byte[] source = File.ReadAllBytes(path);
                    int maxSize = LZ4Codec.MaximumOutputSize(source.Length) - (fileData.PPFPath == null ? 0 : sizeof(long) + 1);
                    byte[] compressed = new byte[maxSize];
                    int actualSize = LZ4Codec.Encode(source, fileData.PPFPath == null ? 0 : sizeof(long) + 1, source.Length - (fileData.PPFPath == null ? 0 : sizeof(long) + 1), compressed, 0, maxSize, codecLevel);

                    if (actualSize >= source.Length || actualSize == -1 /*failed*/) //if so then compression is obviously not worth it!
                    {
                        bw.Write(false);
                        //bw.Write(source.Length); //unused when not compressed
                        bw.Write(source);
                        fileSizes.Add(fileData.FullPath, source.LongLength - (fileData.PPFPath == null ? 0 : sizeof(long) + 1));
                    }
                    else
                    {
                        bw.Write(true);
                        bw.Write(source.Length);
                        bw.Write(compressed, 0, actualSize);
                        fileSizes.Add(fileData.FullPath, actualSize);
                    }
                }

                recordedOffset = (ulong)stream.Position;
                bw.Seek(6, SeekOrigin.Begin);
                bw.Write(recordedOffset);

                bw.Seek(0, SeekOrigin.End);
                recordedOffset = 14; //actually used now
                WriteDirectory(bw, fileDatas, fileSizes, ref recordedOffset, content);
                //WriteDirectory(bw, fileDatas, fileSizes, ref recordedOffset, external);
            }
        }

        private static void WriteDirectory(BinaryWriter bw, List<FileData> fileDatas, Dictionary<string, long> fileSizes, ref ulong offset, TreeNode content, bool write = true)
        {
            FileData info = (FileData)content.Tag;

            if (write)
            {
                bw.Write(info.Name);
                bw.Write((byte)(info.IsDirectory ? 1 : 0));
            }
            else
                fileDatas.Add(info);

            if (info.IsDirectory)
            {
                if (write)
                {
                    bw.Write((ushort)content.Nodes.Count);
                }
                foreach (TreeNode node in content.Nodes)
                    WriteDirectory(bw, fileDatas, fileSizes, ref offset, node, write);
            }
            else
            {
                if (write)
                {
                    ulong size = (ulong)(fileSizes[info.FullPath] + 5L);
                    bw.Write(offset);
                    bw.Write(size);
                    offset += size;
                }
            }
        }
    }
}
