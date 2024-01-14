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

public class AnimeDownloader : IDisposable, IAsyncDisposable
{
    private BackgroundWorker? _downloadAnimeBackgroundWorker;

    public delegate void ProgressChangedEventHandler(Uri? animeUri, Tanime? anime, int percent);

    public event ProgressChangedEventHandler? ProgressChanged;

    public delegate void CompletedEventHandler(AnimeDownloaderFinishedOperation operationResult, string? message);

    public event CompletedEventHandler? OperationCompleted;


    /// <summary>
    /// Lance l'opération de téléchargement des fiches animés et de leurs vignettes en arrêtant l'opération en arrière-plan
    /// </summary>
    /// <param name="animeUris"></param>
    /// <returns></returns>
    public bool LaunchOperation(IReadOnlyCollection<Uri> animeUris)
    {
        if (_downloadAnimeBackgroundWorker == null)
        {
            _downloadAnimeBackgroundWorker = new BackgroundWorker()
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true,
            };

            _downloadAnimeBackgroundWorker.DoWork += DownloadAnimeBackgroundWorkerOnDoWork;
            _downloadAnimeBackgroundWorker.ProgressChanged += DownloadAnimeBackgroundWorkerOnProgressChanged;
            _downloadAnimeBackgroundWorker.RunWorkerCompleted += DownloadAnimeBackgroundWorkerOnRunWorkerCompleted;
        }

        if (IsBusy)
            return false;

        if (animeUris.Count == 0)
            return false;

        _downloadAnimeBackgroundWorker.RunWorkerAsync(animeUris);
        return true;
    }
    
    public void CancelDownloadAnimeOperation()
    {
        if (_downloadAnimeBackgroundWorker == null)
            return;

        if (!_downloadAnimeBackgroundWorker.IsBusy && !_downloadAnimeBackgroundWorker.CancellationPending)
            return;

        _downloadAnimeBackgroundWorker.CancelAsync();
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
            OperationCompleted?.Invoke(AnimeDownloaderFinishedOperation.Success, null);
        }
    }

    public bool IsBusy
        => _downloadAnimeBackgroundWorker is { IsBusy: true };

    
    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (_downloadAnimeBackgroundWorker != null)
            {
                if (_downloadAnimeBackgroundWorker.IsBusy && !_downloadAnimeBackgroundWorker.CancellationPending)
                {
                    _downloadAnimeBackgroundWorker.CancelAsync();
                    
                    using var task = Task.Run(async () =>
                    {
                        while (_downloadAnimeBackgroundWorker is { IsBusy: true })
                        {
                            await Task.Delay(100);
                        }
                    });
                    
                    task.Wait();
                }
            }
            _downloadAnimeBackgroundWorker?.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private async ValueTask DisposeAsyncCore()
    {
        if (_downloadAnimeBackgroundWorker != null)
        {
            if (_downloadAnimeBackgroundWorker.IsBusy && !_downloadAnimeBackgroundWorker.CancellationPending)
            {
                _downloadAnimeBackgroundWorker.CancelAsync();
                while (_downloadAnimeBackgroundWorker.IsBusy)
                {
                    await Task.Delay(100);
                }
            }
        }

        if (_downloadAnimeBackgroundWorker is IAsyncDisposable downloadAnimeBackgroundWorkerAsyncDisposable)
            await downloadAnimeBackgroundWorkerAsyncDisposable.DisposeAsync();
        else
        {
            _downloadAnimeBackgroundWorker?.Dispose();
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