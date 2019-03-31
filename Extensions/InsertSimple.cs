using CommonBasicStandardLibraries.CollectionClasses;
using Dapper;
using DapperHelpersLibrary.EntityInterfaces;
using DapperHelpersLibrary.MapHelpers;
using System.Data;
using System.Linq;
using System.Threading.Tasks; //most of the time, i will be using asyncs.
using static DapperHelpersLibrary.MapHelpers.MapBaseHelperClass;
using static DapperHelpersLibrary.SQLHelpers.PopulateDynamics;
using static DapperHelpersLibrary.SQLHelpers.SimpleStatementHelpers;
//i think this is the most common things i like to do
namespace DapperHelpersLibrary.Extensions
{
    public static class InsertSimple
    {

        

        private static DapperSQLData GetDapperInsert(EnumDatabaseCategory Category, CustomBasicList<DatabaseMapping> ThisList, string TableName, bool IsAutoIncremented)
        {
            DapperSQLData output = new DapperSQLData();
            
            output.SQLStatement = GetInsertStatement(Category, ThisList, TableName, IsAutoIncremented);
            PopulateSimple(ThisList, output, EnumCategory.UseDatabaseMapping);
            return output;
        }
        private static DapperSQLData GetDapperInsert<E>(E ThisObj, EnumDatabaseCategory Category, out bool IsAutoIncremented) where E : class
        {
            IsAutoIncremented = ThisObj.GetType().IsAutoIncremented();
            CustomBasicList<DatabaseMapping> ThisList = GetMappingList(ThisObj, out string TableName, IsAutoIncremented);
            return GetDapperInsert(Category, ThisList, TableName, IsAutoIncremented);
        }

        private static int PrivateInsertSingle(this IDbConnection db, DapperSQLData ThisData, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null)
        {
            return db.Query<int>(ThisData.SQLStatement, ThisData.Parameters, ThisTran, commandTimeout: ConnectionTimeOut).FirstOrDefault();
        }

        private async static Task<int> PrivateInsertSingleAsync(this IDbConnection db, DapperSQLData ThisData, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null)
        {
            var Temps = await db.QueryAsync<int>(ThisData.SQLStatement, ThisData.Parameters, ThisTran, commandTimeout: ConnectionTimeOut);
            return Temps.FirstOrDefault();
        }

        public static int InsertSingle<E>(this IDbConnection db, E ThisObject, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class, ISimpleDapperEntity
        {
            EnumDatabaseCategory Category = db.GetDatabaseCategory();
            var ThisData = GetDapperInsert(ThisObject, Category, out bool IsAutoIncremented);
            int ID = db.PrivateInsertSingle(ThisData, ThisTran, ConnectionTimeOut);
            if (IsAutoIncremented == true)
                return ID;
            return ThisObject.ID;
        }

        //public static int InsertSingle<I, E>(this IDbConnection db, I ThisObject, )

        //i was going to do the interface version but the work is much harder so you should just cast the proper type first.  i can always rethink if necessary

        public static async Task<int> InsertSingleAsync<E>(this IDbConnection db, E ThisObject, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class, ISimpleDapperEntity
        {
            EnumDatabaseCategory Category = db.GetDatabaseCategory();
            var ThisData = GetDapperInsert(ThisObject, Category, out bool IsAutoIncremented);
            int ID = await db.PrivateInsertSingleAsync(ThisData, ThisTran, ConnectionTimeOut);
            if (IsAutoIncremented == true)
                return ID;
            return ThisObject.ID;
            //var Temps = await db.QueryAsync<int>(ThisData.SQLStatement, ThisData.Parameters, ThisTran, commandTimeout: ConnectionTimeOut);
            //return Temps.First();
        }




    }
}
