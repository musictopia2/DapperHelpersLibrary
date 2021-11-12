namespace DapperHelpersLibrary.Extensions;
public static class CountAndExist
{
    public static int Count<E>(this IDbConnection db, BasicList<ICondition> conditions, IDbConnector conn, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class
    {
        EnumDatabaseCategory database = db.GetDatabaseCategory(conn); //unfortunately, i need one more parameter now though.
        BasicList<DatabaseMapping> thisList = GetMappingList<E>(out string TableName);
        var (sqls, ParameterMappings) = GetConditionalStatement(thisList, TableName, conditions, null, database, EnumSQLCategory.Count);
        DapperSQLData thisData = new();
        thisData.SQLStatement = sqls;
        PopulateSimple(ParameterMappings, thisData, EnumCategory.Conditional);
        return db.ExecuteScalar<int>(sqls, thisData.Parameters, thisTran, connectionTimeOut);
    }
    public static int Count<E>(this IDbConnection db, IDbConnector conn, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class
    {
        BasicList<DatabaseMapping> ThisList = GetMappingList<E>(out string TableName);
        EnumDatabaseCategory Database = db.GetDatabaseCategory(conn);
        string sqls = GetSimpleSelectStatement(ThisList, TableName, Database, EnumSQLCategory.Count);
        return db.ExecuteScalar<int>(sqls, null, thisTran, connectionTimeOut);
    }
    public static bool Exists<E>(this IDbConnection db, BasicList<ICondition> conditions, IDbConnector conn, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class
    {
        BasicList<DatabaseMapping> thisList = GetMappingList<E>(out string TableName);
        EnumDatabaseCategory database = db.GetDatabaseCategory(conn);
        var (sqls, ParameterMappings) = GetConditionalStatement(thisList, TableName, conditions, null, database, EnumSQLCategory.Bool);
        DapperSQLData thisData = new();
        thisData.SQLStatement = sqls;
        PopulateSimple(ParameterMappings, thisData, EnumCategory.Conditional);
        return db.ExecuteScalar<bool>(sqls, thisData.Parameters, thisTran, connectionTimeOut);
    }
    public static bool Exists<E>(this IDbConnection db, int id, IDbConnector conn, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class
    {
        BasicList<ICondition> ThisList = StartConditionWithID(id);
        return db.Exists<E>(ThisList, conn, thisTran, connectionTimeOut);
    }
}