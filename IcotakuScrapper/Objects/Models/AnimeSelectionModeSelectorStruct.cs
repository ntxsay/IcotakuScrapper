using System.ComponentModel;
using System.Runtime.CompilerServices;
using IcotakuScrapper.Anime;

namespace IcotakuScrapper.Objects.Models;


public readonly struct AnimeSelectionModeSelectorStruct
{
    public AnimeSelectionMode Kind { get; init; }
    public string Name { get; init; } = null!;

    public AnimeSelectionModeSelectorStruct(AnimeSelectionMode kind, string name)
    {
        Kind = kind;
        Name = name;
    }

    public AnimeSelectionModeSelectorStruct()
    {

    }
}

public class AnimeSelectionModeSelector : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged = delegate { };
    private AnimeSelectionMode _Kind;

    public AnimeSelectionMode Kind
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

    internal AnimeSelectionModeSelector(AnimeSelectionMode kind, string name)
    {
        Kind = kind;
        Name = name;
    }

    internal AnimeSelectionModeSelector()
    {
    }

    public static IEnumerable<AnimeSelectionModeSelector> GetFilterByList()
    {
        var enumValues = Enum.GetValues<AnimeSelectionMode>().ToArray();
        foreach (var enumValue in enumValues)
        {
            yield return enumValue switch
            {
                AnimeSelectionMode.None => new AnimeSelectionModeSelector(AnimeSelectionMode.None, "Aucun"),
                AnimeSelectionMode.Letter => new AnimeSelectionModeSelector(AnimeSelectionMode.Letter, "Lettre"),
                AnimeSelectionMode.OrigineAdaptation => new AnimeSelectionModeSelector(AnimeSelectionMode.OrigineAdaptation, "Origine de l'adaptation"),
                AnimeSelectionMode.Format => new AnimeSelectionModeSelector(AnimeSelectionMode.Format, "Format"),
                AnimeSelectionMode.Season => new AnimeSelectionModeSelector(AnimeSelectionMode.Season, "Saison"),
                AnimeSelectionMode.ReleaseMonth => new AnimeSelectionModeSelector(AnimeSelectionMode.ReleaseMonth, "Date de diffusion"),
                AnimeSelectionMode.Category => new AnimeSelectionModeSelector(AnimeSelectionMode.Category, "Catégorie"),
                _ => throw new NotImplementedException(),
            };
        }
    }

    public static IEnumerable<AnimeSelectionModeSelectorStruct> GetFilterStructByList()
    {
        var enumValues = Enum.GetValues<AnimeSelectionMode>().ToArray();
        foreach (var enumValue in enumValues)
        {
            yield return enumValue switch
            {
                AnimeSelectionMode.None => new AnimeSelectionModeSelectorStruct(AnimeSelectionMode.None, "Aucun"),
                AnimeSelectionMode.Letter => new AnimeSelectionModeSelectorStruct(AnimeSelectionMode.Letter, "Lettre"),
                AnimeSelectionMode.OrigineAdaptation => new AnimeSelectionModeSelectorStruct(AnimeSelectionMode.OrigineAdaptation, "Origine de l'adaptation"),
                AnimeSelectionMode.Format => new AnimeSelectionModeSelectorStruct(AnimeSelectionMode.Format, "Format"),
                AnimeSelectionMode.Season => new AnimeSelectionModeSelectorStruct(AnimeSelectionMode.Season, "Saison"),
                AnimeSelectionMode.ReleaseMonth => new AnimeSelectionModeSelectorStruct(AnimeSelectionMode.ReleaseMonth, "Date de diffusion"),
                AnimeSelectionMode.Category => new AnimeSelectionModeSelectorStruct(AnimeSelectionMode.Category, "Catégorie"),
                _ => throw new NotImplementedException(),
            };
        }
    }

    public virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}