using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FiveMCacheCleaner;

public sealed class CleanItem : INotifyPropertyChanged
{
    private bool _isSelected;
    private string _info = "—";

    public CleanItem(string group, string icon, string name, string description, string path, bool isSelected)
    {
        Group = group;
        Icon = icon;
        Name = name;
        Description = description;
        Path = path;
        _isSelected = isSelected;
    }

    public string Group { get; }
    public string Icon { get; }
    public string Name { get; }
    public string Description { get; }
    public string Path { get; set; }

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
