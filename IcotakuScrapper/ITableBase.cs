using Microsoft.Data.Sqlite;

namespace IcotakuScrapper;

public interface ITableBase<T>  where T : class
{
    /// <summary>
    /// Obtient ou définit l'identifiant de base de données de l'objet
    /// </summary>
    public int Id { get;  }

    /*/// <summary>
    /// Retourne une valeur booléenne indiquant si l'objet <typeparamref name="T"/>.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public static abstract Task<T?> SingleAsync(int id, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null);*/
}

public interface ITableSheetBase<T> : ITableBase<T> where T : class
{
    /// <summary>
    /// Obtient ou définit l'identifiant de la fiche Icotaku.
    /// </summary>
    public int SheetId { get; protected set; }

    /// <summary>
    /// Retourne l'objet <typeparamref name="T"/> si elle existe dans la base de données.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="columnSelect"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public static abstract Task<T?> SingleAsync(int id, IntColumnSelect columnSelect,
        CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null);

    /// <summary>
    /// Retourne l'objet <typeparamref name="T"/> si elle existe dans la base de données.
    /// </summary>
    /// <param name="sheetUri"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public static abstract Task<T?> SingleAsync(Uri sheetUri, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null);
}