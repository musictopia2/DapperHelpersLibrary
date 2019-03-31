using CommonBasicStandardLibraries.CollectionClasses;
using Dapper;
using DapperHelpersLibrary.EntityInterfaces;
using DapperHelpersLibrary.MapHelpers;
using System.Data;
using System.Linq;
using System.Threading.Tasks; //most of the time, i will be using asyncs.
using static DapperHelpersLibrary.MapHelpers.MapBaseHelperClass;
using static DapperHelpersLibrary.SQLHelpers.PopulateDynamics;
using static DapperHelpersLibrary.Extensions.DeleteSimple;
using static DapperHelpersLibrary.SQLHelpers.SimpleStatementHelpers;//i think this is the most common things i like to do
using System.Data.SqlClient;

namespace DapperHelpersLibrary.Extensions
{
    public static class BulkInsertDelete
    {
        #region Insert
        private static DapperSQLData GetDapperInsert(EnumDatabaseCategory Category, CustomBasicList<DatabaseMapping> ThisList, string TableName, bool IsAutoIncremented)
        {
            DapperSQLData output = new DapperSQLData();
            output.SQLStatement = GetInsertStatement(Category, ThisList, TableName, IsAutoIncremented);
            PopulateSimple(ThisList, output, EnumCategory.UseDatabaseMapping);
            return output;
        }

        private static DapperSQLData GetDapperInsert<E>(EnumDatabaseCategory Category, E ThisObj) where E : class
        {
            bool IsAutoIncremented = ThisObj.GetType().IsAutoIncremented();
            CustomBasicList<DatabaseMapping> ThisList = GetMappingList(ThisObj, out string TableName, IsAutoIncremented);
            return GetDapperInsert(Category, ThisList, TableName, IsAutoIncremented);
        }

        

        public static void InsertRange<E>(this IDbConnection db, CustomBasicList<E> ThisList, IDbTransaction ThisTran, int? ConnectionTimeOut = null) where E : class, ISimpleDapperEntity
        {
            EnumDatabaseCategory Category = db.GetDatabaseCategory();
            
            ThisList.ForEach(Items =>
            {
                var ThisData = GetDapperInsert(Category, Items);
                db.PrivateInsertBulk(ThisData, ThisTran, ConnectionTimeOut);
            });            
        }
        private static void PrivateInsertBulk(this IDbConnection db, DapperSQLData ThisData, IDbTransaction ThisTran, int? ConnectionTimeOut = null)
        {
            db.Execute(ThisData.SQLStatement, ThisData.Parameters, ThisTran, commandTimeout: ConnectionTimeOut);
        }

        private async static Task<int> PrivateInsertBulkAsync(this IDbConnection db, DapperSQLData ThisData, IDbTransaction ThisTran, int? ConnectionTimeOut = null)
        {
            return await db.ExecuteScalarAsync<int>(ThisData.SQLStatement, ThisData.Parameters, ThisTran, commandTimeout: ConnectionTimeOut);
        }
        public static async Task InsertRangeAsync<E>(this IDbConnection db, CustomBasicList<E> ThisList, IDbTransaction ThisTran, int? ConnectionTimeOut = null) where E : class, ISimpleDapperEntity
        {
            EnumDatabaseCategory Category = db.GetDatabaseCategory();
            await ThisList.ForEachAsync(async Items =>
            {
                var ThisData = GetDapperInsert(Category, Items);
                Items.ID = await db.PrivateInsertBulkAsync(ThisData, ThisTran, ConnectionTimeOut);
            });
        }
        #endregion
        #region Delete
        
        public static void DeleteRange<E>(this IDbConnection db, CustomBasicList<int> DeleteList, IDbTransaction ThisTran, int? ConnectionTimeOut = null) where E : class
        {

            DeleteList.ForEach(Items =>
            {
                DapperSQLData dapper = PrivateDeleteSingleItem<E>(Items);
                db.Execute(dapper.SQLStatement, dapper.Parameters, ThisTran, ConnectionTimeOut);
            });
            
        }

        
       

        public static async Task DeleteRangeAsync<E>(this IDbConnection db, CustomBasicList<int> DeleteList, IDbTransaction ThisTran, int? ConnectionTimeOut = null) where E : class
        {
            await DeleteList.ForEachAsync(async Items =>
            {
                DapperSQLData dapper = PrivateDeleteSingleItem<E>(Items);
                await db.ExecuteAsync(dapper.SQLStatement, dapper.Parameters, ThisTran, ConnectionTimeOut);
            });
            
        }


        public static void DeleteRange<E>(this IDbConnection db, CustomBasicList<E> ObjectList, IDbTransaction ThisTran, int? ConnectionTimeOut = null) where E : class, ISimpleDapperEntity
        {
            CustomBasicList<int> DeleteList = ObjectList.GetIDList();
            DeleteList.ForEach(Items =>
            {
                DapperSQLData dapper = PrivateDeleteSingleItem<E>(Items);
                db.Execute(dapper.SQLStatement, dapper.Parameters, ThisTran, ConnectionTimeOut);
            });

        }

        public static async Task DeleteRangeAsync<E>(this IDbConnection db, CustomBasicList<E> ObjectList, IDbTransaction ThisTran, int? ConnectionTimeOut = null) where E : class, ISimpleDapperEntity
        {
            CustomBasicList<int> DeleteList = ObjectList.GetIDList();
            await DeleteList.ForEachAsync(async Items =>
            {
                DapperSQLData dapper = PrivateDeleteSingleItem<E>(Items);
                await db.ExecuteAsync(dapper.SQLStatement, dapper.Parameters, ThisTran, ConnectionTimeOut);
            });

        }


        #endregion


    }
}