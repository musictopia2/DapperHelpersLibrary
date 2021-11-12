namespace DapperHelpersLibrary.Extensions;
public static class BulkInsertDelete
{
    #region Insert
    private static DapperSQLData GetDapperInsert(EnumDatabaseCategory category, BasicList<DatabaseMapping> thisList, string tableName, bool isAutoIncremented)
    {
        DapperSQLData output = new();
        output.SQLStatement = GetInsertStatement(category, thisList, tableName, isAutoIncremented);
        PopulateSimple(thisList, output, EnumCategory.UseDatabaseMapping);
        return output;
    }
    private static DapperSQLData GetDapperInsert<E>(EnumDatabaseCategory category, E thisObj) where E : class
    {
        bool isAutoIncremented = thisObj.GetType().IsAutoIncremented();
        BasicList<DatabaseMapping> ThisList = GetMappingList(thisObj, out string TableName, isAutoIncremented);
        return GetDapperInsert(category, ThisList, TableName, isAutoIncremented);
    }
    public static void InsertRange<E>(this IDbConnection db, BasicList<E> thisList, IDbTransaction thisTran, IDbConnector conn, int? connectionTimeOut = null) where E : class, ISimpleDapperEntity
    {
        EnumDatabaseCategory category = db.GetDatabaseCategory(conn);
        thisList.ForEach(items =>
        {
            var thisData = GetDapperInsert(category, items);
            db.PrivateInsertBulk(thisData, thisTran, connectionTimeOut);
        });
    }
    private static void PrivateInsertBulk(this IDbConnection db, DapperSQLData thisData, IDbTransaction thisTran, int? connectionTimeOut = null)
    {
        db.Execute(thisData.SQLStatement, thisData.Parameters, thisTran, commandTimeout: connectionTimeOut);
    }
    private async static Task<int> PrivateInsertBulkAsync(this IDbConnection db, DapperSQLData thisData, IDbTransaction thisTran, int? connectionTimeOut = null)
    {
        return await db.ExecuteScalarAsync<int>(thisData.SQLStatement, thisData.Parameters, thisTran, commandTimeout: connectionTimeOut);
    }
    public static async Task InsertRangeAsync<E>(this IDbConnection db, BasicList<E> thisList, IDbTransaction thisTran, IDbConnector conn, int? connectionTimeOut = null) where E : class, ISimpleDapperEntity
    {
        EnumDatabaseCategory category = db.GetDatabaseCategory(conn);
        await thisList.ForEachAsync(async Items =>
        {
            var ThisData = GetDapperInsert(category, Items);
            Items.ID = await db.PrivateInsertBulkAsync(ThisData, thisTran, connectionTimeOut);
        });
    }
    #endregion
    #region Delete
    public static void DeleteRange<E>(this IDbConnection db, BasicList<int> deleteList, IDbTransaction thisTran, int? connectionTimeOut = null) where E : class
    {
        deleteList.ForEach(Items =>
        {
            DapperSQLData dapper = PrivateDeleteSingleItem<E>(Items);
            db.Execute(dapper.SQLStatement, dapper.Parameters, thisTran, connectionTimeOut);
        });
    }
    public static async Task DeleteRangeAsync<E>(this IDbConnection db, BasicList<int> deleteList, IDbTransaction thisTran, int? connectionTimeOut = null) where E : class
    {
        await deleteList.ForEachAsync(async items =>
        {
            DapperSQLData dapper = PrivateDeleteSingleItem<E>(items);
            await db.ExecuteAsync(dapper.SQLStatement, dapper.Parameters, thisTran, connectionTimeOut);
        });
    }
    public static void DeleteRange<E>(this IDbConnection db, BasicList<E> objectList, IDbTransaction thisTran, int? connectionTimeOut = null) where E : class, ISimpleDapperEntity
    {
        BasicList<int> deleteList = objectList.GetIDList();
        deleteList.ForEach(items =>
        {
            DapperSQLData dapper = PrivateDeleteSingleItem<E>(items);
            db.Execute(dapper.SQLStatement, dapper.Parameters, thisTran, connectionTimeOut);
        });
    }
    public static async Task DeleteRangeAsync<E>(this IDbConnection db, BasicList<E> objectList, IDbTransaction thisTran, int? connectionTimeOut = null) where E : class, ISimpleDapperEntity
    {
        BasicList<int> deleteList = objectList.GetIDList();
        await deleteList.ForEachAsync(async items =>
        {
            DapperSQLData dapper = PrivateDeleteSingleItem<E>(items);
            await db.ExecuteAsync(dapper.SQLStatement, dapper.Parameters, thisTran, connectionTimeOut);
        });
    }
    #endregion
}