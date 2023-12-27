using IcotakuScrapper.Anime;
using Microsoft.Data.Sqlite;

namespace IcotakuScrapper.Extensions
{
    public static class TanimeExtensions
    {
        #region AlternativeTitles

        public static async Task AddOrReplaceAlternativeTitlesAsync(this TanimeBase anime,
            CancellationToken? cancellationToken = null,
            SqliteCommand? cmd = null)
        {
            ArgumentNullException.ThrowIfNull(anime);

            if (anime.AlternativeTitles.Count == 0)
            {
                LogServices.LogDebug("La collection est vide.");
                return;
            }

            foreach (var alternativeTitle in anime.AlternativeTitles.Where(alternativeTitle => alternativeTitle.IdAnime != anime.Id))
            {
                alternativeTitle.Id = anime.Id;
            }

            await TanimeAlternativeTitle.InsertOrReplaceAsync(anime.Id, anime.AlternativeTitles, DbInsertMode.InsertOrReplace, cancellationToken, cmd);
        }
        
        internal static async Task UpdateAlternativeTitlesAsync(this TanimeBase anime, CancellationToken? cancellationToken = null,
            SqliteCommand? cmd = null)
        {
            await TanimeAlternativeTitle.DeleteUnusedAsync(
                anime.AlternativeTitles.Select(x => (x.Title, x.IdAnime)).ToHashSet(), cancellationToken, cmd);
            await AddOrUpdateRangeAsync(anime, anime.AlternativeTitles, cancellationToken, cmd);
        }

        public static async Task AddOrUpdateRangeAsync(this TanimeBase anime, IReadOnlyCollection<TanimeAlternativeTitle> values, CancellationToken? cancellationToken = null,
            SqliteCommand? cmd = null)
        {
            ArgumentNullException.ThrowIfNull(anime);

            ArgumentNullException.ThrowIfNull(values);

            if (values.Count == 0)
            {
                LogServices.LogDebug("La collection est vide.");
                return;
            }

            foreach (var value in values)
                await anime.AddOrUpdateAsync(value, cancellationToken, cmd);
        }

        internal static async Task AddOrUpdateAsync(this TanimeBase anime, TanimeAlternativeTitle value,
            CancellationToken? cancellationToken = null,
            SqliteCommand? cmd = null)
        {
            ArgumentNullException.ThrowIfNull(anime);

            ArgumentNullException.ThrowIfNull(value);

            if (value.IdAnime != anime.Id)
                value.IdAnime = anime.Id;

            await value.AddOrUpdateAsync(cancellationToken, cmd);
        }

        #endregion

        #region Websites

        public static async Task AddOrReplaceWebsitesAsync(this TanimeBase anime,
            CancellationToken? cancellationToken = null,
            SqliteCommand? cmd = null)
        {
            ArgumentNullException.ThrowIfNull(anime);

            if (anime.Websites.Count == 0)
            {
                LogServices.LogDebug("La collection est vide.");
                return;
            }

            foreach (var item in anime.Websites.Where(item => item.IdAnime != anime.Id))
            {
                item.IdAnime = anime.Id;
            }

            await TanimeWebSite.InsertOrReplaceAsync(anime.Id, anime.Websites, DbInsertMode.InsertOrReplace, cancellationToken, cmd);
        }
        
        internal static async Task UpdateWebsitesAsync(this TanimeBase anime, CancellationToken? cancellationToken = null,
            SqliteCommand? cmd = null)
        {
            await TanimeWebSite.DeleteUnusedAsync(
                anime.Websites.Select(x => (x.Url, x.IdAnime)).ToHashSet(), cancellationToken, cmd);
            await AddOrUpdateRangeAsync(anime, anime.Websites, cancellationToken, cmd);
        }
        
        public static async Task AddOrUpdateRangeAsync(this TanimeBase anime, IReadOnlyCollection<TanimeWebSite> values,
            CancellationToken? cancellationToken = null,
            SqliteCommand? cmd = null)
        {
            ArgumentNullException.ThrowIfNull(anime);

            ArgumentNullException.ThrowIfNull(values);

            if (values.Count == 0)
            {
                LogServices.LogDebug("La collection est vide.");
                return;
            }

            foreach (var value in values)
            {
                await anime.AddOrUpdateAsync(value, cancellationToken, cmd);
            }
        }
        
        internal static async Task<OperationState> AddOrUpdateAsync(this TanimeBase anime, TanimeWebSite value, CancellationToken? cancellationToken = null,
            SqliteCommand? cmd = null)
        {
            ArgumentNullException.ThrowIfNull(anime);

            ArgumentNullException.ThrowIfNull(value);

            if (value.IdAnime != anime.Id)
                value.IdAnime = anime.Id;

            return await value.AddOrUpdateAsync(cancellationToken, cmd);
        }

        #endregion
        
         #region Studios

        public static async Task<OperationState> AddOrReplaceStudiosAsync(this TanimeBase anime, CancellationToken? cancellationToken = null,
            SqliteCommand? cmd = null)
        {
            ArgumentNullException.ThrowIfNull(anime);

            if (anime.Studios.Count == 0)
                return new OperationState(true, "La collection est vide.");

            return await TanimeStudio.InsertOrReplaceAsync(anime.Id, anime.Studios.Select(s => s.Id).ToArray(), DbInsertMode.InsertOrReplace, cancellationToken, cmd);
        }
        
        internal static async Task UpdateStudiosAsync(this Tanime anime, CancellationToken? cancellationToken = null,
            SqliteCommand? cmd = null)
        {
            await TanimeStudio.DeleteUnusedAsync(
                anime.Studios.Select(x => (x.Id, anime.Id)).ToHashSet(), cancellationToken, cmd);
            await AddOrUpdateRangeAsync(anime, anime.Studios.Select(s => new TanimeStudio(anime.Id, s)).ToHashSet(), cancellationToken, cmd);
        }

        private static async Task AddOrUpdateRangeAsync(this TanimeBase anime, IReadOnlyCollection<TanimeStudio> values, CancellationToken? cancellationToken = null,
            SqliteCommand? cmd = null)
        {
            ArgumentNullException.ThrowIfNull(anime);

            ArgumentNullException.ThrowIfNull(values);

            if (values.Count == 0)
            {
                LogServices.LogDebug("La collection est vide.");
                return;
            }

            foreach (var value in values)
                await anime.AddOrUpdateAsync(value, cancellationToken, cmd);
        }
        
        private static async Task AddOrUpdateAsync(this TanimeBase anime, TanimeStudio value,
            CancellationToken? cancellationToken = null,
            SqliteCommand? cmd = null)
        {
            ArgumentNullException.ThrowIfNull(anime);

            ArgumentNullException.ThrowIfNull(value);

            if (value.IdAnime != anime.Id)
                value.IdAnime = anime.Id;

            await value.AddOrUpdateAsync(cancellationToken, cmd);
        }

        #endregion
        
        #region Categories

        public static async Task<OperationState> AddOrReplaceCategoriesAsync(this TanimeBase anime, CancellationToken? cancellationToken = null,
            SqliteCommand? cmd = null)
        {
            ArgumentNullException.ThrowIfNull(anime);

            if (anime.Categories.Count == 0)
                return new OperationState(true, "La collection est vide.");

            return await TanimeCategory.InsertOrReplaceAsync(anime.Id, anime.Categories.Select(s => s.Id).ToArray(), DbInsertMode.InsertOrReplace, cancellationToken, cmd);
        }
        
        internal static async Task UpdateCategoriesAsync(this Tanime anime, CancellationToken? cancellationToken = null,
            SqliteCommand? cmd = null)
        {
            await TanimeCategory.DeleteUnusedAsync(
                anime.Categories.Select(x => (x.Id, anime.Id)).ToHashSet(), cancellationToken, cmd);
            await AddOrUpdateRangeAsync(anime, anime.Categories.Select(s => new TanimeCategory(anime.Id, s)).ToHashSet(), cancellationToken, cmd);
        }

        private static async Task AddOrUpdateRangeAsync(this TanimeBase anime, IReadOnlyCollection<TanimeCategory> values, CancellationToken? cancellationToken = null,
            SqliteCommand? cmd = null)
        {
            ArgumentNullException.ThrowIfNull(anime);

            ArgumentNullException.ThrowIfNull(values);

            if (values.Count == 0)
            {
                LogServices.LogDebug("La collection est vide.");
                return;
            }

            foreach (var value in values)
                await anime.AddOrUpdateAsync(value, cancellationToken, cmd);
        }
        
        private static Task<OperationState> AddOrUpdateAsync(this TanimeBase anime, TanimeCategory value, CancellationToken? cancellationToken = null,
            SqliteCommand? cmd = null)
        {
            ArgumentNullException.ThrowIfNull(anime);

            ArgumentNullException.ThrowIfNull(value);

            if (value.IdAnime != anime.Id)
                value.IdAnime = anime.Id;

            return value.AddOrUpdateAsync(cancellationToken, cmd);
        }

        #endregion
        
        #region Episodes

        public static async Task<OperationState> AddOrReplaceEpisodesAsync(this Tanime anime, CancellationToken? cancellationToken = null,
            SqliteCommand? cmd = null)
        {
            ArgumentNullException.ThrowIfNull(anime);

            if (anime.Episodes.Count == 0)
                return new OperationState(true, "La collection est vide.");
            
            foreach (var episode in anime.Episodes.Where(episode => episode.IdAnime != anime.Id))
            {
                episode.IdAnime = anime.Id;
            }

            return await TanimeEpisode.InsertOrReplaceAsync(anime.Id, anime.Episodes.ToArray(), DbInsertMode.InsertOrReplace, cancellationToken, cmd);
        }
        
        internal static async Task UpdateEpisodesAsync(this Tanime anime, CancellationToken? cancellationToken = null,
            SqliteCommand? cmd = null)
        {
            await TanimeEpisode.DeleteUnusedAsync(
                anime.Episodes.Select(x => (x.NoEpisode, anime.Id)).ToHashSet(), cancellationToken, cmd);
            await AddOrUpdateRangeAsync(anime, anime.Episodes.ToHashSet(), cancellationToken, cmd);
        }

        private static async Task AddOrUpdateRangeAsync(this Tanime anime, IReadOnlyCollection<TanimeEpisode> values, CancellationToken? cancellationToken = null,
            SqliteCommand? cmd = null)
        {
            ArgumentNullException.ThrowIfNull(anime);

            ArgumentNullException.ThrowIfNull(values);

            if (values.Count == 0)
            {
                LogServices.LogDebug("La collection est vide.");
                return;
            }

            foreach (var value in values)
                await anime.AddOrUpdateAsync(value, cancellationToken, cmd);
        }
        
        private static async Task<OperationState> AddOrUpdateAsync(this Tanime anime, TanimeEpisode value, CancellationToken? cancellationToken = null,
            SqliteCommand? cmd = null)
        {
            ArgumentNullException.ThrowIfNull(anime);

            ArgumentNullException.ThrowIfNull(value);

            if (value.IdAnime != anime.Id)
                value.IdAnime = anime.Id;

            return await value.AddOrUpdateAsync(cancellationToken, cmd);
        }

        #endregion

        #region Licenses

        public static async Task<OperationState> AddOrReplaceLicensesAsync(this TanimeBase anime, CancellationToken? cancellationToken = null,
            SqliteCommand? cmd = null)
        {
            ArgumentNullException.ThrowIfNull(anime);

            if (anime.Licenses.Count == 0)
                return new OperationState(true, "La collection est vide.");
            
            foreach (var item in anime.Licenses.Where(item => item.IdAnime != anime.Id))
            {
                item.IdAnime = anime.Id;
            }

            return await TanimeLicense.InsertOrReplaceAsync(anime.Id, anime.Licenses.ToArray(), DbInsertMode.InsertOrReplace, cancellationToken, cmd);
        }
        
        internal static async Task UpdateLicensesAsync(this Tanime anime, CancellationToken? cancellationToken = null,
            SqliteCommand? cmd = null)
        {
            await TanimeLicense.DeleteUnusedAsync(
                anime.Licenses.Select(x => (x.Distributor.Id, x.Type.Id, anime.Id)).ToHashSet(), cancellationToken, cmd);
            await AddOrUpdateRangeAsync(anime, anime.Licenses.ToHashSet(), cancellationToken, cmd);
        }

        private static async Task AddOrUpdateRangeAsync(this Tanime anime, IReadOnlyCollection<TanimeLicense> values, CancellationToken? cancellationToken = null,
            SqliteCommand? cmd = null)
        {
            ArgumentNullException.ThrowIfNull(anime);

            ArgumentNullException.ThrowIfNull(values);

            if (values.Count == 0)
            {
                LogServices.LogDebug("La collection est vide.");
                return;
            }

            foreach (var value in values)
                await anime.AddOrUpdateAsync(value, cancellationToken, cmd);
        }
        
        private static async Task<OperationState> AddOrUpdateAsync(this TanimeBase anime, TanimeLicense value, CancellationToken? cancellationToken = null,
            SqliteCommand? cmd = null)
        {
            ArgumentNullException.ThrowIfNull(anime);

            ArgumentNullException.ThrowIfNull(value);

            if (value.IdAnime != anime.Id)
                value.IdAnime = anime.Id;

            return await value.AddOrUpdateAsync(cancellationToken, cmd);
        }

        #endregion

        #region Staffs

        public static async Task<OperationState> AddOrReplaceStaffsAsync(this TanimeBase anime, CancellationToken? cancellationToken = null,
            SqliteCommand? cmd = null)
        {
            ArgumentNullException.ThrowIfNull(anime);

            if (anime.Staffs.Count == 0)
                return new OperationState(true, "La collection est vide.");
            
            foreach (var item in anime.Staffs.Where(item => item.IdAnime != anime.Id))
            {
                item.IdAnime = anime.Id;
            }

            return await TanimeStaff.InsertOrReplaceAsync(anime.Id, anime.Staffs.ToArray(), DbInsertMode.InsertOrReplace, cancellationToken, cmd);
        }
        
        internal static async Task UpdateStaffsAsync(this Tanime anime, CancellationToken? cancellationToken = null,
            SqliteCommand? cmd = null)
        {
            await TanimeStaff.DeleteUnusedAsync(
                anime.Staffs.Select(x => (x.Person.Id, x.Role.Id, anime.Id)).ToHashSet(), cancellationToken, cmd);
            await AddOrUpdateRangeAsync(anime, anime.Staffs.ToHashSet(), cancellationToken, cmd);
        }

        private static async Task AddOrUpdateRangeAsync(this TanimeBase anime, IReadOnlyCollection<TanimeStaff> values, CancellationToken? cancellationToken = null,
            SqliteCommand? cmd = null)
        {
            ArgumentNullException.ThrowIfNull(anime);

            ArgumentNullException.ThrowIfNull(values);

            if (values.Count == 0)
            {
                LogServices.LogDebug("La collection est vide.");
                return;
            }

            foreach (var value in values)
                await anime.AddOrUpdateAsync(value, cancellationToken, cmd);
        }
        
        private static async Task<OperationState> AddOrUpdateAsync(this TanimeBase anime, TanimeStaff value, CancellationToken? cancellationToken = null,
            SqliteCommand? cmd = null)
        {
            ArgumentNullException.ThrowIfNull(anime);

            ArgumentNullException.ThrowIfNull(value);

            if (value.IdAnime != anime.Id)
                value.IdAnime = anime.Id;

            return await value.AddOrUpdateAsync(cancellationToken, cmd);
        }

        #endregion

    }
}
