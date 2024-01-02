using IcotakuScrapper.Extensions;

namespace IcotakuScrapper.Objects;

/// <summary>
/// Structure contenant les paramètres de recherche avancée d'anime
/// </summary>
public readonly struct AnimeFinderParameterStruct
{
    public AnimeFinderParameterStruct()
    {
        
    }

    /// <summary>
    /// Titre partiel ou complet de l'anime
    /// </summary>
    public string? Title { get; init; }
    
    /// <summary>
    /// Format de l'anime
    /// </summary>
    public string? Format { get; init; }
    
    /// <summary>
    /// Public visé de l'anime
    /// </summary>
    public string? Target { get; init; }
    
    /// <summary>
    /// Origine de l'adaptation de l'anime
    /// </summary>
    public string? OrigineAdaptation { get; init; }
    
    /// <summary>
    /// Id du studio d'animation de l'anime
    /// </summary>
    public int? StudioId { get; init; }
    
    /// <summary>
    /// Etat de la diffusion de l'anime
    /// </summary>
    public DiffusionStateKind DiffusionState { get; init; }
    
    /// <summary>
    /// Id du distributeur de l'anime
    /// </summary>
    public int? DistributorId { get; init; }
    
    /// <summary>
    /// Année de diffusion de l'anime
    /// </summary>
    public ushort Year { get; init; }
    
    /// <summary>
    /// Mois de diffusion de l'anime
    /// </summary>
    public byte MonthNumber { get; init; }
    
    /// <summary>
    /// Saision de diffusion de l'anime
    /// </summary>
    public WeatherSeasonKind Season { get; init; }
    
    /// <summary>
    /// Genres que l'anime doit contenir
    /// </summary>
    public int[] IncludeGenresId { get; init; } = [];
    
    /// <summary>
    /// Genres que l'anime ne doit pas contenir
    /// </summary>
    public int[] ExcludeGenresId { get; init; } = [];
    
    /// <summary>
    /// Thèmes que l'anime doit contenir
    /// </summary>
    public int[] IncludeThemesId { get; init; } = [];
    
    /// <summary>
    /// Thèmes que l'anime ne doit pas contenir
    /// </summary>
    public int[] ExcludeThemesId { get; init; } = [];
    
    public bool HasTitle => Title != null && !Title.IsStringNullOrEmptyOrWhiteSpace();
}