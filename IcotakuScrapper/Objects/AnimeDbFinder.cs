using System.ComponentModel;
using HtmlAgilityPack;
using IcotakuScrapper.Anime;
using IcotakuScrapper.Objects.Models;

namespace IcotakuScrapper.Objects;

public class AnimeDbFinder
{
    public delegate void OperationCompletedEventHandler(RunWorkerCompletedEventArgs args);
    public event OperationCompletedEventHandler? OperationCompletedRequested;

    public delegate void ProgressChangedEventHandler(double percent, OperationState<TanimeBase?> operationState);
    public event ProgressChangedEventHandler? ProgressChangedRequested;


    private BackgroundWorker _worker;

    private AnimeDbFinderOptions? _parameter = null;
    private bool disposedValue;

    public bool IsRunning => _worker.IsBusy;
    public bool IsCancelled => _worker.CancellationPending;
    
     public AnimeDbFinder()
    {
        _worker = new BackgroundWorker
        {
            WorkerReportsProgress = true,
            WorkerSupportsCancellation = true
        };

        _worker.DoWork += WorkerOnDoWork;
        _worker.ProgressChanged += WorkerOnProgressChanged;
        _worker.RunWorkerCompleted += WorkerOnRunWorkerCompleted;
    }

    public void Find(AnimeDbFinderOptions parameter)
    {
        if (IsRunning)
        {
            LogServices.LogDebug("La recherche est déjà en cours.");
            return;
        }

        var uri = IcotakuWebHelpers.GetAdvancedSearchUri(IcotakuSection.Anime, new AnimeFinderParameter());
        if (uri == null)
        {
            LogServices.LogDebug("Impossible de créer l'uri de recherche.");
            return;
        }

        HtmlWeb htmlWeb = new();
        var htmlDocument = htmlWeb.Load(uri);

        var tableNode = htmlDocument.DocumentNode.SelectSingleNode("//table[contains(@class, 'table_apercufiche')]");
        if (tableNode == null)
        {
            LogServices.LogDebug("Impossible de récupérer le tableau des résultats.");
            return;
        }

        _parameter = parameter;
        _worker.RunWorkerAsync(htmlDocument.DocumentNode);
    }

    private void WorkerOnDoWork(object? sender, DoWorkEventArgs e)
    {
        if (e.Argument is not HtmlNode htmlNode)
            return;

        var (minPage, maxPage) = TanimeBase.GetSearchMinAndMaxPage(htmlNode);
        //Compte appriximativement le nombre de fiches, il y a 15 fiches par page
        var totalItems = (int)(maxPage * 15);
        var count = 0;

        //Obtient l'uri des fiches de la première page
        var animeSheetUris = TanimeBase.ScrapSearchResultUri(htmlNode).ToArray();
        foreach (var animeSheetUri in animeSheetUris)
        {
            if (_worker.CancellationPending)
            {
                e.Cancel = true;
                return;
            }

            count++;
            var percent = count * 100 / totalItems;

            using var animeBaseResult = TanimeBase.ScrapAnimeBaseAsync(animeSheetUri, AnimeScrapingOptions.All);
            animeBaseResult.Wait();
            
            _worker.ReportProgress(percent, animeBaseResult.Result);
            Thread.Sleep(100);
        }

        //Si il n'y a qu'une seule page, on arrête la recherche
        if (minPage < 1 || maxPage < 2)
            return;

        //Sinon on scrap les autres pages
        for (var i = minPage; i <= maxPage; i++)
        {
            if (_worker.CancellationPending)
            {
                e.Cancel = true;
                return;
            }

            //Obtient l'uri de la page en cours
            var pageUri = IcotakuWebHelpers.GetAdvancedSearchUri(IcotakuSection.Anime, new AnimeFinderParameter(), i);
            if (pageUri == null)
                continue;

            //Charge le document HTML
            HtmlWeb htmlWeb = new();
            var htmlDocument = htmlWeb.Load(pageUri);

            //Obtient l'uri des fiches
            animeSheetUris = TanimeBase.ScrapSearchResultUri(htmlDocument.DocumentNode).ToArray();

            //Si on est à la dernière page, on vérifie si le nombre de fiches est inférieur à 15 pour ajuster le nombre total de fiches
            if (i == maxPage)
            {
                var pageCountItems = animeSheetUris.Length;
                if (pageCountItems < 15)
                {
                   
                    totalItems -= (15 - pageCountItems);
                }
            }

            //Scrap les fiches
            foreach (var sheetUri in animeSheetUris)
            {
                if (_worker.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }

                count++;
                var percent2 = count * 100 / totalItems;

                using var animeBaseResult2 = TanimeBase.ScrapAnimeBaseAsync(sheetUri, AnimeScrapingOptions.All);
                animeBaseResult2.Wait();
                
                _worker.ReportProgress(percent2, animeBaseResult2.Result);
                Thread.Sleep(100);
            }
        }
    }

    private void WorkerOnProgressChanged(object? sender, ProgressChangedEventArgs e)
    {
        if (e.UserState is not OperationState<TanimeBase?> operationState)
        {
            ProgressChangedRequested?.Invoke(e.ProgressPercentage, default);
            return;
        }

        ProgressChangedRequested?.Invoke(e.ProgressPercentage, operationState);
    }

    private void WorkerOnRunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
    {
        OperationCompletedRequested?.Invoke(e);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: supprimer l'état managé (objets managés)
            }

            // TODO: libérer les ressources non managées (objets non managés) et substituer le finaliseur
            _worker.Dispose();
            // TODO: affecter aux grands champs une valeur null
            _worker = null!;

            disposedValue = true;
        }
    }

    // TODO: substituer le finaliseur uniquement si 'Dispose(bool disposing)' a du code pour libérer les ressources non managées
    ~AnimeDbFinder()
    {
        // Ne changez pas ce code. Placez le code de nettoyage dans la méthode 'Dispose(bool disposing)'
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        // Ne changez pas ce code. Placez le code de nettoyage dans la méthode 'Dispose(bool disposing)'
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    
}