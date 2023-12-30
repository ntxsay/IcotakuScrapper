using Microsoft.Data.Sqlite;

namespace IcotakuScrapper;

public interface ITableBase<T>  where T : class
{
    /// <summary>
    /// Obtient ou définit l'identifiant de base de données de l'objet
    /// </summary>
    public int Id { get;  }

    /// <summary>
    /// Compte le nombre d'objet <typeparamref name="T"/> dans la base de données.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static abstract Task<int> CountAsync(int id, CancellationToken? cancellationToken = null);
    
    /// <summary>
    /// Insère l'objet <typeparamref name="T"/> dans la base de données.
    /// </summary>
    /// <param name="disableVerification">Active ou désactive la validation en base de données avant l'insertion</param>
    /// <param name="cancellationToken">Permet d'annuler l'opération</param>
    /// <param name="cmd">Permet d'utiliser un objet <see cref="SqliteCommand"/> extérieur à la méthode</param>
    /// <returns></returns>
    public Task<OperationState<int>> InsertAsync(bool disableVerification = false,
        CancellationToken? cancellationToken = null);

    /// <summary>
    /// Met à jour l'objet <typeparamref name="T"/> dans la base de données.
    /// </summary>
    /// <param name="disableVerification"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<OperationState> UpdateAsync(bool disableVerification, CancellationToken? cancellationToken = null);

    /*/// <summary>
    /// Retourne une valeur booléenne indiquant si l'objet <typeparamref name="T"/>.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public static abstract Task<T?> SingleAsync(int id, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null);*/

    /// <summary>
    /// Retourne l'ojet <typeparamref name="T"/> si elle existe dans la base de données ou l'insère si elle n'existe pas.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="reloadIfExist"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static abstract Task<T?> SingleOrCreateAsync(T value, bool reloadIfExist = false,
        CancellationToken? cancellationToken = null);
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

public interface ITableNameDescriptionBase<T> : ITableBase<T> where T : class
{
    /// <summary>
    /// Obtient ou définit le nom de l'objet <typeparamref name="T"/>.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Obtient ou définit la description de l'objet <typeparamref name="T"/>.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Compte le nombre d'objet <typeparamref name="T"/> dans la base de données.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="section"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static abstract Task<int> CountAsync(string name, IcotakuSection section,
        CancellationToken? cancellationToken = null);
    
    /// <summary>
    /// Vérifie si l'objet <typeparamref name="T"/> existe dans la base de données.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static abstract Task<bool> ExistsAsync(int id, CancellationToken? cancellationToken = null);
    
    /// <summary>
    /// Vérifie si l'objet <typeparamref name="T"/> existe dans la base de données.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="section"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static abstract Task<bool> ExistsAsync(string name, IcotakuSection section,
        CancellationToken? cancellationToken = null);
    
    /// <summary>
    /// Retourne l'objet <typeparamref name="T"/> si elle existe dans la base de données.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static abstract Task<T?> SingleAsync(int id, CancellationToken? cancellationToken = null);
    
    /// <summary>
    /// Retourne l'objet <typeparamref name="T"/> si elle existe dans la base de données.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="section"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static abstract Task<T?> SingleAsync(string name, IcotakuSection section,
        CancellationToken? cancellationToken = null);
    
    public static abstract Task<int?> GetIdOfAsync(string name, IcotakuSection section,
        CancellationToken? cancellationToken = null);

    public static abstract Task<OperationState> InsertOrReplaceAsync(IReadOnlyCollection<T> values,
        DbInsertMode insertMode = DbInsertMode.InsertOrReplace, CancellationToken? cancellationToken = null);

    /*
    /// <summary>
    /// Convertit les données de l'objet <see cref="SqliteDataReader"/> en objet <typeparamref name="T"/>.
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    internal static abstract IAsyncEnumerable<T> GetRecords(SqliteDataReader reader,
        CancellationToken? cancellationToken = null);
*/
}