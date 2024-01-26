using System.ComponentModel;
using System.Runtime.CompilerServices;
using IcotakuScrapper.Anime;

namespace IcotakuScrapper.Objects.Models;

public readonly struct AnimeSortBySelectorStruct
{
    public AnimeSortBy Kind { get; init; }
    public string Name { get; init; } = null!;

    public AnimeSortBySelectorStruct(AnimeSortBy kind, string name)
    {
        Kind = kind;
        Name = name;
    }

    public AnimeSortBySelectorStruct()
    {

    }
}

public class AnimeSortBySelector : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged = delegate { };
    private AnimeSortBy _Kind;

    public AnimeSortBy Kind
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

    internal AnimeSortBySelector(AnimeSortBy kind, string name)
    {
        Kind = kind;
        Name = name;
    }

    internal AnimeSortBySelector()
    {
    }

    public static IEnumerable<AnimeSortBySelector> GetFilterByList()
    {
        var enumValues = Enum.GetValues<AnimeSortBy>().ToArray();
        foreach (var enumValue in enumValues)
        {
            yield return enumValue switch
            {
                AnimeSortBy.Name => new AnimeSortBySelector(enumValue, "Nom  de l'animé"),
                AnimeSortBy.SheetId => new AnimeSortBySelector(enumValue, "Numéro de fiche"),
                AnimeSortBy.OrigineAdaptation => new AnimeSortBySelector(enumValue, "Origine de l'adaptation"),
                AnimeSortBy.Format => new AnimeSortBySelector(enumValue, "Format"),
                AnimeSortBy.Season => new AnimeSortBySelector(enumValue, "Saison"),
                AnimeSortBy.ReleaseMonth => new AnimeSortBySelector(enumValue, "Date de diffusion"),
                AnimeSortBy.Id => new AnimeSortBySelector(enumValue, "Identifiant"),
                AnimeSortBy.EpisodesCount => new AnimeSortBySelector(enumValue, "Nombre d'épisodes"),
                AnimeSortBy.EndDate => new AnimeSortBySelector(enumValue, "Date de fin"),
                AnimeSortBy.Duration => new AnimeSortBySelector(enumValue, "Durée"),
                AnimeSortBy.Target => new AnimeSortBySelector(enumValue, "Public visé"),
                _ => throw new NotImplementedException(),
            };
        }
    }

    public static IEnumerable<AnimeSortBySelectorStruct> GetFilterStructByList()
    {
        var enumValues = Enum.GetValues<AnimeSortBy>().ToArray();
        foreach (var enumValue in enumValues)
        {
            yield return enumValue switch
            {
                AnimeSortBy.Name => new AnimeSortBySelectorStruct(enumValue, "Nom  de l'animé"),
                AnimeSortBy.SheetId => new AnimeSortBySelectorStruct(enumValue, "Numéro de fiche"),
                AnimeSortBy.OrigineAdaptation => new AnimeSortBySelectorStruct(enumValue, "Origine de l'adaptation"),
                AnimeSortBy.Format => new AnimeSortBySelectorStruct(enumValue, "Format"),
                AnimeSortBy.Season => new AnimeSortBySelectorStruct(enumValue, "Saison"),
                AnimeSortBy.ReleaseMonth => new AnimeSortBySelectorStruct(enumValue, "Date de diffusion"),
                AnimeSortBy.Id => new AnimeSortBySelectorStruct(enumValue, "Identifiant"),
                AnimeSortBy.EpisodesCount => new AnimeSortBySelectorStruct(enumValue, "Nombre d'épisodes"),
                AnimeSortBy.EndDate => new AnimeSortBySelectorStruct(enumValue, "Date de fin"),
                AnimeSortBy.Duration => new AnimeSortBySelectorStruct(enumValue, "Durée"),
                AnimeSortBy.Target => new AnimeSortBySelectorStruct(enumValue, "Public visé"),
                _ => throw new NotImplementedException(),
            };
        }
    }

    public virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}