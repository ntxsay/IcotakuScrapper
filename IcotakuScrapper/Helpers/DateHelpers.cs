using System.Diagnostics;
using System.Globalization;
using IcotakuScrapper.Extensions;

namespace IcotakuScrapper.Helpers;

public static class DateHelpers
{
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

}