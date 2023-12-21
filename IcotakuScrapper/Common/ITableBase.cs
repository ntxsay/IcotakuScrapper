namespace IcotakuScrapper.Common;

public interface ITableBase
{
    /// <summary>
    /// Obtient ou définit l'identifiant de base de données de l'objet.
    /// </summary>
    public int Id { get;  }
}

public interface ITableSheetBase : ITableBase
{
    /// <summary>
    /// Obtient ou définit l'identifiant de la fiche Icotaku.
    /// </summary>
    public int SheetId { get; protected set; }
}