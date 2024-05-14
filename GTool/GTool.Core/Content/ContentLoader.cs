using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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

namespace GTool.Content
{
    public class ContentLoader
    {
        private static readonly ContentLoader _instance = new ContentLoader();

        private List<Stream?> _streams = new List<Stream>();
        private Dictionary<string, FileData> _files = new Dictionary<string, FileData>();

        public const int Version = 1;

        internal void Init(Assembly assembly, string name)
        {
            _streams.Add(assembly.GetManifestResourceStream(name));
            Debug.Assert(_streams.Last() != null);

            using (BinaryReader br = new BinaryReader(_streams.Last(), Encoding.UTF8, true))
            {
                string header = Encoding.UTF8.GetString(br.ReadBytes(4));
                if (header != "BCF\0")
                    throw new InvalidDataException("Header is invalid!");
                ushort version = br.ReadUInt16();
                if (version != Version)
                    throw new InvalidDataException("Version is invalid!");

                ulong dataOffset = br.ReadUInt64();
                br.BaseStream.Seek((long)dataOffset, SeekOrigin.Begin);
                ReadDirectory(br, "");
            }
        }

        private void ReadDirectory(BinaryReader br, string path)
        {
            string name = br.ReadString();
            byte type = br.ReadByte();

            switch (type)
            {
                case 1:
                    {
                        ushort contained = br.ReadUInt16();
                        for (int i = 0; i < contained; i++)
                            ReadDirectory(br, $"{path}{name}/");
                    }
                    break;
                case 0:
                    {
                        ulong offset = br.ReadUInt64();
                        ulong size = br.ReadUInt64();

                        _files.Add($"{path}{name}", new FileData
                        {
                            Name = name,
                            StreamId = _streams.Count - 1,
                            Offset = offset,
                            Size = size
                        });
                    }
                    break;
                default:
                    throw new InvalidDataException("Unkown file type!");
            }
        }

        public static byte[] GetBytes(string path)
        {
            if (!_instance._files.TryGetValue(path, out FileData data))
            {
                byte[] bytes = new byte[data.Size];

                _instance._streams[data.StreamId]?.Seek((long)data.Offset, SeekOrigin.Begin);
                _instance._streams[data.StreamId]?.Read(bytes, 0, bytes.Length);

                return bytes;
            }

            throw new FileNotFoundException("Failed to find path!");
        }

        public static string GetString(string path) => Encoding.UTF8.GetString(GetBytes(path));

        public static void Initialize(string name) => _instance.Init(Assembly.GetEntryAssembly(), name);
        public static void Initialize(string name, Assembly assembly) => _instance.Init(assembly, name);

        internal static void Dispose()
        {
            for (int i = 0; i < _instance._streams.Count; i++)
                _instance._streams[i]?.Dispose();
            _instance._streams.Clear();
        }

        private struct FileData
        {
            public string Name;
            public int StreamId;

            public ulong Offset;
            public ulong Size;
        }
    }
}
