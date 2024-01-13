using HtmlAgilityPack;
using IcotakuScrapper.Extensions;

namespace IcotakuScrapper.Objects;

/// <summary>
/// Enumération des types d'éléments d'un répertoire de serveur web.
/// </summary>
public enum WebServerItemType
{
    /// <summary>
    /// L'élément est inconnu.
    /// </summary>
    Unknown,
    
    /// <summary>
    /// L'élément est un fichier.
    /// </summary>
    File,
    
    /// <summary>
    /// L'élément est un répertoire.
    /// </summary>
    Directory
}

/// <summary>
/// Classe permettant de récupérer les informations d'un répertoire de serveur web.
/// </summary>
public class WebServerDirectoryIndex
{
    private bool _isLoaded;
    private bool _isRootDirectory;
    private uint _rootDistance;
    private bool _isWebServerDirectoryUrl;
    private Uri _baseUri;
    private Uri? _parentUri;
    private List<WebServerDirectoryContent> _directoryContents = [];
    private List<WebServerDirectoryIndex> _subDirectories = [];
    private WebServerDirectoryIndex? _parentDirectory;
    private HtmlDocument? _document;

    /// <summary>
    /// Obtient une valeur indiquant si le répertoire courant est le répertoire racine.
    /// </summary>
    public bool IsRootDirectory => _isRootDirectory;
    
    /// <summary>
    /// Obtient la distance entre le répertoire courant et le répertoire root.
    /// </summary>
    public uint RootDistance => _rootDistance;

    /// <summary>
    /// Obtient une valeur indiquant si l'URL est un répertoire de serveur web.
    /// </summary>
    public bool IsWebServerDirectoryUrl => _isWebServerDirectoryUrl;
    
    /// <summary>
    /// Obtient l'URI du répertoire actuel.
    /// </summary>
    public Uri BaseUri => _baseUri;
    
    /// <summary>
    /// Obtient l'URI du répertoire parent.
    /// </summary>
    public Uri? ParentUri => _parentUri;
    
    /// <summary>
    /// Obtient les sous-répertoires du répertoire actuel.
    /// </summary>
    public WebServerDirectoryIndex[] SubDirectories => _subDirectories.ToArray();
    
    /// <summary>
    /// Obtient le répertoire parent du répertoire actuel.
    /// </summary>
    public WebServerDirectoryIndex? ParentDirectory => _parentDirectory;
    
    /// <summary>
    /// Obtient une valeur indiquant si le répertoire actuel contient des sous-répertoires ou des fichiers.
    /// </summary>
    public bool HasChildrens => _directoryContents.Count > 0;
    
    /// <summary>
    /// Obtient une valeur indiquant si le répertoire actuel contient des sous-répertoires.
    /// </summary>
    public bool HasDirectories => _directoryContents.Any(x => x.Type == WebServerItemType.Directory);
    
    /// <summary>
    /// Obtient une valeur indiquant si le répertoire actuel contient des fichiers.
    /// </summary>
    public bool HasFiles => _directoryContents.Any(x => x.Type == WebServerItemType.File);
    
    /// <summary>
    /// Obtient le contenu du répertoire.
    /// </summary>
    public WebServerDirectoryContent[] DirectoryContents => _directoryContents.ToArray();
    
    /// <summary>
    /// Initialise une nouvelle instance de la classe <see cref="WebServerDirectoryIndex"/>.
    /// </summary>
    /// <param name="uri">Url absolue du répertoire</param>
    public WebServerDirectoryIndex(Uri uri)
    {
        _baseUri = uri;
    }

    /// <summary>
    /// Charge les sous-répertoires du répertoire actuel.
    /// </summary>
    public async Task LoadSubDirectoryAsync()
    {
        if (!_isLoaded)
            return;

        foreach (var uri in DirectoryContents.Where(s => s.Type == WebServerItemType.Directory).Select(s => s.Uri))
        {
            var directory = new WebServerDirectoryIndex(uri);
            await directory.LoadAsync();
            if (!directory.IsWebServerDirectoryUrl)
                continue;
            
            _subDirectories.Add(directory);
        }
    }
    
    /// <summary>
    /// Charge le répertoire parent du répertoire actuel.
    /// </summary>
    public async Task LoadParentDirectoryAsync()
    {
        if (!_isLoaded)
            return;

        if (_parentUri == null)
            return;
        
        var directory = new WebServerDirectoryIndex(_parentUri);
        await directory.LoadAsync();
        if (!directory.IsWebServerDirectoryUrl)
            return;
        
        _parentDirectory = directory;
    }
    
    /// <summary>
    /// Charge les informations du répertoire actuel.
    /// </summary>
    public async Task LoadAsync()
    {
        _isLoaded = false;
        HtmlWeb web = new();
        _document = await web.LoadFromWebAsync(BaseUri.ToString());

        var titleNode = _document.DocumentNode.SelectSingleNode("//title");
        if (titleNode == null || !titleNode.InnerText.StartsWith("Index of /", StringComparison.OrdinalIgnoreCase))
        {
            _isWebServerDirectoryUrl = false;
            _isLoaded = true;
            return;
        }

        _isWebServerDirectoryUrl = true;
        GetParentLevel();
        GetParentDirectoryUri();
        _directoryContents = GetContent().ToList();
        _isLoaded = true;
    }
    
    /// <summary>
    /// Cherche la distance entre le répertoire courant et le répertoire racine.
    /// </summary>
    private void GetParentLevel()
    {
        var title = _document?.DocumentNode.SelectSingleNode("//title");
        if (title == null) 
            return;
        var titleText = title.InnerText.Replace("Index of /", "", StringComparison.InvariantCultureIgnoreCase).Trim();
        var splitPath = titleText.Split("/", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            
        _rootDistance = (uint)splitPath.Length;
        if (splitPath.Length == 1)
            _isRootDirectory = true;
    }

    /// <summary>
    /// Cherche l'URI du répertoire parent.
    /// </summary>
    private void GetParentDirectoryUri()
    {
        var aNode = _document?.DocumentNode.SelectSingleNode("//table//a[text()='Parent Directory']");
        if (aNode == null) 
            return;
        var href = aNode.GetAttributeValue("href", "");
        if (string.IsNullOrEmpty(href)) 
            return;

        try
        {
            var uri = new Uri(BaseUri, href);
            _parentUri = uri;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    /// <summary>
    /// Recherche le contenu du répertoire.
    /// </summary>
    /// <returns></returns>
    private IEnumerable<WebServerDirectoryContent> GetContent()
    {
        var trNodes = _document?.DocumentNode.SelectNodes("//table//tr")?.ToArray();
        if (trNodes == null || trNodes.Length == 0) 
            yield break;

        for (var i = 3; i < trNodes.Length; i++)
        {
            var trNode = trNodes[i];
            var tdNodes = trNode.SelectNodes("td")?.ToArray();
            if (tdNodes is not { Length: 5 }) 
                continue;
            
            var type = GetItemType(tdNodes[0]);
            var (name, uri) = GetItemName(tdNodes[1]);
            if (name == null || uri == null)
                continue;

            yield return new WebServerDirectoryContent()
            {
                Type = type,
                Name = name,
                Uri = uri
            };
        }
    }
    
    /// <summary>
    /// Recherche le type de l'élément.
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    private WebServerItemType GetItemType(HtmlNode node)
    {
        var imgNode = node.SelectSingleNode("./img");
        if (imgNode == null)
            return WebServerItemType.Unknown;

        var src = imgNode.GetAttributeValue("src", "");
        var alt = imgNode.GetAttributeValue("alt", "");
        if (string.IsNullOrEmpty(src))
            return WebServerItemType.Unknown;

        if (src.Contains("folder.gif", StringComparison.OrdinalIgnoreCase) || alt.Equals("[DIR]", StringComparison.OrdinalIgnoreCase))
            return WebServerItemType.Directory;
        if (src.Contains("image2.gif", StringComparison.OrdinalIgnoreCase) || alt.Equals("[IMG]", StringComparison.OrdinalIgnoreCase))
            return WebServerItemType.File;
        return WebServerItemType.Unknown;
    }
    
    /// <summary>
    /// Recherche le nom et l'URI de l'élément.
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    private (string? Name, Uri? Uri) GetItemName(HtmlNode node)
    {
        var aNode = node.SelectSingleNode("./a");
        if (aNode == null)
            return (null, null);
        
        var href = aNode.GetAttributeValue("href", "");
        if (href.IsStringNullOrEmptyOrWhiteSpace())
            return (null, null);
        
        try
        {
            var uri = new Uri(BaseUri, href);
            return (aNode.InnerText, uri);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return (null, null); 
        }
    }
}

/// <summary>
/// Représente un élément d'un répertoire de serveur web.
/// </summary>
public readonly struct WebServerDirectoryContent
{
    /// <summary>
    /// Obtient le type de l'élément.
    /// </summary>
    public WebServerItemType Type { get; init; }
    
    /// <summary>
    /// Obtient le nom de l'élément.
    /// </summary>
    public string Name { get; init; }
    
    /// <summary>
    /// Obtient l'URI de l'élément.
    /// </summary>
    public Uri Uri { get; init; }
}