using CommonBasicStandardLibraries.AdvancedGeneralFunctionsAndProcesses.BasicExtensions;
using CommonBasicStandardLibraries.CollectionClasses;
using CommonBasicStandardLibraries.DatabaseHelpers.EntityInterfaces;
using CommonBasicStandardLibraries.DatabaseHelpers.MiscClasses;
using CommonBasicStandardLibraries.DatabaseHelpers.MiscInterfaces;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks; //most of the time, i will be using asyncs.
using static DapperHelpersLibrary.SQLHelpers.PopulateDynamics;
using static DapperHelpersLibrary.SQLHelpers.SimpleStatementHelpers;
using static DapperHelpersLibrary.SQLHelpers.StatementSelectFactoryJoin;
namespace DapperHelpersLibrary.Extensions
{
    public static class GetSimple
    {
        #region Single Tables
        public static E Get<E>(this IDbConnection db, int id, IDbConnector conn, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class
        {
            IEnumerable<E> results = db.PrivateGetSingleItem<E>(id, conn, thisTran, connectionTimeOut);
            return results.Single();
        }
        public static IEnumerable<E> Get<E>(this IDbConnection db, IDbConnector conn, CustomBasicList<SortInfo>? sortList = null, int HowMany = 0, IDbTransaction? ThisTran = null, int? ConnectionTimeOut = null) where E : class
        {
            return db.PrivateSimpleSelectAll<E>(sortList, conn, HowMany, ThisTran, ConnectionTimeOut);
        }
        public async static Task<E> GetAsync<E>(this IDbConnection db, int id, IDbConnector conn, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class
        {
            IEnumerable<E> Results = await db.PrivateGetSingleItemAsync<E>(id, conn, thisTran, connectionTimeOut);
            return Results.Single();
        }
        public async static Task<IEnumerable<E>> GetAsync<E>(this IDbConnection db, IDbConnector conn, CustomBasicList<SortInfo>? sortList = null, int howMany = 0, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class
        {
            EnumDatabaseCategory category = db.GetDatabaseCategory(conn);
            var (sqls, MapList) = GetSimpleSelectStatement<E>(category, howMany);
            if (sortList != null)
            {
                StringBuilder thisStr = new StringBuilder(sqls);
                thisStr.Append(GetSortStatement(MapList, sortList, false));
                sqls = thisStr.ToString();
            }
            return await db.QueryAsync<E>(sqls, thisTran, commandTimeout: connectionTimeOut);
        }
        private static IEnumerable<E> PrivateGetSingleItem<E>(this IDbConnection db, int id, IDbConnector conn, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class
        {
            StringBuilder builder = new StringBuilder();
            EnumDatabaseCategory category = db.GetDatabaseCategory(conn);
            var (sqls, _) = GetSimpleSelectStatement<E>(category);
            builder.Append(sqls);
            DynamicParameters dynamic = GetDynamicIDData(ref builder, id, false);
            return db.Query<E>(builder.ToString(), dynamic, thisTran, commandTimeout: connectionTimeOut);
        }
        private static IEnumerable<E> PrivateSimpleSelectAll<E>(this IDbConnection db, CustomBasicList<SortInfo>? sortList, IDbConnector conn, int howMany = 0, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class
        {
            EnumDatabaseCategory category = db.GetDatabaseCategory(conn);
            var (sqls, MapList) = GetSimpleSelectStatement<E>(category, howMany);
            StringBuilder thisStr = new StringBuilder();
            thisStr.Append(sqls);
            if (sortList != null)
            {
                thisStr.Append(GetSortStatement(MapList, sortList, false));
            }
            thisStr.Append(GetLimitSQLite(category, howMany));
            sqls = thisStr.ToString();
            return db.Query<E>(sqls, thisTran, commandTimeout: connectionTimeOut);
        } 
        private async static Task<IEnumerable<E>> PrivateGetSingleItemAsync<E>(this IDbConnection db, int id, IDbConnector conn, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class
        {
            StringBuilder builder = new StringBuilder();
            EnumDatabaseCategory category = db.GetDatabaseCategory(conn);
            var (sqls, _) = GetSimpleSelectStatement<E>(category);
            builder.Append(sqls);
            DynamicParameters dynamic = GetDynamicIDData(ref builder, id, false);
            return await db.QueryAsync<E>(builder.ToString(), dynamic, thisTran, commandTimeout: connectionTimeOut);
        }
        #endregion
        #region Two Table One On One
        //this would be the same.
        internal static E PrivateOneToOne<E, T1>(E main, T1 detail, Action<E, T1>? action) where E : class, IJoinedEntity
        {
            if (detail == null)
            {
                if (action != null)
                    action.Invoke(main, detail);
                return main;
            }
            main.AddRelationships(detail);
            action?.Invoke(main, detail);
            return main;
        } 
        public static E Get<E, D1>(this IDbConnection db, int id, IDbConnector conn, Action<E, D1>? action = null, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class, IJoinedEntity where D1 : class
        {
            IEnumerable<E> Results = db.PrivateGetOneToOneItem(id, conn, action, thisTran, connectionTimeOut);
            return Results.Single();
        }
        public static IEnumerable<E> Get<E, D1>(this IDbConnection db, CustomBasicList<SortInfo>? sortList, IDbConnector conn, int howMany = 0, Action<E, D1>? action = null, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class, IJoinedEntity where D1 : class
        {
            return db.PrivateOneToOneSelectAll(sortList, conn, howMany, action, thisTran, connectionTimeOut);
        }
        public async static Task<E> GetAsync<E, D1>(this IDbConnection db, int id, IDbConnector conn, Action<E, D1>? action = null, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class, IJoinedEntity where D1 : class
        {
            IEnumerable<E> Results = await db.PrivateGetOneToOneItemAsync<E, D1>(id, conn, action, thisTran, connectionTimeOut);
            return Results.Single();
        }
        public async static Task<IEnumerable<E>> GetAsync<E, D1>(this IDbConnection db, CustomBasicList<SortInfo>? sortList, IDbConnector conn, int howMany = 0, Action<E, D1>? action = null, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class, IJoinedEntity where D1 : class
        {
            EnumDatabaseCategory category = db.GetDatabaseCategory(conn);
            string sqls = GetSimpleSelectStatement<E, D1>(true, sortList, category, howMany);
            return await db.QueryAsync<E, D1, E>(sqls, (Main, Detail) => PrivateOneToOne(Main, Detail, action), thisTran, commandTimeout: connectionTimeOut);
        }
        private static IEnumerable<E> PrivateGetOneToOneItem<E, D1>(this IDbConnection db, int id, IDbConnector conn, Action<E, D1>? action = null, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class, IJoinedEntity where D1 : class
        {
            StringBuilder builder = new StringBuilder();
            EnumDatabaseCategory category = db.GetDatabaseCategory(conn);
            builder.Append(GetSimpleSelectStatement<E, D1>(true, null, category, 0)); //its implied because of id.
            DynamicParameters dynamic = GetDynamicIDData(ref builder, id, true);
            return db.Query<E, D1, E>(builder.ToString(), (Main, Detail) => PrivateOneToOne(Main, Detail, action), dynamic, thisTran, commandTimeout: connectionTimeOut);
        }
        private static IEnumerable<E> PrivateOneToOneSelectAll<E, D1>(this IDbConnection db, CustomBasicList<SortInfo>? sortList, IDbConnector conn, int howMany = 0, Action<E, D1>? action = null, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class, IJoinedEntity where D1 : class
        {
            EnumDatabaseCategory category = db.GetDatabaseCategory(conn);
            string sqls = GetSimpleSelectStatement<E, D1>(true, sortList, category, howMany);
            return db.Query<E, D1, E>(sqls, (Main, Detail) => PrivateOneToOne(Main, Detail, action), thisTran, commandTimeout: connectionTimeOut);
        }
        private async static Task<IEnumerable<E>> PrivateGetOneToOneItemAsync<E, D1>(this IDbConnection db, int id, IDbConnector conn, Action<E, D1>? action = null, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class, IJoinedEntity where D1 : class
        {
            StringBuilder builder = new StringBuilder();
            EnumDatabaseCategory category = db.GetDatabaseCategory(conn);
            builder.Append(GetSimpleSelectStatement<E, D1>(true, null, category, 0));
            DynamicParameters dynamic = GetDynamicIDData(ref builder, id, true);
            return await db.QueryAsync<E, D1, E>(builder.ToString(), (Main, Detail) => PrivateOneToOne(Main, Detail, action), dynamic, thisTran, commandTimeout: connectionTimeOut);
        }
        #endregion
        #region Three Table One On One
        internal static E PrivateOneToOne<E, T1, T2>(E main, T1 detail1, T2 detail2, Action<E, T1, T2>? action) where E : class, IJoin3Entity<T1, T2>
        {
            //if you have an action, the action is responsible for figuring out if something is null.
            action?.Invoke(main, detail1, detail2);
            main.AddRelationships(detail1, detail2);
            return main;
        }
        public static E Get<E, D1, D2>(this IDbConnection db, int ID, IDbConnector conn, Action<E, D1, D2>? action = null, IDbTransaction? thisTran = null, int? ConnectionTimeOut = null) where E : class, IJoin3Entity<D1, D2> where D1 : class where D2 : class
        {
            IEnumerable<E> Results = db.PrivateGetOneToOneItem(ID, conn, action, thisTran, ConnectionTimeOut);
            return Results.Single();
        }
        public static IEnumerable<E> Get<E, D1, D2>(this IDbConnection db, CustomBasicList<SortInfo> sortList, IDbConnector conn, int howMany = 0, Action<E, D1, D2>? action = null, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class, IJoin3Entity<D1, D2> where D1 : class where D2 : class
        {
            return db.PrivateOneToOneSelectAll(sortList, conn, howMany, action, thisTran, connectionTimeOut);
        }
        public async static Task<E> GetAsync<E, D1, D2>(this IDbConnection db, int id, IDbConnector conn, Action<E, D1, D2>? action = null, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class, IJoin3Entity<D1, D2> where D1 : class where D2 : class
        {
            IEnumerable<E> results = await db.PrivateGetOneToOneItemAsync(id, conn, action, thisTran, connectionTimeOut);
            return results.Single();
        }
        public async static Task<IEnumerable<E>> GetAsync<E, D1, D2>(this IDbConnection db, CustomBasicList<SortInfo> sortList, IDbConnector conn, int howMany = 0, Action<E, D1, D2>? action = null, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class, IJoin3Entity<D1, D2> where D1 : class where D2 : class
        {
            EnumDatabaseCategory category = db.GetDatabaseCategory(conn);
            string sqls = GetSimpleSelectStatement<E, D1, D2>(sortList, category, howMany);
            return await db.QueryAsync<E, D1, D2, E>(sqls, (Main, Detail1, Detail2) => PrivateOneToOne(Main, Detail1, Detail2, action), thisTran, commandTimeout: connectionTimeOut);
        }
        private static IEnumerable<E> PrivateGetOneToOneItem<E, D1, D2>(this IDbConnection db, int id, IDbConnector conn, Action<E, D1, D2>? action = null, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class, IJoin3Entity<D1, D2> where D1 : class where D2 : class
        {
            StringBuilder builder = new StringBuilder();
            EnumDatabaseCategory category = db.GetDatabaseCategory(conn);
            builder.Append(GetSimpleSelectStatement<E, D1, D2>(null, category, 0));
            DynamicParameters dynamic = GetDynamicIDData(ref builder, id, true);
            return db.Query<E, D1, D2, E>(builder.ToString(), (Main, Detail1, Detail2) => PrivateOneToOne(Main, Detail1, Detail2, action), dynamic, thisTran, commandTimeout: connectionTimeOut);
        }
        private static IEnumerable<E> PrivateOneToOneSelectAll<E, D1, D2>(this IDbConnection db, CustomBasicList<SortInfo> sortList, IDbConnector conn, int howMany = 0, Action<E, D1, D2>? action = null, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class, IJoin3Entity<D1, D2> where D1 : class where D2 : class
        {
            EnumDatabaseCategory category = db.GetDatabaseCategory(conn);
            string sqls = GetSimpleSelectStatement<E, D1, D2>(sortList, category, howMany);
            return db.Query<E, D1, D2, E>(sqls, (Main, Detail1, Detail2) => PrivateOneToOne(Main, Detail1, Detail2, action), thisTran, commandTimeout: connectionTimeOut);
        } 
        private async static Task<IEnumerable<E>> PrivateGetOneToOneItemAsync<E, D1, D2>(this IDbConnection db, int id, IDbConnector conn, Action<E, D1, D2>? action = null, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class, IJoin3Entity<D1, D2> where D1 : class where D2 : class
        {
            EnumDatabaseCategory category = db.GetDatabaseCategory(conn);
            StringBuilder builder = new StringBuilder();
            builder.Append(GetSimpleSelectStatement<E, D1, D2>(null, category, 0));
            DynamicParameters dynamic = GetDynamicIDData(ref builder, id, true);
            return await db.QueryAsync<E, D1, D2, E>(builder.ToString(), (Main, Detail1, Detail2) => PrivateOneToOne(Main, Detail1, Detail2, action), dynamic, thisTran, commandTimeout: connectionTimeOut);
        }
        #endregion
        #region Two Table One To Many
        //looks like the only difference is the function is different.
        internal static E PrivateGetOneToMany<E, D1>(E main, D1 detail, Action<E, D1>? action, Dictionary<int, E> thisDict) where E : class, IJoinedEntity
        {
            if (detail == null)
            {
                action?.Invoke(main, detail);
                return main;
            }
            bool had = false;
            if (thisDict.TryGetValue(main.ID, out E thisTemp) == false)
            {
                thisTemp = main;
                thisDict.Add(main.ID, thisTemp);
                had = true;
            }
            thisTemp.AddRelationships(detail); //its up to the main entity to decide what its going to do.
            if (action != null && had == true)
                action.Invoke(main, detail);  //if any changes are done to it with the invoke, will reflect here because of reference  decided to keep the action so single responsibility principle is still followed
            return thisTemp;
        }
        internal static E PrivateGetOneToMany<E, D1, D2>(E main, D1 detail1, D2 detail2, Action<E, D1, D2>? action, Dictionary<int, E> thisDict) where E : class, IJoin3Entity<D1, D2>
            where D1: class
            where D2: class
        {
            if (detail1 == null)
            {
                action?.Invoke(main, detail1!, detail2);
                return main;
            }
            bool had = false;
            if (thisDict.TryGetValue(main.ID, out E thisTemp) == false)
            {
                thisTemp = main;
                thisDict.Add(main.ID, thisTemp);
                had = true;
            }
            thisTemp.AddRelationships(detail1, detail2); //its up to the main entity to decide what its going to do.
            if (action != null && had == true)
                action.Invoke(main, detail1, detail2);  //if any changes are done to it with the invoke, will reflect here because of reference  decided to keep the action so single responsibility principle is still followed
            return thisTemp;
        }
        public static E GetOneToMany<E, D1>(this IDbConnection db, int id, IDbConnector conn, Action<E, D1>? action = null, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class, IJoinedEntity where D1 : class
        {
            IEnumerable<E> results = db.PrivateGetOneToManyItem(id, conn, action, thisTran, connectionTimeOut);
            return results.Single();
        }
        public static IEnumerable<E> GetOneToMany<E, D1>(this IDbConnection db, IDbConnector conn, CustomBasicList<SortInfo>? sortList = null, Action<E, D1>? action = null, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class, IJoinedEntity where D1 : class
        {
            return db.PrivateOneToManySelectAll<E, D1>(conn, sortList, action, thisTran, connectionTimeOut);
        }
        public async static Task<E> GetOneToManyAsync<E, D1>(this IDbConnection db, int id, IDbConnector conn, Action<E, D1>? action = null, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class, IJoinedEntity where D1 : class
        {
            IEnumerable<E> results = await db.PrivateGetOneToManyItemAsync(id, conn, action, thisTran, connectionTimeOut);
            return results.Single();
        }
        public async static Task<IEnumerable<E>> GetOneToManyAsync<E, D1>(this IDbConnection db, IDbConnector conn, CustomBasicList<SortInfo>? sortList = null, Action<E, D1>? action = null, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class, IJoinedEntity where D1 : class
        {
            EnumDatabaseCategory category = db.GetDatabaseCategory(conn);
            string sqls = GetSimpleSelectStatement<E, D1>(false, sortList, category, 0);
            Dictionary<int, E> thisDict = new Dictionary<int, E>();
            var thisList = await db.QueryAsync<E, D1, E>(sqls, (Main, Detail) => PrivateGetOneToMany(Main, Detail, action, thisDict), thisTran, commandTimeout: connectionTimeOut);
            return thisList.Distinct();
        }
        private static IEnumerable<E> PrivateGetOneToManyItem<E, D1>(this IDbConnection db, int id, IDbConnector conn, Action<E, D1>? action = null, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class, IJoinedEntity where D1 : class
        {
            EnumDatabaseCategory category = db.GetDatabaseCategory(conn);
            StringBuilder builder = new StringBuilder();
            builder.Append(GetSimpleSelectStatement<E, D1>(false, null, category, 0));
            DynamicParameters dynamic = GetDynamicIDData(ref builder, id, true);
            Dictionary<int, E> thisDict = new Dictionary<int, E>();
            return db.Query<E, D1, E>(builder.ToString(), (Main, Detail) => PrivateGetOneToMany(Main, Detail, action, thisDict), dynamic, thisTran, commandTimeout: connectionTimeOut).Distinct();
        }
        private static IEnumerable<E> PrivateOneToManySelectAll<E, D1>(this IDbConnection db, IDbConnector conn, CustomBasicList<SortInfo>? sortList = null, Action<E, D1>? action = null, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class, IJoinedEntity where D1 : class
        {
            EnumDatabaseCategory category = db.GetDatabaseCategory(conn);
            string sqls = GetSimpleSelectStatement<E, D1>(false, sortList, category, 0);
            Dictionary<int, E> thisDict = new Dictionary<int, E>();
            return db.Query<E, D1, E>(sqls, (Main, Detail) => PrivateGetOneToMany(Main, Detail, action, thisDict), thisTran, commandTimeout: connectionTimeOut).Distinct();
        }
        private async static Task<IEnumerable<E>> PrivateGetOneToManyItemAsync<E, D1>(this IDbConnection db, int id, IDbConnector conn, Action<E, D1>? action = null, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class, IJoinedEntity where D1 : class
        {
            EnumDatabaseCategory category = db.GetDatabaseCategory(conn);
            StringBuilder builder = new StringBuilder();
            builder.Append(GetSimpleSelectStatement<E, D1>(false, null, category, 0));
            DynamicParameters dynamic = GetDynamicIDData(ref builder, id, true);
            Dictionary<int, E> thisDict = new Dictionary<int, E>();
            var thisList = await db.QueryAsync<E, D1, E>(builder.ToString(), (Main, Detail) => PrivateGetOneToMany(Main, Detail, action, thisDict), dynamic, thisTran, commandTimeout: connectionTimeOut);
            return thisList.Distinct();
        }
        #endregion
    }
}