using System.ComponentModel;
using System.Runtime.CompilerServices;
using IcotakuScrapper.Anime;

namespace IcotakuScrapper.Objects.Models;


public readonly struct AnimeSelectionModeSelector
{
    public AnimeSelectionMode Kind { get; init; }
    public string Name { get; init; } = null!;

    public AnimeSelectionModeSelector(AnimeSelectionMode kind, string name)
    {
        Kind = kind;
        Name = name;
    }

    public AnimeSelectionModeSelector()
    {

    }
}

public class AnimeSelectionModeSelectorVm : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged = delegate { };
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

    internal AnimeSelectionModeSelectorVm(AnimeSelectionMode kind, string name)
    {
        Kind = kind;
        Name = name;
    }

    internal AnimeSelectionModeSelectorVm()
    {
    }

    public static IEnumerable<AnimeSelectionModeSelectorVm> GetFilterByList()
    {
        var enumValues = Enum.GetValues<AnimeSelectionMode>().ToArray();
        foreach (var enumValue in enumValues)
        {
            yield return enumValue switch
            {
                AnimeSelectionMode.None => new AnimeSelectionModeSelectorVm(AnimeSelectionMode.None, "Aucun"),
                AnimeSelectionMode.Letter => new AnimeSelectionModeSelectorVm(AnimeSelectionMode.Letter, "Lettre"),
                AnimeSelectionMode.OrigineAdaptation => new AnimeSelectionModeSelectorVm(AnimeSelectionMode.OrigineAdaptation, "Origine de l'adaptation"),
                AnimeSelectionMode.GroupName => new AnimeSelectionModeSelectorVm(AnimeSelectionMode.GroupName, "Format"),
                AnimeSelectionMode.Season => new AnimeSelectionModeSelectorVm(AnimeSelectionMode.Season, "Saison"),
                AnimeSelectionMode.ReleaseMonth => new AnimeSelectionModeSelectorVm(AnimeSelectionMode.ReleaseMonth, "Date de diffusion"),
                AnimeSelectionMode.Category => new AnimeSelectionModeSelectorVm(AnimeSelectionMode.Category, "Catégorie"),
                _ => throw new NotImplementedException(),
            };
        }
    }

    public static IEnumerable<AnimeSelectionModeSelector> GetFilterStructByList()
    {
        var enumValues = Enum.GetValues<AnimeSelectionMode>().ToArray();
        foreach (var enumValue in enumValues)
        {
            yield return enumValue switch
            {
                AnimeSelectionMode.None => new AnimeSelectionModeSelector(AnimeSelectionMode.None, "Aucun"),
                AnimeSelectionMode.Letter => new AnimeSelectionModeSelector(AnimeSelectionMode.Letter, "Lettre"),
                AnimeSelectionMode.OrigineAdaptation => new AnimeSelectionModeSelector(AnimeSelectionMode.OrigineAdaptation, "Origine de l'adaptation"),
                AnimeSelectionMode.GroupName => new AnimeSelectionModeSelector(AnimeSelectionMode.GroupName, "Format"),
                AnimeSelectionMode.Season => new AnimeSelectionModeSelector(AnimeSelectionMode.Season, "Saison"),
                AnimeSelectionMode.ReleaseMonth => new AnimeSelectionModeSelector(AnimeSelectionMode.ReleaseMonth, "Date de diffusion"),
                AnimeSelectionMode.Category => new AnimeSelectionModeSelector(AnimeSelectionMode.Category, "Catégorie"),
                _ => throw new NotImplementedException(),
            };
        }
    }

    public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}