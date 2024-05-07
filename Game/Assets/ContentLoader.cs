using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Assets
{
    internal class ContentLoader : IDisposable
    {
        private static ContentLoader _instance;

        private Dictionary<string, ContentFile> _contentFiles = new Dictionary<string, ContentFile>();
        private FileStream _contentStream;

        public ContentLoader()
        {
            _instance = this;

            if (!File.Exists("res/Content.bcf"))
            {
                Log.Fatal("Failed to find content file: {@}", "res/Content.bcf");
                Environment.Exit(2);
            }

            _contentStream = File.OpenRead("res/Content.bcf");
            using (BinaryReader br = new BinaryReader(_contentStream, Encoding.UTF8, true))
            {
                byte[] header = br.ReadBytes(4);
                if (header[0] != 66 || header[1] != 67 || header[2] != 70 && header[3] != 0)
                {
                    Log.Fatal("Error in header!");
                    Environment.Exit(3);
                }

                ushort version = br.ReadUInt16();
                if (version != 1)
                {
                    Log.Fatal("Error in version!");
                    Environment.Exit(3);
                }

                int fileCount = br.ReadInt32();
                for (int i = 0; i < fileCount; i++)
                {
                    ContentFile file = new ContentFile();
                    file.Name = br.ReadString();
                    file.Offset = br.ReadInt64();
                    file.Size = br.ReadInt64();
                    _contentFiles.Add(file.Name, file);
                }
            }
        }

        public void Dispose()
        {
            _contentStream.Dispose();
        }

        public byte[] Read(string name)
        {
            if (_contentFiles.TryGetValue(name, out ContentFile file))
            {
                byte[] buffer = new byte[file.Size];
                _contentStream.Position = file.Offset;
                _contentStream.Read(buffer, 0, buffer.Length);
                return buffer;
            }
            
            Log.Error("Failed to find content with name: {@Name}", name);
            return Array.Empty<byte>();
        }
        public static byte[] ReadBytes(string name) => _instance.Read(name);

        private struct ContentFile
        {
            public string Name;
            public long Offset;
            public long Size;
        }
    }
}
