using IcotakuScrapper.Anime;
using Microsoft.Data.Sqlite;

namespace IcotakuScrapper.Extensions
{
    public static class TanimeExtensions
    {
        public static async Task<OperationState> AddAlternativeTitlesAsync(this Tanime anime, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
        {
            ArgumentNullException.ThrowIfNull(anime);

            if (anime.AlternativeTitles.Count == 0)
                return new OperationState(true, "La collection est vide.");

            foreach (var alternativeTitle in anime.AlternativeTitles)
            {
                if (alternativeTitle.Id != anime.Id)
                    alternativeTitle.Id = anime.Id;
            }

            return await TanimeAlternativeTitle.InsertAsync(anime.Id, anime.AlternativeTitles, DbInsertMode.InsertOrReplace, cancellationToken, cmd);
        }

        public static async Task<OperationState> AddOrUpdateRangeAsync(this Tanime anime, IReadOnlyCollection<TanimeAlternativeTitle> values, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
        {

            ArgumentNullException.ThrowIfNull(anime);

            ArgumentNullException.ThrowIfNull(values);

            if (values.Count == 0)
                return new OperationState(true, "La collection est vide.");

            List<OperationState> result = [];
            foreach (var value in values)
            {
                result.Add(await anime.AddOrUpdateAsync(value, cancellationToken, cmd));
            }

            return new OperationState(result.All(x => x.IsSuccess), $"{result.Count(c => c.IsSuccess)} valeur(s) mise(s) à jour ou ajoutée(s) sur {result.Count}.");
        }

        public static async Task<OperationState> AddOrUpdateAsync(this Tanime anime, TanimeAlternativeTitle value, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
        {
            ArgumentNullException.ThrowIfNull(anime);

            ArgumentNullException.ThrowIfNull(value);

            if (value.Id != anime.Id)
                value.Id = anime.Id;

            if (await TanimeAlternativeTitle.ExistsAsync(value.Id, SelectCountIdIdAnimeKind.Id, cancellationToken, cmd))
                return await value.UpdateAsync(cancellationToken, cmd);
            else
            {
                var result = await value.InsertAsync(cancellationToken, cmd);
                return result.ToBaseState();
            }
        }

        public static async Task<OperationState<int>> AddAsync(this Tanime anime, TanimeAlternativeTitle value, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
        {
            ArgumentNullException.ThrowIfNull(anime);

            ArgumentNullException.ThrowIfNull(value);

            if (value.Id != anime.Id)
                value.Id = anime.Id;

            return await value.InsertAsync(cancellationToken, cmd);
        }

        public static async Task<OperationState> UpdateAsync(this Tanime anime, TanimeAlternativeTitle value, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
        {
            ArgumentNullException.ThrowIfNull(anime);

            ArgumentNullException.ThrowIfNull(value);

            if (value.Id != anime.Id)
                value.Id = anime.Id;

            return await value.UpdateAsync(cancellationToken, cmd);
        }

        public static async Task<OperationState> AddRangeAsync(this Tanime anime, IReadOnlyCollection<TanimeAlternativeTitle> values, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
        {
            ArgumentNullException.ThrowIfNull(anime);

            ArgumentNullException.ThrowIfNull(values);

            if (values.Count == 0)
                return new OperationState(true, "La collection est vide.");

            return await TanimeAlternativeTitle.InsertAsync(anime.Id, values, DbInsertMode.InsertOrReplace, cancellationToken, cmd);
        }
    }
}
