using CommonBasicStandardLibraries.AdvancedGeneralFunctionsAndProcesses.BasicExtensions;
using CommonBasicStandardLibraries.CollectionClasses;
using CommonBasicStandardLibraries.DatabaseHelpers.ConditionClasses;
using CommonBasicStandardLibraries.DatabaseHelpers.MiscClasses;
using CommonBasicStandardLibraries.DatabaseHelpers.MiscInterfaces;
using Dapper;
using DapperHelpersLibrary.MapHelpers;
using System.Data;
using System.Threading.Tasks; //most of the time, i will be using asyncs.
using static DapperHelpersLibrary.MapHelpers.MapBaseHelperClass;
using static DapperHelpersLibrary.SQLHelpers.PopulateDynamics;
using static DapperHelpersLibrary.SQLHelpers.StatementFactoryConditionsSingle;
namespace DapperHelpersLibrary.Extensions
{
    public static class GetOneItem //will not have any joins.
    {
        public static R GetSingleObject<E, R>(this IDbConnection db, string property, CustomBasicList<SortInfo> sortList, IDbConnector conn, CustomBasicList<ICondition>? conditions = null, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class
        {
            CustomBasicList<DatabaseMapping> thisList = GetMappingList<E>(out string TableName);
            EnumDatabaseCategory database = db.GetDatabaseCategory(conn);
            var (sqls, ParameterMappings) = GetConditionalStatement(thisList, TableName, conditions, sortList, database, howMany: 1, property: property);
            DapperSQLData thisData = new DapperSQLData();
            thisData.SQLStatement = sqls;
            if (conditions != null)
                PopulateSimple(ParameterMappings, thisData, EnumCategory.Conditional); //if no conditions, then no need.
            return db.ExecuteScalar<R>(thisData.SQLStatement, thisData.Parameters, thisTran, connectionTimeOut);
        }
        public static async Task<R> GetSingleObjectAsync<E, R>(this IDbConnection db, string property, CustomBasicList<SortInfo> sortList, IDbConnector conn, CustomBasicList<ICondition>? conditions = null, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class
        {
            CustomBasicList<DatabaseMapping> thisList = GetMappingList<E>(out string TableName);
            EnumDatabaseCategory database = db.GetDatabaseCategory(conn);
            var (sqls, ParameterMappings) = GetConditionalStatement(thisList, TableName, conditions, sortList, database, howMany: 1, property: property);
            DapperSQLData thisData = new DapperSQLData();
            thisData.SQLStatement = sqls;
            if (conditions != null)
                PopulateSimple(ParameterMappings, thisData, EnumCategory.Conditional); //if no conditions, then no need.
            return await db.ExecuteScalarAsync<R>(thisData.SQLStatement, thisData.Parameters, thisTran, connectionTimeOut);
        }
        public static CustomBasicList<R> GetObjectList<E, R>(this IDbConnection db, string property, IDbConnector conn, CustomBasicList<ICondition>? conditions = null, CustomBasicList<SortInfo>? sortList = null, int howMany = 0, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class
        {
            CustomBasicList<DatabaseMapping> ThisList = GetMappingList<E>(out string TableName);
            EnumDatabaseCategory Database = db.GetDatabaseCategory(conn);
            var (sqls, ParameterMappings) = GetConditionalStatement(ThisList, TableName, conditions, sortList, Database, howMany: howMany, property: property);
            DapperSQLData ThisData = new DapperSQLData();
            ThisData.SQLStatement = sqls;
            if (conditions != null)
                PopulateSimple(ParameterMappings, ThisData, EnumCategory.Conditional); //if no conditions, then no need.
            return db.Query<R>(ThisData.SQLStatement, ThisData.Parameters, thisTran, commandTimeout: connectionTimeOut).ToCustomBasicList();//hopefully that just works
        }
        public static async Task<CustomBasicList<R>> GetObjectListAsync<E, R>(this IDbConnection db, string property, IDbConnector conn, CustomBasicList<ICondition>? Conditions = null, CustomBasicList<SortInfo>? sortList = null, int howMany = 0, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class
        {
            CustomBasicList<DatabaseMapping> thisList = GetMappingList<E>(out string TableName);
            EnumDatabaseCategory database = db.GetDatabaseCategory(conn);
            var (sqls, ParameterMappings) = GetConditionalStatement(thisList, TableName, Conditions, sortList, database, howMany: howMany, property: property);
            DapperSQLData thisData = new DapperSQLData();
            thisData.SQLStatement = sqls;
            if (Conditions != null)
                PopulateSimple(ParameterMappings, thisData, EnumCategory.Conditional); //if no conditions, then no need.
            var temps = await db.QueryAsync<R>(thisData.SQLStatement, thisData.Parameters, thisTran, commandTimeout: connectionTimeOut);
            return temps.ToCustomBasicList();
        }
    }
}