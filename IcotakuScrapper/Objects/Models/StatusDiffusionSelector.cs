using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace IcotakuScrapper.Objects.Models;



public readonly struct StatusDiffusionSelectorStruct
{
    public DiffusionStateKind Kind { get; init; }
    public string Name { get; init; } = null!;

    public StatusDiffusionSelectorStruct(DiffusionStateKind kind, string name)
    {
        Kind = kind;
        Name = name;
    }

    public StatusDiffusionSelectorStruct()
    {

    }
}

public class StatusDiffusionSelector : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged = delegate { };
    private DiffusionStateKind _Kind;

    public DiffusionStateKind Kind
    {
        get => _Kind;
        set
        {
            if (_Kind != value)
            {
                _Kind = value;
                OnPropertyChanged();
            }
        }
    }

    private string _Name = null!;

    public string Name
    {
        get => _Name;
        set
        {
            if (_Name != value)
            {
                _Name = value;
                OnPropertyChanged();
            }
        }
    }

    internal StatusDiffusionSelector(DiffusionStateKind kind, string name)
    {
        Kind = kind;
        Name = name;
    }

    internal StatusDiffusionSelector()
    {
    }

    public static IEnumerable<StatusDiffusionSelector> GetFilterByList()
    {
        var enumValues = Enum.GetValues<DiffusionStateKind>().ToArray();
        foreach (var enumValue in enumValues)
        {
            yield return enumValue switch
            {
                DiffusionStateKind.Unknown => new StatusDiffusionSelector(enumValue, "Inconnu"),
                DiffusionStateKind.UpComing => new StatusDiffusionSelector(enumValue, "Bientôt"),
                DiffusionStateKind.InProgress => new StatusDiffusionSelector(enumValue, "En cours"),
                DiffusionStateKind.Paused => new StatusDiffusionSelector(enumValue, "En Pause"),
                DiffusionStateKind.Stopped => new StatusDiffusionSelector(enumValue, "Annulé"),
                DiffusionStateKind.Completed => new StatusDiffusionSelector(enumValue, "Terminé"),
                _ => throw new NotImplementedException(),
            };
        }
    }

    public static IEnumerable<StatusDiffusionSelectorStruct> GetFilterStructByList()
    {
        var enumValues = Enum.GetValues<DiffusionStateKind>().ToArray();
        foreach (var enumValue in enumValues)
        {
            yield return enumValue switch
            {
                DiffusionStateKind.Unknown => new StatusDiffusionSelectorStruct(enumValue, "Inconnu"),
                DiffusionStateKind.UpComing => new StatusDiffusionSelectorStruct(enumValue, "Bientôt"),
                DiffusionStateKind.InProgress => new StatusDiffusionSelectorStruct(enumValue, "En cours"),
                DiffusionStateKind.Paused => new StatusDiffusionSelectorStruct(enumValue, "En Pause"),
                DiffusionStateKind.Stopped => new StatusDiffusionSelectorStruct(enumValue, "Annulé"),
                DiffusionStateKind.Completed => new StatusDiffusionSelectorStruct(enumValue, "Terminé"),
                _ => throw new NotImplementedException(),
            };
        }
    }

    public virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

