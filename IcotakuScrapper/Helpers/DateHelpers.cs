using System.Diagnostics;
using System.Globalization;
using IcotakuScrapper.Extensions;

namespace IcotakuScrapper.Helpers;

/// <summary>
/// Fournit des méthodes d'assistance pour les dates
/// </summary>
public static class DateHelpers
{
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
    
    /// <summary>
    /// Retourne le numéro de la saison en fonction de son nom (en français)
    /// </summary>
    /// <param name="saisonName"></param>
    /// <returns></returns>
    public static byte GetSeasonNumber(string saisonName)
    {
        if (saisonName.IsStringNullOrEmptyOrWhiteSpace())
            return 0;

        return saisonName.Trim().ToLower() switch
        {
            "printemps" => 1,
            "ete" or "été" or "éte" or "eté" => 2,
            "automne" => 3,
            "hivers" => 4,
            _ => 0
        };
    }
    
    /// <summary>
    /// Retourne le nom de la saison en fonction de son numéro
    /// </summary>
    /// <param name="seasonNumber"></param>
    /// <returns></returns>
    public static string GetSeasonName(byte seasonNumber)
    {
        return seasonNumber switch
        {
            1 => "Printemps",
            2 => "Été",
            3 => "Automne",
            4 => "Hivers",
            _ => "Inconnu"
        };
    }

    /// <summary>
    /// Retourne le nom du mois en fonction de son numéro
    /// </summary>
    /// <param name="monthNumber"></param>
    /// <returns></returns>
    public static string? GetMonthName(byte monthNumber)
    {
        if (monthNumber is < 1 or > 12)
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
}