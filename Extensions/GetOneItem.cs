using CommonBasicLibraries.AdvancedGeneralFunctionsAndProcesses.BasicExtensions;
using CommonBasicLibraries.CollectionClasses;
using CommonBasicLibraries.DatabaseHelpers.ConditionClasses;
using CommonBasicLibraries.DatabaseHelpers.MiscClasses;
using CommonBasicLibraries.DatabaseHelpers.MiscInterfaces;
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
        public static R GetSingleObject<E, R>(this IDbConnection db, string property, BasicList<SortInfo> sortList, IDbConnector conn, BasicList<ICondition>? conditions = null, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class
        {
            BasicList<DatabaseMapping> thisList = GetMappingList<E>(out string TableName);
            EnumDatabaseCategory database = db.GetDatabaseCategory(conn);
            var (sqls, ParameterMappings) = GetConditionalStatement(thisList, TableName, conditions, sortList, database, howMany: 1, property: property);
            DapperSQLData thisData = new ();
            thisData.SQLStatement = sqls;
            if (conditions != null)
            {
                PopulateSimple(ParameterMappings, thisData, EnumCategory.Conditional); //if no conditions, then no need.
            }
            return db.ExecuteScalar<R>(thisData.SQLStatement, thisData.Parameters, thisTran, connectionTimeOut);
        }
        public static async Task<R> GetSingleObjectAsync<E, R>(this IDbConnection db, string property, BasicList<SortInfo> sortList, IDbConnector conn, BasicList<ICondition>? conditions = null, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class
        {
            BasicList<DatabaseMapping> thisList = GetMappingList<E>(out string TableName);
            EnumDatabaseCategory database = db.GetDatabaseCategory(conn);
            var (sqls, ParameterMappings) = GetConditionalStatement(thisList, TableName, conditions, sortList, database, howMany: 1, property: property);
            DapperSQLData thisData = new ();
            thisData.SQLStatement = sqls;
            if (conditions != null)
            {
                PopulateSimple(ParameterMappings, thisData, EnumCategory.Conditional); //if no conditions, then no need.
            }
            return await db.ExecuteScalarAsync<R>(thisData.SQLStatement, thisData.Parameters, thisTran, connectionTimeOut);
        }
        public static BasicList<R> GetObjectList<E, R>(this IDbConnection db, string property, IDbConnector conn, BasicList<ICondition>? conditions = null, BasicList<SortInfo>? sortList = null, int howMany = 0, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class
        {
            BasicList<DatabaseMapping> list = GetMappingList<E>(out string TableName);
            EnumDatabaseCategory database = db.GetDatabaseCategory(conn);
            var (sqls, ParameterMappings) = GetConditionalStatement(list, TableName, conditions, sortList, database, howMany: howMany, property: property);
            DapperSQLData data = new ();
            data.SQLStatement = sqls;
            if (conditions != null)
            {
                PopulateSimple(ParameterMappings, data, EnumCategory.Conditional); //if no conditions, then no need.
            }
            return db.Query<R>(data.SQLStatement, data.Parameters, thisTran, commandTimeout: connectionTimeOut).ToBasicList();//hopefully that just works
        }
        public static async Task<BasicList<R>> GetObjectListAsync<E, R>(this IDbConnection db, string property, IDbConnector conn, BasicList<ICondition>? Conditions = null, BasicList<SortInfo>? sortList = null, int howMany = 0, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class
        {
            BasicList<DatabaseMapping> thisList = GetMappingList<E>(out string TableName);
            EnumDatabaseCategory database = db.GetDatabaseCategory(conn);
            var (sqls, ParameterMappings) = GetConditionalStatement(thisList, TableName, Conditions, sortList, database, howMany: howMany, property: property);
            DapperSQLData thisData = new ();
            thisData.SQLStatement = sqls;
            if (Conditions != null)
            {
                PopulateSimple(ParameterMappings, thisData, EnumCategory.Conditional); //if no conditions, then no need.
            }
            var temps = await db.QueryAsync<R>(thisData.SQLStatement, thisData.Parameters, thisTran, commandTimeout: connectionTimeOut);
            return temps.ToBasicList();
        }
    }
}