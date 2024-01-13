using IcotakuScrapper.Extensions;
using IcotakuScrapper.Objects;

namespace IcotakuScrapper.Services;

/// <summary>
/// Classe statique contenant des méthodes d'assistance pour les saisons
/// </summary>
public static class SeasonHelpers
{
    /// <summary>
    /// Retourne le numéro de la saison en fonction de son nom (en français)
    /// </summary>
    /// <param name="saisonName"></param>
    /// <returns></returns>
    public static byte GetSeasonNumber(string saisonName)
    {
        if (saisonName.IsStringNullOrEmptyOrWhiteSpace())
            return 0;

        return (byte)GetSeasonKind(saisonName);
    }

    /// <summary>
    /// Retourne le numéro de la saison en fonction de son nom (en français)
    /// </summary>
    /// <param name="saisonName"></param>
    /// <returns></returns>
    public static WeatherSeasonKind GetSeasonKind(string saisonName)
    {
        if (saisonName.IsStringNullOrEmptyOrWhiteSpace())
            return 0;

        return saisonName.Trim().ToLower() switch
        {
            "printemps" => WeatherSeasonKind.Spring,
            "ete" or "été" or "éte" or "eté" => WeatherSeasonKind.Summer,
            "automne" => WeatherSeasonKind.Fall,
            "hivers" or "hiver" => WeatherSeasonKind.Winter,
            _ => WeatherSeasonKind.Unknown
        };
    }

    /// <summary>
    /// Retourne le nom de la saison en fonction de son numéro
    /// </summary>
    /// <param name="seasonNumber"></param>
    /// <returns></returns>
    public static string? GetSeasonName(byte seasonNumber)
    {
        return seasonNumber switch
        {
            1 => "Printemps",
            2 => "Été",
            3 => "Automne",
            4 => "Hiver",
            _ => null
        };
    }

    /// <summary>
    /// Retourne le nom de la saison en fonction de son numéro
    /// </summary>
    /// <param name="season"></param>
    /// <returns></returns>
    public static string? GetSeasonName(WeatherSeasonKind season)
    {
        return season switch
        {
            WeatherSeasonKind.Spring => "Printemps",
            WeatherSeasonKind.Summer => "Été",
            WeatherSeasonKind.Fall => "Automne",
            WeatherSeasonKind.Winter => "Hiver",
            _ => null
        };
    }

    public static string? GetSeasonLiteral(uint numberedSeason)
    {
        if (numberedSeason == 0)
            return null;
        var stringIntDate = numberedSeason.ToString();
        if (stringIntDate.Length != 6)//2301-202304
            return null;
        var yearString = stringIntDate[..4];
        var seasonNumberString = stringIntDate.Substring(4, 2);
        
        if (!uint.TryParse(yearString, out var year) || year < DateOnly.MinValue.Year || year > DateOnly.MaxValue.Year)
            return null;
        
        if (!byte.TryParse(seasonNumberString, out var seasonNumber) || seasonNumber is < 1 or > 4)
            return null;
        
        return GetSeasonLiteral((WeatherSeasonKind)seasonNumber, year);
    }
    
    public static string? GetSeasonSearchParameter(WeatherSeasonKind season)
    {
        return season switch
        {
            WeatherSeasonKind.Spring => "printemps",
            WeatherSeasonKind.Summer => "ete",
            WeatherSeasonKind.Fall => "automne",
            WeatherSeasonKind.Winter => "hiver",
            _ => null
        };
    }

    public static WeatherSeason GetWeatherSeason(uint numberedSeason)
    {
        if (numberedSeason == 0)
            return default;
        var stringIntDate = numberedSeason.ToString();
        if (stringIntDate.Length != 6)//2301-202304
            return default;
        var yearString = stringIntDate[..4];
        var seasonNumberString = stringIntDate.Substring(4, 2);
        
        if (!uint.TryParse(yearString, out var year) || year < DateOnly.MinValue.Year || year > DateOnly.MaxValue.Year)
            return default;
        
        if (!byte.TryParse(seasonNumberString, out var seasonNumber) || seasonNumber is < 1 or > 4)
            return default;
        
        return new WeatherSeason((WeatherSeasonKind)seasonNumber, year);
    }

    public static WeatherSeason GetWeatherSeason(string literalSeason)
    {
        var split = literalSeason.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (split.Length != 2)
            return default;

        var season = GetSeasonKind(split[0]);
        if (season == WeatherSeasonKind.Unknown)
            return default;

        if (!uint.TryParse(split[1], out var year) || year < DateOnly.MinValue.Year || year > DateOnly.MaxValue.Year)
            return default;

        return new WeatherSeason(season, year);
    }

    public static WeatherSeason GetWeatherSeason(DateTime date)
    {
        var year = (uint)date.Year;
        var month = (byte)date.Month;
        var season = month switch
        {
            >= 1 and <= 3 => WeatherSeasonKind.Winter,
            >= 4 and <= 6 => WeatherSeasonKind.Spring,
            >= 7 and <= 8 => WeatherSeasonKind.Summer,
            >= 9 and <= 12 => WeatherSeasonKind.Fall,
            _ => WeatherSeasonKind.Unknown
        };
        return new WeatherSeason(season, season == WeatherSeasonKind.Winter ? year - 1 : year);
    }
    
    public static string? GetSeasonLiteral(WeatherSeason season)
    {
        return GetSeasonLiteral(season.Season, season.Year);
    }

    public static string? GetSeasonLiteral(WeatherSeasonKind season, uint year)
    {
        if (year < DateOnly.MinValue.Year || year > DateOnly.MaxValue.Year)
            return null;

        var seasonName = GetSeasonName(season);
        return seasonName == null ? null : $"{seasonName} {year}";
    }

    public static uint GetIntSeason(WeatherSeasonKind season, uint year)
    {
        if (year < DateOnly.MinValue.Year || year > DateOnly.MaxValue.Year)
            return 0;

        return GetIntSeason((byte)season, year);
    }

    public static uint GetIntSeason(byte seasonNumber, uint year)
    {
        if (year < DateOnly.MinValue.Year || year > DateOnly.MaxValue.Year || seasonNumber is < 1 or > 4)
            return 0;

        return uint.Parse($"{year:0000}{seasonNumber:00}");
    }


    public static bool IsSeasonValidated(uint intSeason)
    {
        if (intSeason == 0)
            return false;
        var stringIntDate = intSeason.ToString();
        if (stringIntDate.Length != 6)//2301-202304
            return false;
        var yearString = stringIntDate[..4];
        var seasonNumberString = stringIntDate.Substring(4, 2);
        
        if (!uint.TryParse(yearString, out var year) || year < DateOnly.MinValue.Year || year > DateOnly.MaxValue.Year)
            return false;
        
        if (!byte.TryParse(seasonNumberString, out var seasonNumber) || seasonNumber is < 1 or > 4)
            return false;

        return true;
    }
    
}