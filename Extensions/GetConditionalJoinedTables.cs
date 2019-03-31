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
using static DapperHelpersLibrary.SQLHelpers.StatementFactoryConditionsSingle;//i think this is the most common things i like to do
using System;
using static DapperHelpersLibrary.Extensions.GetSimple;
using static DapperHelpersLibrary.SQLHelpers.StatementSelectFactoryJoin;
using DapperHelpersLibrary.SQLHelpers;

namespace DapperHelpersLibrary.Extensions
{
    public static class GetConditionalJoinedTables
    {
        //looks like the feature has to be one to one.  one to many probably would not work.
        //because it would choose the first item and that would be it.

        #region One To Many 

       

        public static CustomBasicList<E> GetOneToMany<E, D1>(this IDbConnection db, CustomBasicList<ICondition> ConditionList, CustomBasicList<SortInfo> SortList = null, Action<E, D1> action = null, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class, IJoinedEntity where D1 : class
        {
            var FirstList = db.PrivateOneToManySelectConditional(ConditionList, SortList, action, ThisTran, ConnectionTimeOut);
            return FirstList.ToCustomBasicList();
        }

        public static CustomBasicList<E> GetOneToMany<E, D1, D2>(this IDbConnection db, CustomBasicList<ICondition> ConditionList, CustomBasicList<SortInfo> SortList = null, Action<E, D1, D2> action = null, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class, IJoin3Entity<D1, D2> where D1 : class where D2 : class
        {
            var FirstList = db.PrivateOneToManySelectConditional(ConditionList, SortList, action, ThisTran, ConnectionTimeOut);
            return FirstList.ToCustomBasicList();
        }

        public async static Task<CustomBasicList<E>> GetOneToManyAsync<E, D1>(this IDbConnection db, CustomBasicList<ICondition> ConditionList, CustomBasicList<SortInfo> SortList = null, Action<E, D1> action = null, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class, IJoinedEntity where D1 : class
        {
            //string sqls = GetSimpleSelectStatement<E, D1>();
            var (sqls, ParameterMappings) = GetConditionalStatement<E, D1>(ConditionList, SortList, false, EnumDatabaseCategory.SQLServer, 0);
            DapperSQLData ThisData = new DapperSQLData();
            ThisData.SQLStatement = sqls;
            PopulateSimple(ParameterMappings, ThisData, EnumCategory.Conditional);
            Dictionary<int, E> ThisDict = new Dictionary<int, E>();
            var ThisList = await db.QueryAsync<E, D1, E>(sqls, (Main, Detail) => PrivateGetOneToMany(Main, Detail, action, ThisDict), ThisData.Parameters, ThisTran, commandTimeout: ConnectionTimeOut);
            return ThisList.Distinct().ToCustomBasicList();
        }


        public async static Task<CustomBasicList<E>> GetOneToManyAsync<E, D1, D2>(this IDbConnection db, CustomBasicList<ICondition> ConditionList, CustomBasicList<SortInfo> SortList = null, Action<E, D1, D2> action = null, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class, IJoin3Entity<D1, D2> where D1 : class where D2 : class
        {
            var (sqls, ParameterMappings) = GetConditionalStatement<E, D1, D2>(ConditionList, SortList, EnumDatabaseCategory.SQLServer, 0, false);
            //i am missing one more thing.
            DapperSQLData ThisData = new DapperSQLData();
            ThisData.SQLStatement = sqls;
            PopulateSimple(ParameterMappings, ThisData, EnumCategory.Conditional);
            Dictionary<int, E> ThisDict = new Dictionary<int, E>();
            var ThisList  = await db.QueryAsync<E, D1, D2, E>(sqls, (Main, Detail1, Detail2) => PrivateGetOneToMany(Main, Detail1, Detail2, action, ThisDict), ThisData.Parameters, ThisTran, commandTimeout: ConnectionTimeOut);
            return ThisList.Distinct().ToCustomBasicList();
        }


        private static IEnumerable<E> PrivateOneToManySelectConditional<E, D1>(this IDbConnection db, CustomBasicList<ICondition> ConditionList, CustomBasicList<SortInfo> SortList = null, Action<E, D1> action = null, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class, IJoinedEntity where D1 : class
        {
            var (sqls, ParameterMappings) = GetConditionalStatement<E, D1>(ConditionList, SortList, false, EnumDatabaseCategory.SQLServer, 0);
            //i am missing one more thing.
            DapperSQLData ThisData = new DapperSQLData();
            ThisData.SQLStatement = sqls;
            PopulateSimple(ParameterMappings, ThisData, EnumCategory.Conditional);
            Dictionary<int, E> ThisDict = new Dictionary<int, E>();
            return db.Query<E, D1, E>(sqls, (Main, Detail) => PrivateGetOneToMany(Main, Detail, action, ThisDict), ThisData.Parameters, ThisTran, commandTimeout: ConnectionTimeOut).Distinct();
        } //if you need transaction, will think about that as well

        private static IEnumerable<E> PrivateOneToManySelectConditional<E, D1, D2>(this IDbConnection db, CustomBasicList<ICondition> ConditionList, CustomBasicList<SortInfo> SortList = null, Action<E, D1, D2> action = null, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class, IJoin3Entity<D1, D2> where D1 : class where D2: class
        {
            var (sqls, ParameterMappings) = GetConditionalStatement<E, D1, D2>(ConditionList, SortList, EnumDatabaseCategory.SQLServer, 0, false);
            //i am missing one more thing.
            DapperSQLData ThisData = new DapperSQLData();
            ThisData.SQLStatement = sqls;
            PopulateSimple(ParameterMappings, ThisData, EnumCategory.Conditional);
            Dictionary<int, E> ThisDict = new Dictionary<int, E>();
            return db.Query<E, D1, D2, E>(sqls, (Main, Detail1, Detail2) => PrivateGetOneToMany(Main, Detail1, Detail2, action, ThisDict), ThisData.Parameters, ThisTran, commandTimeout: ConnectionTimeOut).Distinct();
        } //if you need transaction, will think about that as well




        #endregion

        #region joined 2 Tables

        //decided to leave as ienumerable.  can change if i decide to.

        public static IEnumerable<E> Get<E, D1>(this IDbConnection db, CustomBasicList<ICondition> ConditionList, CustomBasicList<SortInfo> SortList = null, int HowMany = 0, Action<E, D1> action = null, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class, IJoinedEntity where D1 : class
        {
            return db.PrivateOneToOneSelectConditional(ConditionList, SortList, HowMany, action, ThisTran, ConnectionTimeOut);
        }

        //i think the best bet instead of creating another method is just sending in 1.

       
        public async static Task<IEnumerable<E>> GetAsync<E, D1>(this IDbConnection db, CustomBasicList<ICondition> ConditionList, CustomBasicList<SortInfo> SortList = null, int HowMany = 0, Action<E, D1> action = null, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class, IJoinedEntity where D1 : class
        {
            EnumDatabaseCategory Category = db.GetDatabaseCategory();
            var (sqls, ParameterMappings) = GetConditionalStatement<E, D1>(ConditionList, SortList, true, Category, HowMany);
            DapperSQLData ThisData = new DapperSQLData();
            ThisData.SQLStatement = sqls;
            PopulateSimple(ParameterMappings, ThisData, EnumCategory.Conditional);
            return await db.QueryAsync<E, D1, E>(sqls, (Main, Detail) => PrivateOneToOne(Main, Detail, action), ThisData.Parameters, ThisTran, commandTimeout: ConnectionTimeOut);
        }

       
        private static IEnumerable<E> PrivateOneToOneSelectConditional<E, D1>(this IDbConnection db, CustomBasicList<ICondition> ConditionList, CustomBasicList<SortInfo> SortList = null, int HowMany = 0, Action<E, D1> action = null, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class, IJoinedEntity where D1 : class
        {
            EnumDatabaseCategory Category = db.GetDatabaseCategory();
            var (sqls, ParameterMappings) = GetConditionalStatement<E, D1>(ConditionList, SortList, true, Category, HowMany);
            DapperSQLData ThisData = new DapperSQLData();
            ThisData.SQLStatement = sqls;
            PopulateSimple(ParameterMappings, ThisData, EnumCategory.Conditional);
            return db.Query<E, D1, E>(sqls, (Main, Detail) => PrivateOneToOne(Main, Detail, action), ThisData.Parameters, ThisTran, commandTimeout: ConnectionTimeOut);
        } //if you need transaction, will think about that as well



        #endregion

        #region Join 3 Tables

       

        public static IEnumerable<E> Get<E, D1, D2>(this IDbConnection db, CustomBasicList<ICondition> ConditionList, CustomBasicList<SortInfo> SortList = null, int HowMany = 0, Action<E, D1, D2> action = null, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class, IJoin3Entity<D1, D2> where D1 : class where D2 : class
        {
            return db.PrivateOneToOneSelectConditional(ConditionList, SortList, HowMany, action, ThisTran, ConnectionTimeOut);
        }
       
        public async static Task<IEnumerable<E>> GetAsync <E, D1, D2>(this IDbConnection db, CustomBasicList<ICondition> ConditionList, CustomBasicList<SortInfo> SortList = null, int HowMany = 0, Action<E, D1, D2> action = null, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class, IJoin3Entity<D1, D2> where D1 : class where D2 : class
        {
            EnumDatabaseCategory Category = db.GetDatabaseCategory();
            var (sqls, ParameterMappings) = GetConditionalStatement<E, D1, D2>(ConditionList, SortList, Category, HowMany);
            DapperSQLData ThisData = new DapperSQLData();
            ThisData.SQLStatement = sqls;
            PopulateSimple(ParameterMappings, ThisData, EnumCategory.Conditional);
            return await db.QueryAsync<E, D1, D2, E>(sqls, (Main, Detail1, Detail2) => PrivateOneToOne(Main, Detail1, Detail2, action), ThisData.Parameters, ThisTran, commandTimeout: ConnectionTimeOut);
        }

        
        private static IEnumerable<E> PrivateOneToOneSelectConditional<E, D1, D2>(this IDbConnection db, CustomBasicList<ICondition> ConditionList, CustomBasicList<SortInfo> SortList = null, int HowMany = 0, Action<E, D1, D2> action = null, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class, IJoin3Entity<D1, D2> where D1 : class where D2 : class
        {
            EnumDatabaseCategory Category = db.GetDatabaseCategory();
            var (sqls, ParameterMappings) = GetConditionalStatement<E, D1, D2>(ConditionList, SortList, Category, HowMany);
            DapperSQLData ThisData = new DapperSQLData();
            ThisData.SQLStatement = sqls;
            PopulateSimple(ParameterMappings, ThisData, EnumCategory.Conditional);
            return db.Query<E, D1, D2, E>(sqls, (Main, Detail1, Detail2) => PrivateOneToOne(Main, Detail1, Detail2, action), ThisData.Parameters, ThisTran, commandTimeout: ConnectionTimeOut);
        } //if you need transaction, will think about that as well

        
        #endregion
    }
}
