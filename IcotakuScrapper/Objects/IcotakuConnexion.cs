using System.Net;
using System.Net.Http.Headers;
using HtmlAgilityPack;
using IcotakuScrapper.Extensions;

namespace IcotakuScrapper.Objects;

/// <summary>
/// Classe permettant de se connecter en tant qu'utilisateur au site d'Icotaku
/// </summary>
public class IcotakuConnexion : IDisposable
{
    private readonly Uri _baseUri = new ("https://anime.icotaku.com/");
    private readonly HttpClient _httpClient;
    private readonly HttpClientHandler _handler;
    
    /// <summary>
    /// Nom d'utilisateur de l'utilisateur
    /// </summary>
    public string Username { get; }
    
    /// <summary>
    /// Mot de passe de l'utilisateur
    /// </summary>
    private string Password { get; }
    
    /// <summary>
    /// Obtient une valeur indiquant si l'utilisateur est connecté
    /// </summary>
    public bool IsAuthenticated { get; private set; }
    
    public IcotakuConnexion(string username, string password)
    {
        ArgumentException.ThrowIfNullOrEmpty(username);
        ArgumentException.ThrowIfNullOrWhiteSpace(username);
        ArgumentException.ThrowIfNullOrEmpty(password);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        
        Username = username;
        Password = password;
        
        _handler = new HttpClientHandler
        {
            CookieContainer = new CookieContainer(),
            UseCookies = true
        };

        _httpClient = new HttpClient(_handler);
        
        _httpClient.BaseAddress = _baseUri;
        
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xhtml+xml"));
        _httpClient.DefaultRequestHeaders.AcceptEncoding.Clear();
        _httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("utf-8"));
        _httpClient.DefaultRequestHeaders.AcceptLanguage.Clear();
        _httpClient.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("fr-FR"));
        _httpClient.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("fr"));
    }
    
    /// <summary>
    /// Lance l'autentification de l'utilisateur sur le site d'Icotaku
    /// </summary>
    /// <returns></returns>
    public async Task<bool> ConnectAsync(CancellationToken? cancellationToken = null)
    {
        try
        {
            if (IsAuthenticated)
            {
                LogServices.LogDebug("L'utilisateur est déjà connecté");
                return true;
            }
            
            // Récupérer la page d'accueil pour récupérer le jeton CSRF
            using var homePageResult = await _httpClient.GetAsync("/", cancellationToken ?? CancellationToken.None);
            homePageResult.EnsureSuccessStatusCode();

            var homePageContent = await homePageResult.Content.ReadAsStringAsync();
            var homePageDocument = new HtmlDocument();
            homePageDocument.LoadHtml(homePageContent);

            var csrfTokenNode = homePageDocument.DocumentNode.SelectSingleNode("//input[@name='_csrf_token']");
            if (csrfTokenNode == null)
            {
                LogServices.LogDebug("Le jeton CSRF n'a pas été trouvé");
                IsAuthenticated = false;
                return false;
            }

            var csrfToken = csrfTokenNode.GetAttributeValue("value", string.Empty);

            if (csrfToken.IsStringNullOrEmptyOrWhiteSpace())
            {
                LogServices.LogDebug("Le jeton CSRF est vide");
                IsAuthenticated = false;
                return false;
            }

            // Récupérer le cookie de session
            var cookies = _handler.CookieContainer.GetCookies(_baseUri);
            var sessionCookie = cookies["icookie"];

            if (sessionCookie == null)
            {
                LogServices.LogDebug("Le cookie de session n'a pas été trouvé");
                IsAuthenticated = false;
                return false;
            }

            // Se connecter
            using var loginResult = await _httpClient.PostAsync("/login.html", 
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "_csrf_token", csrfToken },
                    { "login", Username },
                    { "password", Password },
                    { "referer", "/" }
                }), cancellationToken ?? CancellationToken.None);

            loginResult.EnsureSuccessStatusCode();
            IsAuthenticated = true;
            return true;
        }
        catch (Exception e)
        {
            LogServices.LogDebug(e);
            IsAuthenticated = false;
            return false;
        }
    }

    /// <summary>
    /// Déconnecte l'utilisateur du site d'Icotaku
    /// </summary>
    /// <param name="cancellationToken"></param>
    public async Task DisconnectAsync(CancellationToken? cancellationToken = null)
    {
        if (!IsAuthenticated)
        {
            LogServices.LogDebug("L'utilisateur s'est déjà déconnecté");
            return;
        }
        
        try
        {
            using var result = await _httpClient.GetAsync("/logout.html", cancellationToken ?? CancellationToken.None);
            result.EnsureSuccessStatusCode();
            IsAuthenticated = false;
        }
        catch (Exception e)
        {
            LogServices.LogDebug(e);
        }
    }

    /// <summary>
    /// Retourne le contenu HTML de la page demandée
    /// </summary>
    /// <param name="uri"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<string?> GetHtmlStringAsync(Uri uri, CancellationToken? cancellationToken = null)
    {
        if (!IsAuthenticated)
        {
            LogServices.LogDebug("L'utilisateur n'est pas connecté");
            return null;
        }
            
        if (!uri.Host.Contains("icotaku.com") || !uri.IsAbsoluteUri)
        {
            LogServices.LogDebug("L'uri n'est pas valide");
            return null;
        }
        
        try
        {
            using var result = await _httpClient.GetAsync(uri, cancellationToken ?? CancellationToken.None);
            result.EnsureSuccessStatusCode();

            var htmlContent = await result.Content.ReadAsStringAsync();
            return htmlContent;
        }
        catch (Exception e)
        {
            LogServices.LogDebug(e);
            return null;
        }
    }

    /// <summary>
    /// Soumet une requête POST à l'adresse absolue spécifiée
    /// </summary>
    /// <param name="requestUri"></param>
    /// <param name="formUrlEncodedContent"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<PostFormResult> PostAsync(Uri requestUri, FormUrlEncodedContent formUrlEncodedContent, CancellationToken? cancellationToken = null)
    {
        if (!IsAuthenticated)
        {
            LogServices.LogDebug("L'utilisateur n'est pas connecté");
            return new PostFormResult(false, null, "L'utilisateur n'est pas connecté");
        }
        
        try
        {
            using var result = await _httpClient.PostAsync(requestUri, formUrlEncodedContent, cancellationToken ?? CancellationToken.None);
            return new PostFormResult(result.IsSuccessStatusCode, result.StatusCode, result.ToString());
        }
        catch (Exception e)
        {
            LogServices.LogDebug(e);
            return new PostFormResult(false, null, e.Message);
        }
    }
    
    /// <summary>
    /// Soumet une requête POST à l'adresse relative spécifiée
    /// </summary>
    /// <param name="relativeRequestUrl"></param>
    /// <param name="formUrlEncodedContent"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<PostFormResult> PostAsync(string relativeRequestUrl, FormUrlEncodedContent formUrlEncodedContent, CancellationToken? cancellationToken = null)
    {
        if (!IsAuthenticated)
        {
            LogServices.LogDebug("L'utilisateur n'est pas connecté");
            return new PostFormResult(false, null, "L'utilisateur n'est pas connecté");
        }
        
        try
        {
            using var result = await _httpClient.PostAsync(relativeRequestUrl, formUrlEncodedContent, cancellationToken ?? CancellationToken.None);
            return new PostFormResult(result.IsSuccessStatusCode, result.StatusCode, result.ToString());
        }
        catch (Exception e)
        {
            LogServices.LogDebug(e);
            return new PostFormResult(false, null, e.Message);
        }
    }
    
    private void ReleaseUnmanagedResources()
    {
        // TODO release unmanaged resources here
        _httpClient?.Dispose();
        _handler?.Dispose();
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~IcotakuConnexion()
    {
        ReleaseUnmanagedResources();
    }
}

public readonly struct PostFormResult
{
    public bool IsSucces { get; init; }
    public HttpStatusCode? StatusCode { get; init; }
    public string? Message { get; init; }

    public PostFormResult()
    {
        
    }

    public PostFormResult(bool isSucces, HttpStatusCode? statusCode, string? message)
    {
        IsSucces = isSucces;
        StatusCode = statusCode;
        Message = message;
    }
}