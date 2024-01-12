using IcotakuScrapper.Anime;

namespace IcotakuScrapper.Objects;

public class AnimeUpdater
{
    public int Count { get; private set;  }
    private (int Id, int SheetId, string Url)[] _animeToUpdate;
    public async Task FindAnimeToUpdateAsync(WeatherSeason season)
    {
        _animeToUpdate = await TanimeBase.FindAnimesToBeUpdate(season).ToArrayAsync();
        Count = _animeToUpdate.Length;
    }
}