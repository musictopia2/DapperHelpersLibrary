namespace DapperHelpersLibrary.Extensions;
public static class GetConditionalSingleTable
{
    public static BasicList<E> Get<E>(this IDbConnection db, BasicList<ICondition> conditions, IDbConnector conn, BasicList<SortInfo>? sortList = null, int howMany = 0, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class
    {
        return db.PrivateSimpleSelectConditional<E>(conditions, conn, sortList, howMany, thisTran, connectionTimeOut).ToBasicList();
    }
    public async static Task<BasicList<E>> GetAsync<E>(this IDbConnection db, BasicList<ICondition> conditions, IDbConnector conn, BasicList<SortInfo>? sortList = null, int howMany = 0, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class
    {
        var temps = await db.PrivateSimpleSelectConditionalAsync<E>(conditions, conn, sortList, howMany, thisTran, connectionTimeOut);
        return temps.ToBasicList();
    }
    private static IEnumerable<E> PrivateSimpleSelectConditional<E>(this IDbConnection db, BasicList<ICondition> Conditions, IDbConnector conn, BasicList<SortInfo>? sortList = null, int howMany = 0, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class
    {
        BasicList<DatabaseMapping> thisList = GetMappingList<E>(out string TableName);
        EnumDatabaseCategory database = db.GetDatabaseCategory(conn);
        var (sqls, ParameterMappings) = GetConditionalStatement(thisList, TableName, Conditions, sortList, database, howMany: howMany);
        DapperSQLData thisData = new();
        thisData.SQLStatement = sqls;
        PopulateSimple(ParameterMappings, thisData, EnumCategory.Conditional);
        return db.Query<E>(thisData.SQLStatement, thisData.Parameters, thisTran, commandTimeout: connectionTimeOut);
    }
    private async static Task<IEnumerable<E>> PrivateSimpleSelectConditionalAsync<E>(this IDbConnection db, BasicList<ICondition> conditions, IDbConnector conn, BasicList<SortInfo>? sortList = null, int howMany = 0, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class
    {
        BasicList<DatabaseMapping> thisList = GetMappingList<E>(out string TableName);
        EnumDatabaseCategory database = db.GetDatabaseCategory(conn);
        var (sqls, ParameterMappings) = GetConditionalStatement(thisList, TableName, conditions, sortList, database, howMany: howMany);
        DapperSQLData thisData = new();
        thisData.SQLStatement = sqls;
        PopulateSimple(ParameterMappings, thisData, EnumCategory.Conditional);
        return await db.QueryAsync<E>(thisData.SQLStatement, thisData.Parameters, thisTran, commandTimeout: connectionTimeOut);

    }
}