using System.Globalization;
using System.IO;

namespace FiveMCacheCleaner;

public readonly record struct ScanResult(long Files, long Bytes, bool Exists);
public readonly record struct CleanResult(long Deleted, long Bytes, long Skipped, bool Exists);

public static class Cleaner
{
    private static readonly EnumerationOptions Opts = new()
    {
        RecurseSubdirectories = true,
        IgnoreInaccessible = true,
        AttributesToSkip = FileAttributes.ReparsePoint, // non seguire junction/symlink
        ReturnSpecialDirectories = false,
    };

    public static ScanResult Scan(string path)
    {
        if (!Directory.Exists(path)) return new(0, 0, false);
        long files = 0, bytes = 0;
        foreach (var f in new DirectoryInfo(path).EnumerateFiles("*", Opts))
        {
            try { bytes += f.Length; files++; } catch { }
        }
        return new(files, bytes, true);
    }

    /// <summary>Svuota la cartella senza eliminare la cartella stessa.</summary>
    public static CleanResult Clear(string path)
    {
        if (!Directory.Exists(path)) return new(0, 0, 0, false);
        long deleted = 0, bytes = 0, skipped = 0;

        foreach (var f in new DirectoryInfo(path).EnumerateFiles("*", Opts).ToList())
        {
            try
            {
                long len = f.Length;
                f.Attributes = FileAttributes.Normal;
                f.Delete();
                deleted++; bytes += len;
            }
            catch { skipped++; }
        }

        // sottocartelle, dalle più profonde
        var dirs = new DirectoryInfo(path).EnumerateDirectories("*", Opts)
                       .OrderByDescending(d => d.FullName.Length).ToList();
        foreach (var d in dirs)
        {
            try { d.Delete(false); } catch { /* non vuota o in uso */ }
        }
        return new(deleted, bytes, skipped, true);
    }

    public static string FormatSize(long bytes)
    {
        if (bytes >= 1073741824L)
            return string.Create(CultureInfo.CurrentCulture, $"{bytes / 1073741824.0:0.00} GB");
        return string.Create(CultureInfo.CurrentCulture, $"{bytes / 1048576.0:0.00} MB");
    }

    public static string? FindFiveMDir()
    {
        var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var candidates = new List<string> { Path.Combine(local, "FiveM", "FiveM.app") };
        foreach (var drive in DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.Fixed))
        {
            candidates.Add(Path.Combine(drive.Name, "FiveM", "FiveM.app"));
            candidates.Add(Path.Combine(drive.Name, "Games", "FiveM", "FiveM.app"));
        }
        return candidates.FirstOrDefault(Directory.Exists);
    }
}
