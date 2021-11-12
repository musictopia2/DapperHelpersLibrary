namespace DapperHelpersLibrary.Extensions;
public static class GetOneItem
{
    public static R GetSingleObject<E, R>(this IDbConnection db, string property, BasicList<SortInfo> sortList, IDbConnector conn, BasicList<ICondition>? conditions = null, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class
    {
        BasicList<DatabaseMapping> thisList = GetMappingList<E>(out string TableName);
        EnumDatabaseCategory database = db.GetDatabaseCategory(conn);
        var (sqls, ParameterMappings) = GetConditionalStatement(thisList, TableName, conditions, sortList, database, howMany: 1, property: property);
        DapperSQLData thisData = new();
        thisData.SQLStatement = sqls;
        if (conditions != null)
        {
            PopulateSimple(ParameterMappings, thisData, EnumCategory.Conditional);
        }
        return db.ExecuteScalar<R>(thisData.SQLStatement, thisData.Parameters, thisTran, connectionTimeOut);
    }
    public static async Task<R> GetSingleObjectAsync<E, R>(this IDbConnection db, string property, BasicList<SortInfo> sortList, IDbConnector conn, BasicList<ICondition>? conditions = null, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class
    {
        BasicList<DatabaseMapping> thisList = GetMappingList<E>(out string TableName);
        EnumDatabaseCategory database = db.GetDatabaseCategory(conn);
        var (sqls, ParameterMappings) = GetConditionalStatement(thisList, TableName, conditions, sortList, database, howMany: 1, property: property);
        DapperSQLData thisData = new();
        thisData.SQLStatement = sqls;
        if (conditions != null)
        {
            PopulateSimple(ParameterMappings, thisData, EnumCategory.Conditional);
        }
        return await db.ExecuteScalarAsync<R>(thisData.SQLStatement, thisData.Parameters, thisTran, connectionTimeOut);
    }
    public static BasicList<R> GetObjectList<E, R>(this IDbConnection db, string property, IDbConnector conn, BasicList<ICondition>? conditions = null, BasicList<SortInfo>? sortList = null, int howMany = 0, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class
    {
        BasicList<DatabaseMapping> list = GetMappingList<E>(out string TableName);
        EnumDatabaseCategory database = db.GetDatabaseCategory(conn);
        var (sqls, ParameterMappings) = GetConditionalStatement(list, TableName, conditions, sortList, database, howMany: howMany, property: property);
        DapperSQLData data = new();
        data.SQLStatement = sqls;
        if (conditions != null)
        {
            PopulateSimple(ParameterMappings, data, EnumCategory.Conditional);
        }
        return db.Query<R>(data.SQLStatement, data.Parameters, thisTran, commandTimeout: connectionTimeOut).ToBasicList();
    }
    public static async Task<BasicList<R>> GetObjectListAsync<E, R>(this IDbConnection db, string property, IDbConnector conn, BasicList<ICondition>? Conditions = null, BasicList<SortInfo>? sortList = null, int howMany = 0, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class
    {
        BasicList<DatabaseMapping> thisList = GetMappingList<E>(out string TableName);
        EnumDatabaseCategory database = db.GetDatabaseCategory(conn);
        var (sqls, ParameterMappings) = GetConditionalStatement(thisList, TableName, Conditions, sortList, database, howMany: howMany, property: property);
        DapperSQLData thisData = new();
        thisData.SQLStatement = sqls;
        if (Conditions != null)
        {
            PopulateSimple(ParameterMappings, thisData, EnumCategory.Conditional);
        }
        var temps = await db.QueryAsync<R>(thisData.SQLStatement, thisData.Parameters, thisTran, commandTimeout: connectionTimeOut);
        return temps.ToBasicList();
    }
}