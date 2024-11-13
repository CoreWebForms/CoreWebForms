// MIT License.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

#nullable disable

namespace System.Web.Util;
/*
 * Class used to combine several hashcodes into a single hashcode
 */
internal class HashCodeCombiner
{
    internal HashCodeCombiner()
    {
        // Start with a seed (obtained from String.GetHashCode implementation)
        CombinedHash = 5381;
    }

    internal HashCodeCombiner(long initialCombinedHash)
    {
        CombinedHash = initialCombinedHash;
    }

    internal static int CombineHashCodes(int h1, int h2)
    {
        return ((h1 << 5) + h1) ^ h2;
    }

    internal static int CombineHashCodes(int h1, int h2, int h3)
    {
        return CombineHashCodes(CombineHashCodes(h1, h2), h3);
    }

    internal static int CombineHashCodes(int h1, int h2, int h3, int h4)
    {
        return CombineHashCodes(CombineHashCodes(h1, h2), CombineHashCodes(h3, h4));
    }

    internal static int CombineHashCodes(int h1, int h2, int h3, int h4, int h5)
    {
        return CombineHashCodes(CombineHashCodes(h1, h2, h3, h4), h5);
    }

    internal static string GetDirectoryHash(VirtualPath virtualDir)
    {
        HashCodeCombiner hashCodeCombiner = new HashCodeCombiner();
        hashCodeCombiner.AddDirectory(virtualDir.MapPath());
        return hashCodeCombiner.CombinedHashString;
    }

    internal void AddArray(string[] a)
    {
        if (a != null)
        {
            int n = a.Length;
            for (int i = 0; i < n; i++)
            {
                AddObject(a[i]);
            }
        }
    }

    internal void AddInt(int n)
    {
        CombinedHash = ((CombinedHash << 5) + CombinedHash) ^ n;
    }

    internal void AddObject(int n)
    {
        AddInt(n);
    }

    internal void AddObject(byte b)
    {
        AddInt(b.GetHashCode());
    }

    internal void AddObject(long l)
    {
        AddInt(l.GetHashCode());
    }

    internal void AddObject(bool b)
    {
        AddInt(b.GetHashCode());
    }

    internal void AddObject(string s)
    {
        if (s != null)
        {
            AddInt(StringUtil.GetStringHashCode(s));
        }
    }

    internal void AddObject(Type t)
    {
        if (t != null)
        {
            AddObject(t.Assembly.FullName);
            AddObject(t.FullName);
        }
    }

    internal void AddObject(object o)
    {
        if (o != null)
        {
            AddInt(o.GetHashCode());
        }
    }

    internal void AddCaseInsensitiveString(string s)
    {
        if (s != null)
        {
            AddInt(StringUtil.GetNonRandomizedHashCode(s, ignoreCase: true));
        }
    }

    internal void AddDateTime(DateTime dt)
    {
        AddInt(dt.GetHashCode());
    }

    private void AddFileSize(long fileSize)
    {
        AddInt(fileSize.GetHashCode());
    }

    [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "This call site is trusted.")]
    private void AddFileVersionInfo(FileVersionInfo fileVersionInfo)
    {
        AddInt(fileVersionInfo.FileMajorPart.GetHashCode());
        AddInt(fileVersionInfo.FileMinorPart.GetHashCode());
        AddInt(fileVersionInfo.FileBuildPart.GetHashCode());
        AddInt(fileVersionInfo.FilePrivatePart.GetHashCode());
    }

    private void AddFileContentHashKey(string fileContentHashKey)
    {
        AddInt(StringUtil.GetNonRandomizedHashCode(fileContentHashKey));
    }

    internal void AddFileContentHash(string fileName)
    {
        // Convert file content to hash bytes
        byte[] fileContentBytes = File.ReadAllBytes(fileName);
        byte[] fileContentHashBytes = SHA256.HashData(fileContentBytes);

        // Convert byte[] to hex string representation
        StringBuilder sbFileContentHashBytes = new StringBuilder();
        for (int index = 0; index < fileContentHashBytes.Length; index++)
        {
            sbFileContentHashBytes.Append(fileContentHashBytes[index].ToString("X2", CultureInfo.InvariantCulture));
        }

        AddFileContentHashKey(sbFileContentHashBytes.ToString());
    }

    internal void AddFile(string fileName)
    {
        if (!File.Exists(fileName))
        {
            // Review: Should we change the dependency model to take directory into account?
            if (Directory.Exists(fileName))
            {
                // Add as a directory dependency if it's a directory.
                AddDirectory(fileName);
                return;
            }

            return;
        }

        AddExistingFile(fileName);
    }

    // Same as AddFile, but only called for a file which is known to exist
    private void AddExistingFile(string fileName)
    {
        AddInt(StringUtil.GetStringHashCode(fileName));
        FileInfo file = new FileInfo(fileName);
        AddDateTime(file.CreationTimeUtc);
        AddDateTime(file.LastWriteTimeUtc);
        AddFileSize(file.Length);
    }

    internal void AddExistingFileVersion(string fileName)
    {
        AddInt(StringUtil.GetStringHashCode(fileName));
        FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(fileName);

        AddFileVersionInfo(fileVersionInfo);
    }

    internal void AddDirectory(string directoryName)
    {
        DirectoryInfo directory = new DirectoryInfo(directoryName);
        if (!directory.Exists)
        {
            return;
        }

        AddObject(directoryName);

        // Go through all the files in the directory
        foreach (var fileData in directory.GetFileSystemInfos(directoryName, SearchOption.TopDirectoryOnly))
        {
            if (fileData.Attributes.HasFlag(FileAttributes.Directory))
            {
                AddDirectory(fileData.FullName);
            }
            else
            {
                AddExistingFile(fileData.FullName);
            }
        }

        AddDateTime(directory.CreationTimeUtc);
        AddDateTime(directory.LastWriteTimeUtc);
    }

    // Same as AddDirectory, but only look at files that don't have a culture
    internal void AddResourcesDirectory(string directoryName)
    {
        DirectoryInfo directory = new DirectoryInfo(directoryName);
        if (!directory.Exists)
        {
            return;
        }

        AddObject(directoryName);

        foreach (var fileData in directory.GetFileSystemInfos(directoryName, SearchOption.TopDirectoryOnly))
        {
            if (fileData.Attributes.HasFlag(FileAttributes.Directory))
            {
                AddResourcesDirectory(fileData.FullName);
            }
            else
            {
            }
        }
    }

    internal long CombinedHash { get; private set; }

    internal int CombinedHash32 { get { return CombinedHash.GetHashCode(); } }

    internal string CombinedHashString
    {
        get
        {
            return CombinedHash.ToString("x", CultureInfo.InvariantCulture);
        }
    }
}
