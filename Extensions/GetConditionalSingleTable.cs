using CommonBasicStandardLibraries.AdvancedGeneralFunctionsAndProcesses.BasicExtensions;
using CommonBasicStandardLibraries.CollectionClasses;
using CommonBasicStandardLibraries.DatabaseHelpers.ConditionClasses;
using CommonBasicStandardLibraries.DatabaseHelpers.MiscClasses;
using CommonBasicStandardLibraries.DatabaseHelpers.MiscInterfaces;
using Dapper;
using DapperHelpersLibrary.MapHelpers;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks; //most of the time, i will be using asyncs.
using static DapperHelpersLibrary.MapHelpers.MapBaseHelperClass;
using static DapperHelpersLibrary.SQLHelpers.PopulateDynamics;
using static DapperHelpersLibrary.SQLHelpers.StatementFactoryConditionsSingle;
namespace DapperHelpersLibrary.Extensions
{
    public static class GetConditionalSingleTable
    {
        //for this, one or 2 generics.
        //decided to return ienumerable.
        //if i leave as custombasiclist,
        //then what will happen is for the conditional songs, other things will be added automatically.
        public static CustomBasicList<E> Get<E>(this IDbConnection db, CustomBasicList<ICondition> conditions, IDbConnector conn, CustomBasicList<SortInfo>? sortList = null, int howMany = 0, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class
        {
            return db.PrivateSimpleSelectConditional<E>(conditions, conn, sortList, howMany, thisTran, connectionTimeOut).ToCustomBasicList();
        }
        public async static Task<CustomBasicList<E>> GetAsync<E>(this IDbConnection db, CustomBasicList<ICondition> conditions, IDbConnector conn, CustomBasicList<SortInfo>? sortList = null, int howMany = 0, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class
        {
            var temps = await db.PrivateSimpleSelectConditionalAsync<E>(conditions, conn, sortList, howMany, thisTran, connectionTimeOut);
            return temps.ToCustomBasicList();
        }
        private static IEnumerable<E> PrivateSimpleSelectConditional<E>(this IDbConnection db, CustomBasicList<ICondition> Conditions, IDbConnector conn, CustomBasicList<SortInfo>? sortList = null, int howMany = 0, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class
        {
            CustomBasicList<DatabaseMapping> thisList = GetMappingList<E>(out string TableName);
            EnumDatabaseCategory database = db.GetDatabaseCategory(conn);
            var (sqls, ParameterMappings) = GetConditionalStatement(thisList, TableName, Conditions, sortList, database, howMany: howMany);
            DapperSQLData thisData = new DapperSQLData();
            thisData.SQLStatement = sqls;
            PopulateSimple(ParameterMappings, thisData, EnumCategory.Conditional);
            return db.Query<E>(thisData.SQLStatement, thisData.Parameters, thisTran, commandTimeout: connectionTimeOut);
        }
        private async static Task<IEnumerable<E>> PrivateSimpleSelectConditionalAsync<E>(this IDbConnection db, CustomBasicList<ICondition> conditions, IDbConnector conn, CustomBasicList<SortInfo>? sortList = null, int howMany = 0, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class
        {
            CustomBasicList<DatabaseMapping> thisList = GetMappingList<E>(out string TableName);
            EnumDatabaseCategory database = db.GetDatabaseCategory(conn);
            var (sqls, ParameterMappings) = GetConditionalStatement(thisList, TableName, conditions, sortList, database, howMany: howMany);
            DapperSQLData thisData = new DapperSQLData();
            thisData.SQLStatement = sqls;
            PopulateSimple(ParameterMappings, thisData, EnumCategory.Conditional);
            return await db.QueryAsync<E>(thisData.SQLStatement, thisData.Parameters, thisTran, commandTimeout: connectionTimeOut);

        }
    }
}