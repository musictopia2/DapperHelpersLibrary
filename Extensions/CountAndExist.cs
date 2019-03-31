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
using static DapperHelpersLibrary.ConnectionHelper;

//i think this is the most common things i like to do
namespace DapperHelpersLibrary.Extensions
{
    public static class CountAndExist
    {
        //public static CustomBasicList<E> Get<E>(this IDbConnection db, CustomBasicList<ICondition> Conditions, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class
        //{
        //    return db.PrivateSimpleSelectConditional<E>(Conditions, ThisTran, ConnectionTimeOut).ToCustomBasicList();
        //}

        public static int Count<E>(this IDbConnection db, CustomBasicList<ICondition> Conditions, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class
        {
            //decided to not even have an async version.  could change my mind if i choose to.
            SQLHelpers.SimpleStatementHelpers.EnumDatabaseCategory Database = db.GetDatabaseCategory();
            CustomBasicList<DatabaseMapping> ThisList = GetMappingList<E>(out string TableName);
            var (sqls, ParameterMappings) = GetConditionalStatement(ThisList, TableName, Conditions, null, Database, EnumSQLCategory.Count);
            DapperSQLData ThisData = new DapperSQLData();
            ThisData.SQLStatement = sqls;

            PopulateSimple(ParameterMappings, ThisData, EnumCategory.Conditional);
            return db.ExecuteScalar<int>(sqls, ThisData.Parameters, ThisTran, ConnectionTimeOut);
        }

        public static int Count<E>(this IDbConnection db, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class
        {
            //decided to not even have an async version.  could change my mind if i choose to.
            CustomBasicList<DatabaseMapping> ThisList = GetMappingList<E>(out string TableName);
            //var (sqls, ParameterMappings) = GetConditionalStatement(ThisList, TableName, Conditions, EnumSQLCategory.Count);
            //DapperSQLData ThisData = new DapperSQLData();
            //ThisData.SQLStatement = sqls;
            SQLHelpers.SimpleStatementHelpers.EnumDatabaseCategory Database = db.GetDatabaseCategory();
            string sqls = GetSimpleSelectStatement(ThisList, TableName, Database, EnumSQLCategory.Count);
            //PopulateSimple(ParameterMappings, ThisData, EnumCategory.Conditional);
            return db.ExecuteScalar<int>(sqls, null, ThisTran, ConnectionTimeOut);
        }

        public static bool Exists<E>(this IDbConnection db, CustomBasicList<ICondition> Conditions, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class
        {
            CustomBasicList<DatabaseMapping> ThisList = GetMappingList<E>(out string TableName);
            SQLHelpers.SimpleStatementHelpers.EnumDatabaseCategory Database = db.GetDatabaseCategory();
            var (sqls, ParameterMappings) = GetConditionalStatement(ThisList, TableName, Conditions, null, Database, EnumSQLCategory.Bool);
            DapperSQLData ThisData = new DapperSQLData();
            ThisData.SQLStatement = sqls;
            PopulateSimple(ParameterMappings, ThisData, EnumCategory.Conditional);
            return db.ExecuteScalar<bool>(sqls, ThisData.Parameters, ThisTran, ConnectionTimeOut);
        }

        public static bool Exists<E>(this IDbConnection db, int ID, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class
        {
            CustomBasicList<ICondition> ThisList = StartConditionWithID(ID);
            return db.Exists<E>(ThisList, ThisTran, ConnectionTimeOut);
        }
    }
}
