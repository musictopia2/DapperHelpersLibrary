using CommonBasicStandardLibraries.CollectionClasses;
using Dapper;
using DapperHelpersLibrary.EntityInterfaces;
using DapperHelpersLibrary.MapHelpers;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks; //most of the time, i will be using asyncs.
using static DapperHelpersLibrary.MapHelpers.MapBaseHelperClass;
using static DapperHelpersLibrary.SQLHelpers.PopulateDynamics;
using static DapperHelpersLibrary.SQLHelpers.SimpleStatementHelpers;
using static DapperHelpersLibrary.Extensions.ReflectionDatabase;
using System.Collections.Generic;
using CommonBasicStandardLibraries.AdvancedGeneralFunctionsAndProcesses.BasicExtensions;
using DapperHelpersLibrary.ConditionClasses;
using static DapperHelpersLibrary.SQLHelpers.StatementFactoryConditionsSingle;
using DapperHelpersLibrary.SQLHelpers;

namespace DapperHelpersLibrary.Extensions
{
    public static class GetConditionalSingleTable
    {
        //for this, one or 2 generics.

        //decided to return ienumerable.
        //if i leave as custombasiclist,
        //then what will happen is for the conditional songs, other things will be added automatically.
        public static CustomBasicList<E> Get<E>(this IDbConnection db, CustomBasicList<ICondition> Conditions, CustomBasicList<SortInfo> SortList = null, int HowMany = 0, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class
        {
            return db.PrivateSimpleSelectConditional<E>(Conditions, SortList, HowMany, ThisTran, ConnectionTimeOut).ToCustomBasicList();
        }
        
        public async static Task<CustomBasicList<E>> GetAsync<E>(this IDbConnection db, CustomBasicList<ICondition> Conditions, CustomBasicList<SortInfo> SortList = null, int HowMany = 0, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class
        {
            var Temps = await db.PrivateSimpleSelectConditionalAsync<E>(Conditions, SortList, HowMany, ThisTran, ConnectionTimeOut);
            return Temps.ToCustomBasicList();
        }
        //public async static Task<CustomBasicList<I>> GetAsync<I, E>(this IDbConnection db, CustomBasicList<ICondition> Conditions, CustomBasicList<SortInfo> SortList = null, int HowMany = 0, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class, I
        //{
        //    var ThisList = await db.PrivateSimpleSelectConditionalAsync<E>(Conditions, SortList, HowMany, ThisTran, ConnectionTimeOut);
        //    return ThisList.ToCastedList<I>();
        //}




        private static IEnumerable<E> PrivateSimpleSelectConditional<E>(this IDbConnection db, CustomBasicList<ICondition> Conditions, CustomBasicList<SortInfo> SortList = null, int HowMany = 0, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class
        {
            CustomBasicList<DatabaseMapping> ThisList = GetMappingList<E>(out string TableName);
            EnumDatabaseCategory Database = db.GetDatabaseCategory();
            var (sqls, ParameterMappings) = GetConditionalStatement(ThisList, TableName, Conditions, SortList, Database, HowMany: HowMany);
            DapperSQLData ThisData = new DapperSQLData();
            ThisData.SQLStatement = sqls;
            
            PopulateSimple(ParameterMappings, ThisData, EnumCategory.Conditional);
            return db.Query<E>(ThisData.SQLStatement, ThisData.Parameters, ThisTran, commandTimeout: ConnectionTimeOut);
            //string sqls = GetSimpleSelectStatement<E>();
            //return db.Query<E>(sqls, ThisTran, commandTimeout: ConnectionTimeOut);
        } //if you need transaction, will think about that as well


        private async static Task<IEnumerable<E>> PrivateSimpleSelectConditionalAsync<E>(this IDbConnection db, CustomBasicList<ICondition> Conditions, CustomBasicList<SortInfo> SortList = null, int HowMany = 0, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class
        {
            CustomBasicList<DatabaseMapping> ThisList = GetMappingList<E>(out string TableName);
            EnumDatabaseCategory Database = db.GetDatabaseCategory();
            var (sqls, ParameterMappings) = GetConditionalStatement(ThisList, TableName, Conditions, SortList, Database, HowMany: HowMany);
            DapperSQLData ThisData = new DapperSQLData();
            ThisData.SQLStatement = sqls;
            PopulateSimple(ParameterMappings, ThisData, EnumCategory.Conditional);
            return await db.QueryAsync<E>(ThisData.SQLStatement, ThisData.Parameters, ThisTran, commandTimeout: ConnectionTimeOut);

        }
    }
}
