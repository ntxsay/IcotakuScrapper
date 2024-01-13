

namespace IcotakuScrapper.Objects;

/// <summary>
/// Représente une saison météorologique.
/// </summary>
public readonly struct WeatherSeason
{
    /// <summary>
    /// Obtient la saison météorologique.
    /// </summary>
    public WeatherSeasonKind Season { get; }
    
    /// <summary>
    /// Obtient l'année de la saison météorologique.
    /// </summary>
    public uint Year { get; }
    
    public WeatherSeason(WeatherSeasonKind season, uint year)
    {   
        if (year < DateOnly.MinValue.Year || year > DateOnly.MaxValue.Year)
            throw new ArgumentOutOfRangeException(nameof(year), year, $"L'année doit être comprise entre {DateOnly.MinValue.Year} et {DateOnly.MaxValue.Year}.");
        
        if (season == WeatherSeasonKind.Unknown)
            throw new ArgumentOutOfRangeException(nameof(season), season, "La saison ne peut pas être inconnue.");
        
        Season = season;
        Year = year;
    }
    
    /// <summary>
    /// Retourne une représentation numérique de la saison météorologique, exemple : 201004.
    /// </summary>
    /// <returns></returns>
    public uint ToIntSeason()
        => SeasonHelpers.GetIntSeason(Season, Year);

    /// <summary>
    /// Retourne une représentation textuelle de la saison météorologique, exemple : "Automne 2010".
    /// </summary>
    /// <returns></returns>
    public override string ToString()
        => SeasonHelpers.GetSeasonLiteral(Season, Year) ?? $"{Season} {Year}";

    /// <summary>
    /// Retourne une saison météorologique à partir d'une représentation textuelle, exemple : "Automne 2010".
    /// </summary>
    /// <param name="literalSeason"></param>
    /// <returns></returns>
    public static WeatherSeason FromLiteral(string literalSeason)
        => SeasonHelpers.GetWeatherSeason(literalSeason);
}