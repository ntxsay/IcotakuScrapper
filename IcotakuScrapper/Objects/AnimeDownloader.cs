using System.ComponentModel;
using IcotakuScrapper.Anime;
using IcotakuScrapper.Extensions;

namespace IcotakuScrapper.Objects;

public enum AnimeDownloaderFinishedOperation
{
    Success,
    Error,
    Canceled,
}

/// <summary>
/// Classe permettant de télécharger les fiches animés et leurs vignettes en arrière plan
/// </summary>
public class AnimeDownloader : IDisposable, IAsyncDisposable
{
    private BackgroundWorker? _DownloadAnimeBackgroundWorker;

    public delegate void ProgressChangedEventHandler(Uri? animeUri, Tanime? anime, int percent);

    public event ProgressChangedEventHandler? ProgressChanged;

    public delegate void CompletedEventHandler(AnimeDownloaderFinishedOperation operationResult, string? message);

    public event CompletedEventHandler? OperationCompleted;


    /// <summary>
    /// Lance l'opération de téléchargement des fiches animés et de leurs vignettes en arrière plan
    /// </summary>
    /// <param name="animeUris"></param>
    /// <returns></returns>
    public async Task LaunchOperation(IReadOnlyCollection<Uri> animeUris)
    {
        if (_DownloadAnimeBackgroundWorker == null)
        {
            _DownloadAnimeBackgroundWorker = new BackgroundWorker()
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true,
            };

            _DownloadAnimeBackgroundWorker.DoWork += DownloadAnimeBackgroundWorkerOnDoWork;
            _DownloadAnimeBackgroundWorker.ProgressChanged += DownloadAnimeBackgroundWorkerOnProgressChanged;
            _DownloadAnimeBackgroundWorker.RunWorkerCompleted += DownloadAnimeBackgroundWorkerOnRunWorkerCompleted;
        }

        if (IsBusy)
            return;

        if (animeUris.Count == 0)
            return;

        _DownloadAnimeBackgroundWorker.RunWorkerAsync(animeUris);
        while (_DownloadAnimeBackgroundWorker.IsBusy)
        {
            await Task.Delay(100);
        }
    }
    
    public async Task CancelOperationAsync()
    {
        if (_DownloadAnimeBackgroundWorker == null)
            return;

        if (!_DownloadAnimeBackgroundWorker.IsBusy && !_DownloadAnimeBackgroundWorker.CancellationPending)
            return;

        _DownloadAnimeBackgroundWorker.CancelAsync();
        
        while (_DownloadAnimeBackgroundWorker.IsBusy)
        {
            await Task.Delay(100);
        }
    }

    private void DownloadAnimeBackgroundWorkerOnDoWork(object? sender, DoWorkEventArgs e)
    {
        var worker = sender as BackgroundWorker;

        if (e.Argument is not IReadOnlyCollection<Uri> items)
            return;

        int total = items.Count;
        double percentage;
        int count = 0;

        foreach (var item in items)
        {
            if (worker!.CancellationPending)
            {
                e.Cancel = true;
                return;
            }

            (Uri _sheetUri, Tanime? _anime) currentItem = default;
            
            //Télécharge la fiche anime
            using var downloadAnimeInfoTask = Tanime.ScrapAndGetFromUrlAsync(item);
            downloadAnimeInfoTask.Wait();

            //Si la fiche anime n'a pas été téléchargée on passe à l'item suivant
            var anime = downloadAnimeInfoTask.Result;
            if (anime == null)
            {
                count++;
                percentage = (double)count / total * 100;
                worker.ReportProgress(Convert.ToInt32(percentage), currentItem);
                continue;
            }

            //Télécharge la vignette de l'animé
            using var downloadThumbnailTask = anime.GetOrDownloadThumbnailAsync();
            downloadThumbnailTask.Wait();

            _ = downloadThumbnailTask.Result;

            currentItem = (item, anime);

            count++;
            percentage = (double)count / total * 100;
            worker.ReportProgress(Convert.ToInt32(percentage), currentItem);
            Thread.Sleep(100);
        }
    }

    private void DownloadAnimeBackgroundWorkerOnProgressChanged(object? sender, ProgressChangedEventArgs e)
    {
        if (e.UserState is not ValueTuple<Uri, Tanime?> operationState)
        {
            ProgressChanged?.Invoke(null, null, e.ProgressPercentage);
            return;
        }

        if (operationState.Item2 == null)
        {
            ProgressChanged?.Invoke(null, null, e.ProgressPercentage);
            return;
        }

        ProgressChanged?.Invoke(operationState.Item1, operationState.Item2, e.ProgressPercentage);
    }

    private void DownloadAnimeBackgroundWorkerOnRunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
    {
        if (e.Cancelled)
        {
            OperationCompleted?.Invoke(AnimeDownloaderFinishedOperation.Canceled,
                "L'opération a été annulée par l'utilisateur.");
        }
        else if (e.Error != null)
        {
            OperationCompleted?.Invoke(AnimeDownloaderFinishedOperation.Error, e.Error.Message);
        }
        else
        {
            OperationCompleted?.Invoke(AnimeDownloaderFinishedOperation.Success, "L'opération de téléchargement des fiches anime s'est terminée.");
        }
    }

    public bool IsBusy
        => _DownloadAnimeBackgroundWorker is { IsBusy: true };

    
    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (_DownloadAnimeBackgroundWorker != null)
            {
                if (_DownloadAnimeBackgroundWorker.IsBusy && !_DownloadAnimeBackgroundWorker.CancellationPending)
                {
                    _DownloadAnimeBackgroundWorker.CancelAsync();
                    
                    using var task = Task.Run(async () =>
                    {
                        while (_DownloadAnimeBackgroundWorker is { IsBusy: true })
                        {
                            await Task.Delay(100);
                        }
                    });
                    
                    task.Wait();
                }
            }
            _DownloadAnimeBackgroundWorker?.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private async ValueTask DisposeAsyncCore()
    {
        if (_DownloadAnimeBackgroundWorker != null)
        {
            if (_DownloadAnimeBackgroundWorker.IsBusy && !_DownloadAnimeBackgroundWorker.CancellationPending)
            {
                _DownloadAnimeBackgroundWorker.CancelAsync();
                while (_DownloadAnimeBackgroundWorker.IsBusy)
                {
                    await Task.Delay(100);
                }
            }
        }

        if (_DownloadAnimeBackgroundWorker is IAsyncDisposable downloadAnimeBackgroundWorkerAsyncDisposable)
            await downloadAnimeBackgroundWorkerAsyncDisposable.DisposeAsync();
        else
        {
            _DownloadAnimeBackgroundWorker?.Dispose();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        GC.SuppressFinalize(this);
    }

    ~AnimeDownloader()
    {
        Dispose(false);
    }
}