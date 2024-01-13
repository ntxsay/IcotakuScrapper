using System.Diagnostics;
using System.Globalization;
using HtmlAgilityPack;
using IcotakuScrapper.Common;
using IcotakuScrapper.Extensions;
using IcotakuScrapper.Objects;

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

    /// <summary>
    /// Convertit une date en chaîne de caractère en nombre entier yyyyMM
    /// </summary>
    /// <param name="date">date en chaine de caractères au format MMMM yyyy (ex : Janvier 2013)</param>
    /// <returns>Retourne un nombre entier qui suit la logique yyyyMM</returns>
    internal static uint GetNumberedMonthAndYear(string? date)
    {
        if (date == null || date.IsStringNullOrEmptyOrWhiteSpace())
            return 0;

        var _date = date.Trim();

        var split = _date.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (split.Length == 1)
        {
            if (!ushort.TryParse(split[0], out ushort year))
                return uint.Parse($"{year:0000}00");
        }
        else if (split.Length == 2)
        {
            var monthNumber = GetMonthNumber(split[0]);

            if (!ushort.TryParse(split[1], out ushort year))
                return 0;

            return uint.Parse($"{year:0000}{monthNumber:00}");
        }

        return 0;
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

}