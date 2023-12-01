﻿namespace IcotakuScrapper
{
    /// <summary>
    /// Afin d'éviter de créer plusieurs méthodes de sélection, cette énumération permet de sélectionner la colonne à utiliser pour la sélection.
    /// </summary>
    public enum SheetIntColumnSelect
    {
        Id,
        SheetId,
    }
    
    /// <summary>
    /// Enumération des colonnes d'id permettant de sélectionner ou de compter  des enrehistrements via l'id ou l'idAnime.
    /// </summary>
    public enum SelectCountIdIdAnimeKind
    {
        Id,
        IdAnime,
    }
    
    public enum SelectCountIdIdAnimeSheetIdKind
    {
        Id,
        IdAnime,
        SheetId
    }
    
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
    
    public enum SheetType : byte
    {
        Unknown,
        Anime,
        Person,
        Character,
        Studio,
        Distributor,
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
