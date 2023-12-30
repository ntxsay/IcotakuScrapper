namespace IcotakuScrapper.Anime
{
    /// <summary>
    /// Structure contenante les informations additionnelles d'un anime.
    /// </summary>
    internal readonly struct AnimeAdditionalInfosStruct
    {
        internal int SheetId { get; init; }
        internal string? ThumbnailUrl { get; init; }
        internal bool IsAdultContent { get; init; }
        internal bool IsExplicitContent { get; init; }
    }
}
