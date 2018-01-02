using System;
using System.Collections.Generic;
using System.IO;

namespace Chisel.Packages
{
    public interface IPackage : IPackageStreamSource
    {
        FileInfo PackageFile { get; }
        IEnumerable<IPackageEntry> GetEntries();
        IPackageEntry GetEntry(string path);
        byte[] ExtractEntry(IPackageEntry entry);
        Stream OpenStream(IPackageEntry entry);
        IPackageStreamSource GetStreamSource();
    }
}