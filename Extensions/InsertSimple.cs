namespace DapperHelpersLibrary.Extensions;
public static class InsertSimple
{
    private static DapperSQLData GetDapperInsert(EnumDatabaseCategory category, BasicList<DatabaseMapping> thisList, string tableName, bool isAutoIncremented)
    {
        DapperSQLData output = new();
        output.SQLStatement = GetInsertStatement(category, thisList, tableName, isAutoIncremented);
        PopulateSimple(thisList, output, EnumCategory.UseDatabaseMapping);
        return output;
    }
    private static DapperSQLData GetDapperInsert<E>(E thisObj, EnumDatabaseCategory category, out bool isAutoIncremented) where E : class
    {
        isAutoIncremented = thisObj.GetType().IsAutoIncremented();
        BasicList<DatabaseMapping> ThisList = GetMappingList(thisObj, out string TableName, isAutoIncremented);
        return GetDapperInsert(category, ThisList, TableName, isAutoIncremented);
    }
    private static int PrivateInsertSingle(this IDbConnection db, DapperSQLData thisData, IDbTransaction? thisTran = null, int? connectionTimeOut = null)
    {
        return db.Query<int>(thisData.SQLStatement, thisData.Parameters, thisTran, commandTimeout: connectionTimeOut).FirstOrDefault();
    }
    private async static Task<int> PrivateInsertSingleAsync(this IDbConnection db, DapperSQLData thisData, IDbTransaction? thisTran = null, int? connectionTimeOut = null)
    {
        var temps = await db.QueryAsync<int>(thisData.SQLStatement, thisData.Parameters, thisTran, commandTimeout: connectionTimeOut);
        return temps.FirstOrDefault();
    }
    public static int InsertSingle<E>(this IDbConnection db, E thisObject, IDbConnector conn, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class, ISimpleDapperEntity
    {
        EnumDatabaseCategory category = db.GetDatabaseCategory(conn);
        var thisData = GetDapperInsert(thisObject, category, out bool IsAutoIncremented);
        int id = db.PrivateInsertSingle(thisData, thisTran, connectionTimeOut);
        if (IsAutoIncremented == true)
        {
            return id;
        }
        return thisObject.ID;
    }
    public static async Task<int> InsertSingleAsync<E>(this IDbConnection db, E thisObject, IDbConnector conn, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class, ISimpleDapperEntity
    {
        EnumDatabaseCategory category = db.GetDatabaseCategory(conn);
        var thisData = GetDapperInsert(thisObject, category, out bool IsAutoIncremented);
        int id = await db.PrivateInsertSingleAsync(thisData, thisTran, connectionTimeOut);
        if (IsAutoIncremented == true)
        {
            return id;
        }
        return thisObject.ID;
    }
}