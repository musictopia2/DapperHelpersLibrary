using System;
using System.Text;
using CommonBasicStandardLibraries.Exceptions;
using CommonBasicStandardLibraries.AdvancedGeneralFunctionsAndProcesses.BasicExtensions;
using System.Linq;
using CommonBasicStandardLibraries.BasicDataSettingsAndProcesses;
using static CommonBasicStandardLibraries.BasicDataSettingsAndProcesses.BasicDataFunctions;
using CommonBasicStandardLibraries.CollectionClasses;
using System.Threading.Tasks; //most of the time, i will be using asyncs.
using fs = CommonBasicStandardLibraries.AdvancedGeneralFunctionsAndProcesses.JsonSerializers.FileHelpers;
using js = CommonBasicStandardLibraries.AdvancedGeneralFunctionsAndProcesses.JsonSerializers.NewtonJsonStrings; //just in case i need those 2.
using Dapper;
using System.Data.SqlClient;
using System.Data;
using System.Collections.Generic; //probably going to be forced to use a dictionary for multiple tables.  especially for one to many relationships
using DapperHelpersLibrary.Extensions;
using DapperHelpersLibrary.ConditionClasses;
using cs = DapperHelpersLibrary.ConditionClasses.ConditionOperators; // just in case you need conditions.
using DapperHelpersLibrary.EntityInterfaces;
using static DapperHelpersLibrary.SQLHelpers.StatementFactoryUpdates;
using DapperHelpersLibrary.SQLHelpers;
using static DapperHelpersLibrary.SQLHelpers.SimpleStatementHelpers;
using DapperHelpersLibrary.MapHelpers;
using static DapperHelpersLibrary.MapHelpers.MapBaseHelperClass;
using static DapperHelpersLibrary.SQLHelpers.StatementFactoryConditionsSingle;
using static DapperHelpersLibrary.SQLHelpers.PopulateDynamics;
//i think this is the most common things i like to do
namespace DapperHelpersLibrary.Extensions
{
    public static class GetOneItem //will not have any joins.
    {
        //needs to start with just one.


        //public static E GetSingle<E>(this IDbConnection db, CustomBasicList<SortInfo> SortList, CustomBasicList<ICondition> Conditions = null, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class
        //{
        //    IEnumerable<E> Results = db.PrivateGetSingleItem<E>(SortList, Conditions, ThisTran, ConnectionTimeOut);
        //    return Results.Single();
        //}

        public static R GetSingleObject<E, R>(this IDbConnection db, string Property, CustomBasicList<SortInfo> SortList, CustomBasicList<ICondition> Conditions = null, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class
        {
            CustomBasicList<DatabaseMapping> ThisList = GetMappingList<E>(out string TableName);
            EnumDatabaseCategory Database = db.GetDatabaseCategory();
            var (sqls, ParameterMappings) = GetConditionalStatement(ThisList, TableName, Conditions, SortList, Database, HowMany: 1, Property: Property);
            DapperSQLData ThisData = new DapperSQLData();  
            ThisData.SQLStatement = sqls;
            if (Conditions != null)
                PopulateSimple(ParameterMappings, ThisData, EnumCategory.Conditional); //if no conditions, then no need.
            return  db.ExecuteScalar<R>(ThisData.SQLStatement, ThisData.Parameters, ThisTran, ConnectionTimeOut);
        }

        public static async Task<R> GetSingleObjectAsync<E, R>(this IDbConnection db, string Property, CustomBasicList<SortInfo> SortList, CustomBasicList<ICondition> Conditions = null, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class
        {
            CustomBasicList<DatabaseMapping> ThisList = GetMappingList<E>(out string TableName);
            EnumDatabaseCategory Database = db.GetDatabaseCategory();
            var (sqls, ParameterMappings) = GetConditionalStatement(ThisList, TableName, Conditions, SortList, Database, HowMany: 1, Property: Property);
            DapperSQLData ThisData = new DapperSQLData();
            ThisData.SQLStatement = sqls;
            if (Conditions != null)
                PopulateSimple(ParameterMappings, ThisData, EnumCategory.Conditional); //if no conditions, then no need.
            return await db.ExecuteScalarAsync<R>(ThisData.SQLStatement, ThisData.Parameters, ThisTran, ConnectionTimeOut);
        }


        public static CustomBasicList<R> GetObjectList<E, R>(this IDbConnection db, string Property, CustomBasicList<ICondition> Conditions = null, CustomBasicList<SortInfo> SortList = null, int HowMany = 0, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class
        {
            CustomBasicList<DatabaseMapping> ThisList = GetMappingList<E>(out string TableName);
            EnumDatabaseCategory Database = db.GetDatabaseCategory();
            var (sqls, ParameterMappings) = GetConditionalStatement(ThisList, TableName, Conditions, SortList, Database, HowMany: HowMany, Property: Property);
            DapperSQLData ThisData = new DapperSQLData();
            ThisData.SQLStatement = sqls;
            if (Conditions != null)
                PopulateSimple(ParameterMappings, ThisData, EnumCategory.Conditional); //if no conditions, then no need.
            return db.Query<R>(ThisData.SQLStatement, ThisData.Parameters, ThisTran, commandTimeout: ConnectionTimeOut).ToCustomBasicList();//hopefully that just works
        }

        public static async Task<CustomBasicList<R>> GetObjectListAsync<E, R>(this IDbConnection db, string Property, CustomBasicList<ICondition> Conditions = null, CustomBasicList<SortInfo> SortList = null, int HowMany = 0, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class
        {
            CustomBasicList<DatabaseMapping> ThisList = GetMappingList<E>(out string TableName);
            EnumDatabaseCategory Database = db.GetDatabaseCategory();
            var (sqls, ParameterMappings) = GetConditionalStatement(ThisList, TableName, Conditions, SortList, Database, HowMany: HowMany, Property: Property);
            DapperSQLData ThisData = new DapperSQLData();
            ThisData.SQLStatement = sqls;
            if (Conditions != null)
                PopulateSimple(ParameterMappings, ThisData, EnumCategory.Conditional); //if no conditions, then no need.
            var Temps = await db.QueryAsync<R>(ThisData.SQLStatement, ThisData.Parameters, ThisTran, commandTimeout: ConnectionTimeOut);
            return Temps.ToCustomBasicList();

            //return await db.QueryAsync<R>(ThisData.SQLStatement, ThisData.Parameters, ThisTran, commandTimeout: ConnectionTimeOut).ToCustomBasicList();//hopefully that just works
        }


    }
}
