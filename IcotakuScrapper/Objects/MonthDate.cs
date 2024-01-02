namespace IcotakuScrapper.Objects;

public readonly struct MonthDate
{
    public ushort Year { get; init; }
    public byte Month { get; init; }
    public MonthDate(ushort year, byte month)
    {
        Year = year;
        Month = month;
    }

    public MonthDate(uint numberedDate)
    {
        var stringDate = numberedDate.ToString("000000");
        if (stringDate.Length != 6)
            return;
        
        if (!ushort.TryParse(stringDate.Substring(0, 4), out var year))
            return;
        
        if (!byte.TryParse(stringDate.Substring(4, 2), out var month))
        {
            Year = year;
            Month = 0;
            return;
        }

        Year = year;
        Month = month;
    }
    
    public static MonthDate FromDateTime(DateTime dateTime) => new((ushort)dateTime.Year, (byte)dateTime.Month);
    
    public static MonthDate FromNumberedDate(uint numberedDate)
    {
        var stringDate = numberedDate.ToString("000000");
        if (stringDate.Length != 6)
            return default;
        
        if (!ushort.TryParse(stringDate.Substring(0, 4), out var year))
            return default;
        
        if (!byte.TryParse(stringDate.Substring(4, 2), out var month))
            return new MonthDate(year, 0);
        
        return new MonthDate(year, month);
    }

    public override string ToString() =>
        $"{DateHelpers.GetMonthName(Month)} {Year:0000}";

    public uint ToNumberedDate() =>uint.TryParse($"{Year:0000}{Month:00}", out var result) ? result : (uint)0;
}