using System.IO;

namespace Chisel.Packages.Wad
{
    public class WadEntry : IPackageEntry
    {
        public WadPackage Package { get; private set; }

        public uint Offset { get; private set; }
        public uint CompressedLength { get; private set; }
        public long Length { get; private set; }

        public WadEntryType Type { get; private set; }
        public byte CompressionType { get; private set; }
        public string Name { get; private set; }

        public long TextureDataOffset { get; internal set; }
        public long PaletteDataOffset { get; internal set; }
        public uint Width { get; internal set; }
        public uint Height { get; internal set; }
        public uint PaletteSize { get; internal set; }

        public string FullName { get { return Name; } }
        public string ParentPath { get { return ""; } }

        public WadEntry(WadPackage package, string name, WadEntryType type, uint offset, byte compressionType, uint compressedLength, uint fullLength)
        {
            Package = package;
            Name = name;
            Offset = offset;
            CompressionType = compressionType;
            CompressedLength = compressedLength;
            Length = fullLength;
            Type = type;
        }

        public Stream Open()
        {
            return Package.OpenStream(this);
        }
    };
}