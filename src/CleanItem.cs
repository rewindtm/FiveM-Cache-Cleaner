using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FiveMCacheCleaner;

public sealed class CleanItem : INotifyPropertyChanged
{
    private bool _isSelected;
    private string _info = "—";

    public CleanItem(string name, string description, string path, bool isSelected, bool isSystem)
    {
        Name = name;
        Description = description;
        Path = path;
        _isSelected = isSelected;
        IsSystem = isSystem;
    }

    public string Name { get; }
    public string Description { get; }
    public string Path { get; set; }
    public bool IsSystem { get; }

    public long Files { get; set; }
    public long Bytes { get; set; }

    public bool IsSelected
    {
        get => _isSelected;
        set { _isSelected = value; OnChanged(); }
    }

    public string Info
    {
        get => _info;
        set { _info = value; OnChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnChanged([CallerMemberName] string? n = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}
