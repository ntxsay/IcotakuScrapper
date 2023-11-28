namespace IcotakuScrapper
{
    /// <summary>
    /// Enuméation des sections du site icotaku.com et exploité par l'API.
    /// </summary>
    public enum IcotakuSection : byte
    {
        Anime,
        Manga,
        LightNovel,
        Drama,
        Community,
    }
    
    /// <summary>
    /// Enumération des modes de tri.
    /// </summary>
    public enum OrderBy : byte
    {
        /// <summary>
        /// Tri les éléments par ordre croissant.
        /// </summary>
        Asc,
        
        /// <summary>
        /// Tri les éléments par ordre décroissant.
        /// </summary>
        Desc
    }

    /// <summary>
    /// Enumération des types de catégories.
    /// </summary>
    public enum CategoryType
    {
        /// <summary>
        /// Représente la catégorie "Theme".
        /// </summary>
        Theme,
        
        /// <summary>
        /// Représente la catégorie "Genre".
        /// </summary>
        Genre,
    }

    /// <summary>
    /// Représente un état de diffusion
    /// </summary>
    public enum DiffusionStateKind : byte
    {
        /// <summary>
        /// Diffusion inconnue
        /// </summary>
        Unknown,
        /// <summary>
        /// Diffusion à venir
        /// </summary>
        UpComing,
        /// <summary>
        /// Diffusion en cours
        /// </summary>
        InProgress,
        /// <summary>
        /// Diffusion terminée
        /// </summary>
        Completed,
        /// <summary>
        /// Diffusion en pause
        /// </summary>
        Paused,
        /// <summary>
        /// Diffusion arrêtée
        /// </summary>
        Stopped,
    }

    public enum ContactType : byte
    {
        Unknown,
        Person,
        Character,
        Studio,
        Distributor,
    }
    
    public enum MoisKind
    {
        Unknow,
        Janvier,
        Février,
        Mars,
        Avril,
        Mai,
        Juin,
        Juillet,
        Août,
        Septembre,
        Octobre,
        Novembre,
        Décembre
    }
}
