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
using static DapperHelpersLibrary.SQLHelpers.StatementSelectFactoryJoin;
using static DapperHelpersLibrary.Extensions.ReflectionDatabase;
using System.Collections.Generic;
using CommonBasicStandardLibraries.AdvancedGeneralFunctionsAndProcesses.BasicExtensions;
using System;
using DapperHelpersLibrary.SQLHelpers;
//i think this is the most common things i like to do
namespace DapperHelpersLibrary.Extensions
{
    public static class GetSimple
    {
        #region Single Tables
        public static E Get<E>(this IDbConnection db, int ID, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class
        {
            IEnumerable<E> Results = db.PrivateGetSingleItem<E>(ID, ThisTran, ConnectionTimeOut);
            return Results.Single();
        }

        public static IEnumerable<E> Get<E>(this IDbConnection db, CustomBasicList<SortInfo> SortList = null, int HowMany = 0, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class
        {
            return db.PrivateSimpleSelectAll<E>(SortList, HowMany, ThisTran, ConnectionTimeOut);
        }

        public async static Task<E> GetAsync<E>(this IDbConnection db, int ID, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class
        {
            IEnumerable<E> Results = await db.PrivateGetSingleItemAsync<E>(ID, ThisTran, ConnectionTimeOut);
            return Results.Single();
        }

        //public async static Task<I> GetAsync <I, E>(this IDbConnection db, int ID, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class, I
        //{
        //    IEnumerable<E> Results = await db.PrivateGetSingleItemAsync<E>(ID, ThisTran, ConnectionTimeOut);
        //    return (I)Results.Single();
        //}

        public async static Task<IEnumerable<E>> GetAsync<E>(this IDbConnection db, CustomBasicList<SortInfo> SortList = null, int HowMany = 0, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class
        {
            //string sqls = GetSimpleSelectStatement<E>();
            EnumDatabaseCategory Category = db.GetDatabaseCategory();
            var (sqls, MapList) = GetSimpleSelectStatement<E>(Category, HowMany);
            if (SortList != null)
            {
                StringBuilder ThisStr = new StringBuilder(sqls);
                ThisStr.Append(GetSortStatement(MapList, SortList, false));
                sqls = ThisStr.ToString();
            }
            return await db.QueryAsync<E>(sqls, ThisTran, commandTimeout: ConnectionTimeOut);
            //return  await db.PrivateSimpleSelectAllAsync<E>(ThisTran, ConnectionTimeOut);
        }

        private static IEnumerable<E> PrivateGetSingleItem<E>(this IDbConnection db, int ID, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class
        {
            StringBuilder builder = new StringBuilder();
            //builder.Append(GetSimpleSelectStatement<E>()); //needs to retest this.
            EnumDatabaseCategory Category = db.GetDatabaseCategory();
            var (sqls, _) = GetSimpleSelectStatement<E>(Category);
            builder.Append(sqls);
            DynamicParameters dynamic = GetDynamicIDData(ref builder, ID, false);

            return db.Query<E>(builder.ToString(), dynamic, ThisTran, commandTimeout: ConnectionTimeOut);
        }
        private static IEnumerable<E> PrivateSimpleSelectAll<E>(this IDbConnection db, CustomBasicList<SortInfo> SortList, int HowMany = 0, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class
        {
            EnumDatabaseCategory Category = db.GetDatabaseCategory();
            var (sqls, MapList) = GetSimpleSelectStatement<E>(Category, HowMany);
            StringBuilder ThisStr = new StringBuilder();
            ThisStr.Append(sqls);
            if (SortList != null)
            {
                ThisStr.Append(GetSortStatement(MapList, SortList, false));
            }
            ThisStr.Append(GetLimitSQLite(Category, HowMany));
            sqls = ThisStr.ToString();

            return db.Query<E>(sqls, ThisTran, commandTimeout: ConnectionTimeOut);
        } //if you need transaction, will think about that as well

        

        private async static Task<IEnumerable<E>> PrivateGetSingleItemAsync<E>(this IDbConnection db, int ID, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class
        {
            StringBuilder builder = new StringBuilder();
            //builder.Append(GetSimpleSelectStatement<E>());
            EnumDatabaseCategory Category = db.GetDatabaseCategory();
            var (sqls, _) = GetSimpleSelectStatement<E>(Category);
            builder.Append(sqls);
            DynamicParameters dynamic = GetDynamicIDData(ref builder, ID, false);

            return await db.QueryAsync<E>(builder.ToString(), dynamic, ThisTran, commandTimeout: ConnectionTimeOut);
        }
        
        #endregion
        #region Two Table One On One
        //this would be the same.
        internal static E PrivateOneToOne<E, T1>(E Main, T1 Detail, Action<E, T1> action) where E : class, IJoinedEntity
        {
            if (Detail == null)
            {
                if (action != null)
                    action.Invoke(Main, Detail);
                return Main;
            }
            Main.AddRelationships(Detail);
            if (action != null)
                action.Invoke(Main, Detail); //do you can do other things.
            return Main;
        } //lots of stuff is done here.

        public static E Get<E, D1>(this IDbConnection db, int ID, Action<E, D1> action = null, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class, IJoinedEntity where D1:class
        {
            IEnumerable<E> Results = db.PrivateGetOneToOneItem(ID, action, ThisTran, ConnectionTimeOut);
            return Results.Single();
        }

        public static IEnumerable<E> Get<E, D1>(this IDbConnection db, CustomBasicList<SortInfo> SortList, int HowMany = 0, Action<E, D1> action = null, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class, IJoinedEntity where D1 : class
        {
            return db.PrivateOneToOneSelectAll(SortList, HowMany, action, ThisTran, ConnectionTimeOut);
        }

        public async static Task<E> GetAsync<E, D1>(this IDbConnection db, int ID, Action<E, D1> action = null, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class, IJoinedEntity where D1 : class
        {
            IEnumerable<E> Results = await db.PrivateGetOneToOneItemAsync<E, D1>(ID, action, ThisTran, ConnectionTimeOut);
            return Results.Single();
        }

        public async static Task<IEnumerable<E>> GetAsync<E, D1>(this IDbConnection db, CustomBasicList<SortInfo> SortList, int HowMany = 0, Action<E, D1> action = null, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class, IJoinedEntity where D1 : class
        {
            EnumDatabaseCategory Category = db.GetDatabaseCategory();
            string sqls = GetSimpleSelectStatement<E, D1>(true, SortList, Category, HowMany);
            return await db.QueryAsync<E, D1, E>(sqls, (Main, Detail) => PrivateOneToOne(Main, Detail, action), ThisTran, commandTimeout: ConnectionTimeOut);
        }

        private static IEnumerable<E> PrivateGetOneToOneItem<E, D1>(this IDbConnection db, int ID, Action<E, D1> action = null, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class, IJoinedEntity where D1 : class
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(GetSimpleSelectStatement<E, D1>(true, null, EnumDatabaseCategory.SQLServer, 0)); //its implied because of id.
            DynamicParameters dynamic = GetDynamicIDData(ref builder, ID, true);

            return db.Query<E, D1, E>(builder.ToString(), (Main, Detail) => PrivateOneToOne(Main, Detail, action), dynamic, ThisTran, commandTimeout: ConnectionTimeOut);
        }
        private static IEnumerable<E> PrivateOneToOneSelectAll<E, D1>(this IDbConnection db, CustomBasicList<SortInfo> SortList, int HowMany = 0, Action<E, D1> action = null, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class, IJoinedEntity where D1 : class
        {
            EnumDatabaseCategory Category = db.GetDatabaseCategory();
            string sqls =  GetSimpleSelectStatement<E, D1>(true, SortList, Category, HowMany);
            
            return db.Query<E, D1, E>(sqls, (Main, Detail) => PrivateOneToOne(Main, Detail, action), ThisTran, commandTimeout: ConnectionTimeOut);
        } //if you need transaction, will think about that as well

        private async static Task<IEnumerable<E>> PrivateGetOneToOneItemAsync<E, D1>(this IDbConnection db, int ID, Action<E, D1> action = null, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class, IJoinedEntity where D1 : class
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(GetSimpleSelectStatement<E, D1>(true, null, EnumDatabaseCategory.SQLServer, 0));
            DynamicParameters dynamic = GetDynamicIDData(ref builder, ID, true);

            return await db.QueryAsync<E, D1, E>(builder.ToString(), (Main, Detail) => PrivateOneToOne(Main, Detail, action), dynamic, ThisTran, commandTimeout: ConnectionTimeOut);
        }
        #endregion
        #region Three Table One On One


        internal static E PrivateOneToOne<E, T1, T2>(E Main, T1 Detail1, T2 Detail2, Action<E, T1, T2> action) where E : class, IJoin3Entity<T1, T2>
        {
            //if you have an action, the action is responsible for figuring out if something is null.
            if (action != null)
                action.Invoke(Main, Detail1, Detail2);

            //i think you should just go ahead and implement this.
            //if you ever need it done differently, then rethink
            Main.AddRelationships(Detail1, Detail2);

            //if (Main is IJoin3Entity<T1, T2> Others)
                
            //else
            //{
            //    if (Detail1 != null)
            //        Main.AddRelationships(Detail1);
            //    if (Detail2 != null)
            //        Main.AddRelationships(Detail2);
            //}

            
            return Main;
        }

        public static E Get<E, D1, D2>(this IDbConnection db, int ID, Action<E, D1, D2> action = null, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class, IJoin3Entity<D1, D2> where D1 : class where D2 : class
        {
            IEnumerable<E> Results = db.PrivateGetOneToOneItem(ID, action, ThisTran, ConnectionTimeOut);
            return Results.Single();
        }

        public static IEnumerable<E> Get<E, D1, D2>(this IDbConnection db, CustomBasicList<SortInfo> SortList, int HowMany = 0, Action<E, D1, D2> action = null, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class, IJoin3Entity<D1, D2> where D1 : class where D2 : class
        {
            return db.PrivateOneToOneSelectAll(SortList, HowMany, action, ThisTran, ConnectionTimeOut);
        }

        public async static Task<E> GetAsync<E, D1, D2>(this IDbConnection db, int ID, Action<E, D1, D2> action = null, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class, IJoin3Entity<D1, D2> where D1 : class where D2 : class
        {
            IEnumerable<E> Results = await db.PrivateGetOneToOneItemAsync(ID, action, ThisTran, ConnectionTimeOut);
            return Results.Single();
        }



        public async static Task<IEnumerable<E>> GetAsync<E, D1, D2>(this IDbConnection db, CustomBasicList<SortInfo> SortList, int HowMany = 0, Action<E, D1, D2> action = null, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class, IJoin3Entity<D1, D2> where D1 : class where D2 : class
        {
            EnumDatabaseCategory Category = db.GetDatabaseCategory();
            string sqls = GetSimpleSelectStatement<E, D1, D2>(SortList, Category, HowMany);
            return await db.QueryAsync<E, D1, D2, E>(sqls, (Main, Detail1, Detail2) => PrivateOneToOne(Main, Detail1, Detail2, action), ThisTran, commandTimeout: ConnectionTimeOut);
        }

        private static IEnumerable<E> PrivateGetOneToOneItem<E, D1, D2>(this IDbConnection db, int ID, Action<E, D1, D2> action = null, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class, IJoin3Entity<D1, D2> where D1 : class where D2 : class
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(GetSimpleSelectStatement<E, D1, D2>(null, EnumDatabaseCategory.SQLServer, 0));
            DynamicParameters dynamic = GetDynamicIDData(ref builder, ID, true);

            return db.Query<E, D1, D2, E>(builder.ToString(), (Main, Detail1, Detail2) => PrivateOneToOne(Main, Detail1, Detail2, action), dynamic, ThisTran, commandTimeout: ConnectionTimeOut);
        }
        private static IEnumerable<E> PrivateOneToOneSelectAll<E, D1, D2>(this IDbConnection db, CustomBasicList<SortInfo> SortList, int HowMany = 0, Action<E, D1, D2> action = null, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class, IJoin3Entity<D1, D2> where D1 : class where D2 : class
        {
            EnumDatabaseCategory Category = db.GetDatabaseCategory();
            string sqls = GetSimpleSelectStatement<E, D1, D2>(SortList, Category, HowMany);
            return db.Query<E, D1, D2, E>(sqls, (Main, Detail1, Detail2) => PrivateOneToOne(Main, Detail1, Detail2, action), ThisTran, commandTimeout: ConnectionTimeOut);
        } //if you need transaction, will think about that as well

        private async static Task<IEnumerable<E>> PrivateGetOneToOneItemAsync<E, D1, D2>(this IDbConnection db, int ID, Action<E, D1, D2> action = null, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class, IJoin3Entity<D1, D2> where D1 : class where D2 : class
        {
            EnumDatabaseCategory Category = db.GetDatabaseCategory();
            StringBuilder builder = new StringBuilder();
            builder.Append(GetSimpleSelectStatement<E, D1, D2>(null, EnumDatabaseCategory.SQLServer, 0));
            DynamicParameters dynamic = GetDynamicIDData(ref builder, ID, true);

            return await db.QueryAsync<E, D1, D2, E>(builder.ToString(), (Main, Detail1, Detail2) => PrivateOneToOne(Main, Detail1, Detail2, action), dynamic, ThisTran, commandTimeout: ConnectionTimeOut);
        }


        #endregion
        #region Two Table One To Many
        //looks like the only difference is the function is different.


        internal static E PrivateGetOneToMany<E, D1>(E Main, D1 Detail, Action<E, D1> action, Dictionary<int, E> ThisDict) where E : class, IJoinedEntity
        {
            if (Detail == null)
            {
                if (action != null)
                    action.Invoke(Main, Detail);
                return Main;
            }
            bool had = false;
            if (ThisDict.TryGetValue(Main.ID, out E ThisTemp) == false)
            {
                ThisTemp = Main;
                ThisDict.Add(Main.ID, ThisTemp);
                had = true;
            }
            ThisTemp.AddRelationships(Detail); //its up to the main entity to decide what its going to do.
            if (action != null && had == true)
                action.Invoke(Main, Detail);  //if any changes are done to it with the invoke, will reflect here because of reference  decided to keep the action so single responsibility principle is still followed
            return ThisTemp;
        }


        internal static E PrivateGetOneToMany<E, D1, D2>(E Main, D1 Detail1, D2 Detail2, Action<E, D1, D2> action, Dictionary<int, E> ThisDict) where E : class, IJoin3Entity<D1, D2>
        {
            if (Detail1 == null)
            {
                if (action != null)
                    action.Invoke(Main, Detail1, Detail2);
                return Main;
            }
            bool had = false;
            if (ThisDict.TryGetValue(Main.ID, out E ThisTemp) == false)
            {
                ThisTemp = Main;
                ThisDict.Add(Main.ID, ThisTemp);
                had = true;
            }
            ThisTemp.AddRelationships(Detail1, Detail2); //its up to the main entity to decide what its going to do.
            if (action != null && had == true)
                action.Invoke(Main, Detail1, Detail2);  //if any changes are done to it with the invoke, will reflect here because of reference  decided to keep the action so single responsibility principle is still followed
            return ThisTemp;
        }


        public static E GetOneToMany<E, D1>(this IDbConnection db, int ID, Action<E, D1> action = null, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class, IJoinedEntity where D1 : class
        {
            IEnumerable<E> Results = db.PrivateGetOneToManyItem<E, D1>(ID, action, ThisTran, ConnectionTimeOut);
            return Results.Single();
        }

        public static IEnumerable<E> GetOneToMany<E, D1>(this IDbConnection db, CustomBasicList<SortInfo> SortList = null, Action<E, D1> action = null, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class, IJoinedEntity where D1 : class
        {
            return db.PrivateOneToManySelectAll<E, D1>(SortList, action, ThisTran, ConnectionTimeOut);
        }

        public async static Task<E> GetOneToManyAsync<E, D1>(this IDbConnection db, int ID, Action<E, D1> action = null, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class, IJoinedEntity where D1 : class
        {
            IEnumerable<E> Results = await db.PrivateGetOneToManyItemAsync<E, D1>(ID, action, ThisTran, ConnectionTimeOut);
            return Results.Single();
        }



        public async static Task<IEnumerable<E>> GetOneToManyAsync<E, D1>(this IDbConnection db, CustomBasicList<SortInfo> SortList = null, Action<E, D1> action = null, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class, IJoinedEntity where D1 : class
        {
            string sqls = GetSimpleSelectStatement<E, D1>(false, SortList, EnumDatabaseCategory.SQLServer, 0);
            Dictionary<int, E> ThisDict = new Dictionary<int, E>();
            var ThisList = await db.QueryAsync<E, D1, E>(sqls, (Main, Detail) => PrivateGetOneToMany(Main, Detail, action, ThisDict), ThisTran, commandTimeout: ConnectionTimeOut);
            return ThisList.Distinct();
        }

        private static IEnumerable<E> PrivateGetOneToManyItem<E, D1>(this IDbConnection db, int ID, Action<E, D1> action = null, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class, IJoinedEntity where D1 : class
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(GetSimpleSelectStatement<E, D1>(false, null, EnumDatabaseCategory.SQLServer, 0));
            DynamicParameters dynamic = GetDynamicIDData(ref builder, ID, true);
            Dictionary<int, E> ThisDict = new Dictionary<int, E>();
            return db.Query<E, D1, E>(builder.ToString(), (Main, Detail) => PrivateGetOneToMany(Main, Detail, action, ThisDict), dynamic, ThisTran, commandTimeout: ConnectionTimeOut).Distinct();
        }
        private static IEnumerable<E> PrivateOneToManySelectAll<E, D1>(this IDbConnection db, CustomBasicList<SortInfo> SortList = null, Action<E, D1> action = null, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class, IJoinedEntity where D1 : class
        {
            string sqls = GetSimpleSelectStatement<E, D1>(false, SortList, EnumDatabaseCategory.SQLServer, 0);
            Dictionary<int, E> ThisDict = new Dictionary<int, E>();
            return db.Query<E, D1, E>(sqls, (Main, Detail) => PrivateGetOneToMany(Main, Detail, action, ThisDict), ThisTran, commandTimeout: ConnectionTimeOut).Distinct();
        } //if you need transaction, will think about that as well

        private async static Task<IEnumerable<E>> PrivateGetOneToManyItemAsync<E, D1>(this IDbConnection db, int ID, Action<E, D1> action = null, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class, IJoinedEntity where D1 : class
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(GetSimpleSelectStatement<E, D1>(false, null, EnumDatabaseCategory.SQLServer, 0));
            DynamicParameters dynamic = GetDynamicIDData(ref builder, ID, true);
            Dictionary<int, E> ThisDict = new Dictionary<int, E>();
            var ThisList = await db.QueryAsync<E, D1, E>(builder.ToString(), (Main, Detail) => PrivateGetOneToMany(Main, Detail, action, ThisDict), dynamic, ThisTran, commandTimeout: ConnectionTimeOut);
            return ThisList.Distinct();
        }


        #endregion
    }
}
