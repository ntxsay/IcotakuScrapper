using System.Diagnostics;
using System.Globalization;
using HtmlAgilityPack;
using IcotakuScrapper.Common;
using IcotakuScrapper.Extensions;

namespace IcotakuScrapper.Services;

/// <summary>
/// Fournit des méthodes d'assistance pour les dates
/// </summary>
public static class DateHelpers
{
    public static string? GetLiteralDay(DayOfWeek? dayOfWeek)
    {

        if (dayOfWeek == null)
            return null;

        return dayOfWeek.Value switch
        {
            DayOfWeek.Sunday => "Dimanche",
            DayOfWeek.Monday => "Lundi",
            DayOfWeek.Tuesday => "Mardi",
            DayOfWeek.Wednesday => "Mercredi",
            DayOfWeek.Thursday => "Jeudi",
            DayOfWeek.Friday => "Vendredi",
            DayOfWeek.Saturday => "Samedi",
            _ => null
        };
    }

    /// <summary>
    /// retourne le numéro du mois en fonction de son nom (en français)
    /// </summary>
    /// <param name="month">nom du mois en français</param>
    /// <returns></returns>
    public static byte GetMonthNumber(string month)
    {
        if (month.IsStringNullOrEmptyOrWhiteSpace())
            return 0;

        return month.Trim().ToLower() switch
        {
            "janvier" => 1,
            "fevrier" or "février" => 2,
            "mars" => 3,
            "avril" => 4,
            "mai" => 5,
            "juin" => 6,
            "juillet" => 7,
            "aout" or "août" => 8,
            "septembre" => 9,
            "octobre" => 10,
            "novembre" => 11,
            "decembre" or "décembre" => 12,
            _ => 0
        };
    }

    public static string? GetMonthSearchParameter(byte monthNumber)
        => monthNumber switch
        {
            1 => "janvier",
            2 => "fevrier",
            3 => "mars",
            4 => "avril",
            5 => "mai",
            6 => "juin",
            7 => "juillet",
            8 => "aout",
            9 => "septembre",
            10 => "octobre",
            11 => "novembre",
            12 => "decembre",
            _ => null
        };
    
    

    /// <summary>
    /// Retourne le nom du mois en fonction de son numéro
    /// </summary>
    /// <param name="monthNumber"></param>
    /// <returns></returns>
    public static string? GetMonthName(byte monthNumber)
    {
        if (monthNumber is < 1 or > 13)
            return null;

        try
        {
            var culture = CultureInfo.CurrentCulture;
            return culture.DateTimeFormat.GetMonthName(monthNumber);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            return null;
        }
    }


    /// <summary>
    /// Retourne la date en fonction de la chaîne de caractère et du format
    /// </summary>
    /// <param name="stringDate"></param>
    /// <param name="format"></param>
    /// <returns></returns>
    public static DateOnly GetDateOnly(string stringDate, string format = "yyyy-MM-dd")
    {
        if (stringDate.IsStringNullOrEmptyOrWhiteSpace())
            return default;

        if (DateOnly.TryParseExact(stringDate, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            return date;

        return default;
    }

    /// <summary>
    /// Retourne la date en fonction de la chaîne de caractère et du format
    /// </summary>
    /// <param name="stringDate"></param>
    /// <param name="format"></param>
    /// <returns></returns>
    public static DateOnly? GetNullableDateOnly(string? stringDate, string format = "yyyy-MM-dd")
    {
        if (stringDate == null || stringDate.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        if (DateOnly.TryParseExact(stringDate, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            return date;

        return null;
    }
    

    /// <summary>
    /// Retourne la date en fonction de la chaîne de caractère
    /// </summary>
    /// <param name="stringDate">Date anglaise yyyy-MM-dd</param>
    /// <returns></returns>
    public static (byte day, byte month, uint year) GetTupledDate(string stringDate)
    {
        if (stringDate.IsStringNullOrEmptyOrWhiteSpace())
            return (0,0,0);

        var splitDate = stringDate.Split('-', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (splitDate.Length != 3)
            return (0,0,0);

        if (!uint.TryParse(splitDate[0], out var year))
            return (0, 0, 0);

        if (!byte.TryParse(splitDate[1], out var month))
            return (0, 0, year);

        if (!byte.TryParse(splitDate[2], out var day))
            return (0, month, year);

        return (day, month, year);
    }

    public static string? GetYearMonthLiteral(uint intDate, string format = "MMMM yyyy")
    {
        if (intDate == 0)
            return null;
        var stringIntDate = intDate.ToString();
        if (stringIntDate.Length != 6)//2308 -//202304
            return null;
        var yearString = stringIntDate[..4];
        var monthString = stringIntDate.Substring(4, 2);

        if (!uint.TryParse(yearString, out var year) || year < DateOnly.MinValue.Year || year > DateOnly.MaxValue.Year)
            return null;

        if (!byte.TryParse(monthString, out var month) || month is < 1 or > 12)
            return year.ToString();

        var value = new DateOnly((int)year, month, 1).ToString(format, CultureInfo.CurrentCulture);
        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value);
    }

    public static ushort GetYear(uint intDate)
    {
        
        if (intDate == 0)
            return 0;
        var stringIntDate = intDate.ToString();
        if (stringIntDate.Length != 6)//2308 -//202304
            return 0;
        var yearString = stringIntDate[..4];
        
        if (!ushort.TryParse(yearString, out var year))
            return 0;
        
        return year;
    }
    
    public static uint GetYearMonthInt(DateOnly date)
    {
        var year = date.Year;
        var month = date.Month;
        return uint.Parse($"{year:0000}{month:00}");
    }

    #region Season

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

    #endregion
}