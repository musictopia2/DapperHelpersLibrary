using CommonBasicStandardLibraries.AdvancedGeneralFunctionsAndProcesses.BasicExtensions;
using CommonBasicStandardLibraries.CollectionClasses;
using CommonBasicStandardLibraries.DatabaseHelpers.ConditionClasses;
using CommonBasicStandardLibraries.DatabaseHelpers.EntityInterfaces;
using CommonBasicStandardLibraries.DatabaseHelpers.MiscClasses;
using CommonBasicStandardLibraries.DatabaseHelpers.MiscInterfaces;
using Dapper;
using DapperHelpersLibrary.MapHelpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks; //most of the time, i will be using asyncs.
using static DapperHelpersLibrary.Extensions.GetSimple;
using static DapperHelpersLibrary.SQLHelpers.PopulateDynamics;
using static DapperHelpersLibrary.SQLHelpers.StatementSelectFactoryJoin;
namespace DapperHelpersLibrary.Extensions
{
    public static class GetConditionalJoinedTables
    {
        //looks like the feature has to be one to one.  one to many probably would not work.
        //because it would choose the first item and that would be it.
        #region One To Many 
        public static CustomBasicList<E> GetOneToMany<E, D1>(this IDbConnection db, CustomBasicList<ICondition> conditionList, IDbConnector conn, CustomBasicList<SortInfo>? sortList = null, Action<E, D1>? action = null, IDbTransaction? thisTran = null, int? ConnectionTimeOut = null) where E : class, IJoinedEntity where D1 : class
        {
            var firstList = db.PrivateOneToManySelectConditional(conditionList, conn, sortList, action, thisTran, ConnectionTimeOut);
            return firstList.ToCustomBasicList();
        }
        public static CustomBasicList<E> GetOneToMany<E, D1, D2>(this IDbConnection db, CustomBasicList<ICondition> conditionList, IDbConnector conn, CustomBasicList<SortInfo>? sortList = null, Action<E, D1, D2>? action = null, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class, IJoin3Entity<D1, D2> where D1 : class where D2 : class
        {
            var firstList = db.PrivateOneToManySelectConditional(conditionList, conn, sortList, action, thisTran, connectionTimeOut);
            return firstList.ToCustomBasicList();
        }
        public async static Task<CustomBasicList<E>> GetOneToManyAsync<E, D1>(this IDbConnection db, CustomBasicList<ICondition> conditionList, IDbConnector conn, CustomBasicList<SortInfo>? sortList = null, Action<E, D1>? action = null, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class, IJoinedEntity where D1 : class
        {
            EnumDatabaseCategory category = db.GetDatabaseCategory(conn);
            var (sqls, ParameterMappings) = GetConditionalStatement<E, D1>(conditionList, sortList, false, category, 0);
            DapperSQLData thisData = new DapperSQLData();
            thisData.SQLStatement = sqls;
            PopulateSimple(ParameterMappings, thisData, EnumCategory.Conditional);
            Dictionary<int, E> thisDict = new Dictionary<int, E>();
            var thisList = await db.QueryAsync<E, D1, E>(sqls, (Main, Detail) => PrivateGetOneToMany(Main, Detail, action, thisDict), thisData.Parameters, thisTran, commandTimeout: connectionTimeOut);
            return thisList.Distinct().ToCustomBasicList();
        }
        public async static Task<CustomBasicList<E>> GetOneToManyAsync<E, D1, D2>(this IDbConnection db, CustomBasicList<ICondition> conditionList, IDbConnector conn, CustomBasicList<SortInfo>? sortList = null, Action<E, D1, D2>? action = null, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class, IJoin3Entity<D1, D2> where D1 : class where D2 : class
        {
            EnumDatabaseCategory category = db.GetDatabaseCategory(conn);
            var (sqls, ParameterMappings) = GetConditionalStatement<E, D1, D2>(conditionList, sortList, category, 0, false);
            DapperSQLData thisData = new DapperSQLData();
            thisData.SQLStatement = sqls;
            PopulateSimple(ParameterMappings, thisData, EnumCategory.Conditional);
            Dictionary<int, E> thisDict = new Dictionary<int, E>();
            var thisList = await db.QueryAsync<E, D1, D2, E>(sqls, (Main, Detail1, Detail2) => PrivateGetOneToMany(Main, Detail1, Detail2, action, thisDict), thisData.Parameters, thisTran, commandTimeout: connectionTimeOut);
            return thisList.Distinct().ToCustomBasicList();
        }
        private static IEnumerable<E> PrivateOneToManySelectConditional<E, D1>(this IDbConnection db, CustomBasicList<ICondition> ConditionList, IDbConnector conn, CustomBasicList<SortInfo>? sortList = null, Action<E, D1>? action = null, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class, IJoinedEntity where D1 : class
        {
            EnumDatabaseCategory category = db.GetDatabaseCategory(conn);
            var (sqls, ParameterMappings) = GetConditionalStatement<E, D1>(ConditionList, sortList, false, category, 0);
            DapperSQLData thisData = new DapperSQLData();
            thisData.SQLStatement = sqls;
            PopulateSimple(ParameterMappings, thisData, EnumCategory.Conditional);
            Dictionary<int, E> thisDict = new Dictionary<int, E>();
            return db.Query<E, D1, E>(sqls, (Main, Detail) => PrivateGetOneToMany(Main, Detail, action, thisDict), thisData.Parameters, thisTran, commandTimeout: connectionTimeOut).Distinct();
        } 
        private static IEnumerable<E> PrivateOneToManySelectConditional<E, D1, D2>(this IDbConnection db, CustomBasicList<ICondition> ConditionList, IDbConnector conn, CustomBasicList<SortInfo>? sortList = null, Action<E, D1, D2>? action = null, IDbTransaction? thisTran = null, int? ConnectionTimeOut = null) where E : class, IJoin3Entity<D1, D2> where D1 : class where D2 : class
        {
            EnumDatabaseCategory Category = db.GetDatabaseCategory(conn);
            var (sqls, ParameterMappings) = GetConditionalStatement<E, D1, D2>(ConditionList, sortList, Category, 0, false);
            DapperSQLData ThisData = new DapperSQLData();
            ThisData.SQLStatement = sqls;
            PopulateSimple(ParameterMappings, ThisData, EnumCategory.Conditional);
            Dictionary<int, E> ThisDict = new Dictionary<int, E>();
            return db.Query<E, D1, D2, E>(sqls, (Main, Detail1, Detail2) => PrivateGetOneToMany(Main, Detail1, Detail2, action, ThisDict), ThisData.Parameters, thisTran, commandTimeout: ConnectionTimeOut).Distinct();
        } 
        #endregion
        #region joined 2 Tables
        //decided to leave as ienumerable.  can change if i decide to.
        public static IEnumerable<E> Get<E, D1>(this IDbConnection db, CustomBasicList<ICondition> conditionList, IDbConnector conn, CustomBasicList<SortInfo>? sortList = null, int HowMany = 0, Action<E, D1>? action = null, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class, IJoinedEntity where D1 : class
        {
            return db.PrivateOneToOneSelectConditional(conditionList, conn, sortList, HowMany, action, thisTran, connectionTimeOut);
        }
        //i think the best bet instead of creating another method is just sending in 1.
        public async static Task<IEnumerable<E>> GetAsync<E, D1>(this IDbConnection db, CustomBasicList<ICondition> conditionList, IDbConnector conn, CustomBasicList<SortInfo>? sortList = null, int HowMany = 0, Action<E, D1>? action = null, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class, IJoinedEntity where D1 : class
        {
            EnumDatabaseCategory Category = db.GetDatabaseCategory(conn);
            var (sqls, ParameterMappings) = GetConditionalStatement<E, D1>(conditionList, sortList, true, Category, HowMany);
            DapperSQLData ThisData = new DapperSQLData();
            ThisData.SQLStatement = sqls;
            PopulateSimple(ParameterMappings, ThisData, EnumCategory.Conditional);
            return await db.QueryAsync<E, D1, E>(sqls, (Main, Detail) => PrivateOneToOne(Main, Detail, action), ThisData.Parameters, thisTran, commandTimeout: connectionTimeOut);
        }
        private static IEnumerable<E> PrivateOneToOneSelectConditional<E, D1>(this IDbConnection db, CustomBasicList<ICondition> conditionList, IDbConnector conn, CustomBasicList<SortInfo>? sortList = null, int HowMany = 0, Action<E, D1>? action = null, IDbTransaction? thisTran = null, int? ConnectionTimeOut = null) where E : class, IJoinedEntity where D1 : class
        {
            EnumDatabaseCategory category = db.GetDatabaseCategory(conn);
            var (sqls, ParameterMappings) = GetConditionalStatement<E, D1>(conditionList, sortList, true, category, HowMany);
            DapperSQLData thisData = new DapperSQLData();
            thisData.SQLStatement = sqls;
            PopulateSimple(ParameterMappings, thisData, EnumCategory.Conditional);
            return db.Query<E, D1, E>(sqls, (Main, Detail) => PrivateOneToOne(Main, Detail, action), thisData.Parameters, thisTran, commandTimeout: ConnectionTimeOut);
        } 
        #endregion
        #region Join 3 Tables
        public static IEnumerable<E> Get<E, D1, D2>(this IDbConnection db, CustomBasicList<ICondition> conditionList, IDbConnector conn, CustomBasicList<SortInfo>? sortList = null, int howMany = 0, Action<E, D1, D2>? action = null, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class, IJoin3Entity<D1, D2> where D1 : class where D2 : class
        {
            return db.PrivateOneToOneSelectConditional(conditionList, conn, sortList, howMany, action, thisTran, connectionTimeOut);
        }
        public async static Task<IEnumerable<E>> GetAsync<E, D1, D2>(this IDbConnection db, CustomBasicList<ICondition> conditionList, IDbConnector conn, CustomBasicList<SortInfo>? sortList = null, int howMany = 0, Action<E, D1, D2>? action = null, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class, IJoin3Entity<D1, D2> where D1 : class where D2 : class
        {
            EnumDatabaseCategory Category = db.GetDatabaseCategory(conn);
            var (sqls, ParameterMappings) = GetConditionalStatement<E, D1, D2>(conditionList, sortList, Category, howMany);
            DapperSQLData ThisData = new DapperSQLData();
            ThisData.SQLStatement = sqls;
            PopulateSimple(ParameterMappings, ThisData, EnumCategory.Conditional);
            return await db.QueryAsync<E, D1, D2, E>(sqls, (Main, Detail1, Detail2) => PrivateOneToOne(Main, Detail1, Detail2, action), ThisData.Parameters, thisTran, commandTimeout: connectionTimeOut);
        }
        private static IEnumerable<E> PrivateOneToOneSelectConditional<E, D1, D2>(this IDbConnection db, CustomBasicList<ICondition> conditionList, IDbConnector conn, CustomBasicList<SortInfo>? sortList = null, int howMany = 0, Action<E, D1, D2>? action = null, IDbTransaction? thisTran = null, int? ConnectionTimeOut = null) where E : class, IJoin3Entity<D1, D2> where D1 : class where D2 : class
        {
            EnumDatabaseCategory Category = db.GetDatabaseCategory(conn);
            var (sqls, ParameterMappings) = GetConditionalStatement<E, D1, D2>(conditionList, sortList, Category, howMany);
            DapperSQLData ThisData = new DapperSQLData();
            ThisData.SQLStatement = sqls;
            PopulateSimple(ParameterMappings, ThisData, EnumCategory.Conditional);
            return db.Query<E, D1, D2, E>(sqls, (Main, Detail1, Detail2) => PrivateOneToOne(Main, Detail1, Detail2, action), ThisData.Parameters, thisTran, commandTimeout: ConnectionTimeOut);
        }
        #endregion
    }
}