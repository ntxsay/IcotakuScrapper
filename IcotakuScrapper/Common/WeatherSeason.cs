﻿using IcotakuScrapper.Helpers;

namespace IcotakuScrapper.Common;

public readonly struct IcotakuSeason
{
    public FourSeasonsKind Season { get; }
    public uint Year { get; }
    
    public IcotakuSeason(FourSeasonsKind season, uint year)
    {   
        if (year < DateOnly.MinValue.Year || year > DateOnly.MaxValue.Year)
            throw new ArgumentOutOfRangeException(nameof(year), year, $"L'année doit être comprise entre {DateOnly.MinValue.Year} et {DateOnly.MaxValue.Year}.");
        
        if (season == FourSeasonsKind.Unknown)
            throw new ArgumentOutOfRangeException(nameof(season), season, "La saison ne peut pas être inconnue.");
        
        Season = season;
        Year = year;
    }
    
    public uint ToIntSeason()
        => DateHelpers.GetIntSeason(Season, Year);
}