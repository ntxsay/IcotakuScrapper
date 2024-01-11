using Microsoft.Data.Sqlite;

namespace IcotakuScrapper.Common;

public partial class TuserSheetNotation : ITableSheetBase<TuserSheetNotation>
{
    public int Id { get; protected set; }
    public int SheetId { get; set; }
    public int? IdAnime { get; set; }
    public IcotakuSection Section { get; set; }
    public WatchStatusKind WatchStatus { get; set; }
    public float? Note { get; set; }
    public string? PublicComment { get; set; }
    public string? PrivateComment { get; set; }

    #region Count

    public static Task<int> CountAsync(CancellationToken? cancellationToken = null)
    {
        throw new NotImplementedException();
    }

    public static Task<int> CountAsync(int id, CancellationToken? cancellationToken = null)
    {
        throw new NotImplementedException();
    }
    
    public static Task<int> CountAsync(int id, IntColumnSelect columnSelect, CancellationToken? cancellationToken = null)
    {
        throw new NotImplementedException();
    }
    
    #endregion



    #region Exists

    public static Task<bool> ExistsByIdAsync(int id, CancellationToken? cancellationToken = null)
    {
        throw new NotImplementedException();
    }

    public static Task<bool> ExistsBySheetIdAsync(int sheetId, CancellationToken? cancellationToken = null)
    {
        throw new NotImplementedException();
    }

    public static Task<bool> ExistsAsync(int id, IntColumnSelect columnSelect, CancellationToken? cancellationToken = null)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region Single

    public static Task<TuserSheetNotation?> SingleByIdAsync(int id, CancellationToken? cancellationToken = null)
    {
        throw new NotImplementedException();
    }

    public static Task<TuserSheetNotation?> SingleBySheetIdAsync(int sheetId, CancellationToken? cancellationToken = null)
    {
        throw new NotImplementedException();
    }

    public static Task<TuserSheetNotation?> SingleAsync(int id, IntColumnSelect columnSelect, CancellationToken? cancellationToken = null)
    {
        throw new NotImplementedException();
    }

    public static Task<TuserSheetNotation?> SingleAsync(Uri sheetUri, CancellationToken? cancellationToken = null)
    {
        throw new NotImplementedException();
    }

    #endregion

    public Task<OperationState<int>> InsertAsync(bool disableVerification = false, CancellationToken? cancellationToken = null)
    {
        throw new NotImplementedException();
    }

    public Task<OperationState> UpdateAsync(bool disableVerification, CancellationToken? cancellationToken = null)
    {
        throw new NotImplementedException();
    }

    public Task<OperationState> DeleteAsync(CancellationToken? cancellationToken = null)
    {
        throw new NotImplementedException();
    }

    public static Task<TuserSheetNotation?> SingleOrCreateAsync(TuserSheetNotation value, bool reloadIfExist = false,
        CancellationToken? cancellationToken = null)
    {
        throw new NotImplementedException();
    }
    
    public static Task<OperationState> DeleteAsync(int id, IntColumnSelect columnSelect, CancellationToken? cancellationToken = null)
    {
        throw new NotImplementedException();
    }

    public static Task<OperationState> DeleteAsync(Uri uri, CancellationToken? cancellationToken = null)
    {
        throw new NotImplementedException();
    }
    
    private static async IAsyncEnumerable<TuserSheetNotation> GetRecords(SqliteDataReader reader,
        CancellationToken? cancellationToken = null)
    {
        while (await reader.ReadAsync(cancellationToken ?? CancellationToken.None))
        {
            yield return new TuserSheetNotation()
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                SheetId = reader.GetInt32(reader.GetOrdinal("SheetId")),
                Section = (IcotakuSection)reader.GetByte(reader.GetOrdinal("Section")),
                WatchStatus = (WatchStatusKind)reader.GetByte(reader.GetOrdinal("WatchingStatus")),
                Note = reader.IsDBNull(reader.GetOrdinal("Note"))
                    ? null
                    : reader.GetFloat(reader.GetOrdinal("Note")),
                PublicComment = reader.IsDBNull(reader.GetOrdinal("PublicComment"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("PublicComment")),
                PrivateComment = reader.IsDBNull(reader.GetOrdinal("PrivateComment"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("PrivateComment")),
                IdAnime = reader.IsDBNull(reader.GetOrdinal("IdAnime"))
                    ? null
                    : reader.GetInt32(reader.GetOrdinal("IdAnime")),
            };
        }
    }
    
    private const string IcotakuSqlSelectScript =
        """
        SELECT
            Id,
            IdAnime,
            SheetId,
            Section,
            WatchingStatus,
            Note,
            PublicComment,
            PrivateComment
        FROM TuserSheetNotation
        """;
}