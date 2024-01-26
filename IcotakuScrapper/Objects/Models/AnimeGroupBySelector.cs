using System.ComponentModel;
using System.Runtime.CompilerServices;
using IcotakuScrapper.Anime;

namespace IcotakuScrapper.Objects.Models;


public readonly struct AnimeGroupBySelectorStruct
{
    public AnimeGroupBy Kind { get; init; }
    public string Name { get; init; } = null!;

    public AnimeGroupBySelectorStruct(AnimeGroupBy kind, string name)
    {
        Kind = kind;
        Name = name;
    }

    public AnimeGroupBySelectorStruct()
    {

    }
}

public class AnimeGroupBySelector : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged = delegate { };
    private AnimeGroupBy _Kind;

    public AnimeGroupBy Kind
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

    internal AnimeGroupBySelector(AnimeGroupBy kind, string name)
    {
        Kind = kind;
        Name = name;
    }

    internal AnimeGroupBySelector()
    {
    }

    public static IEnumerable<AnimeGroupBySelector> GetFilterByList()
    {
        var enumValues = Enum.GetValues<AnimeGroupBy>().ToArray();
        foreach (var enumValue in enumValues)
        {
            yield return enumValue switch
            {
                AnimeGroupBy.Default => new AnimeGroupBySelector(enumValue, "Défaut"),
                AnimeGroupBy.Letter => new AnimeGroupBySelector(enumValue, "Lettre"),
                AnimeGroupBy.OrigineAdaptation => new AnimeGroupBySelector(enumValue, "Origine de l'adaptation"),
                AnimeGroupBy.Format => new AnimeGroupBySelector(enumValue, "Format"),
                AnimeGroupBy.Season => new AnimeGroupBySelector(enumValue, "Saison"),
                AnimeGroupBy.ReleaseMonth => new AnimeGroupBySelector(enumValue, "Date de diffusion"),
                AnimeGroupBy.Categories => new AnimeGroupBySelector(enumValue, "Catégories"),
                AnimeGroupBy.Target => new AnimeGroupBySelector(enumValue, "Cible démographique"),
                _ => throw new ArgumentOutOfRangeException(nameof(enumValue), enumValue, null)
            };
        }

    }

    public static IEnumerable<AnimeGroupBySelectorStruct> GetFilterStructByList()
    {
        var enumValues = Enum.GetValues<AnimeGroupBy>().ToArray();
        foreach (var enumValue in enumValues)
        {
            yield return enumValue switch
            {
                AnimeGroupBy.Default => new AnimeGroupBySelectorStruct(enumValue, "Défaut"),
                AnimeGroupBy.Letter => new AnimeGroupBySelectorStruct(enumValue, "Lettre"),
                AnimeGroupBy.OrigineAdaptation => new AnimeGroupBySelectorStruct(enumValue,
                    "Origine de l'adaptation"),
                AnimeGroupBy.Format => new AnimeGroupBySelectorStruct(enumValue, "Format"),
                AnimeGroupBy.Season => new AnimeGroupBySelectorStruct(enumValue, "Saison"),
                AnimeGroupBy.ReleaseMonth => new AnimeGroupBySelectorStruct(enumValue, "Date de diffusion"),
                AnimeGroupBy.Categories => new AnimeGroupBySelectorStruct(enumValue, "Catégories"),
                AnimeGroupBy.Target => new AnimeGroupBySelectorStruct(enumValue, "Cible démographique"),
                _ => throw new ArgumentOutOfRangeException(nameof(enumValue), enumValue, null)
            };
        }
    }

    public virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}