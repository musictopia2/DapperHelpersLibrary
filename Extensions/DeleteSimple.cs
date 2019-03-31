using Dapper;
using DapperHelpersLibrary.EntityInterfaces;
using DapperHelpersLibrary.MapHelpers;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using static DapperHelpersLibrary.Extensions.ReflectionDatabase;
using static DapperHelpersLibrary.SQLHelpers.PopulateDynamics;
using static DapperHelpersLibrary.SQLHelpers.SimpleStatementHelpers;
using CommonBasicStandardLibraries.CollectionClasses;
using DapperHelpersLibrary.ConditionClasses;
using static DapperHelpersLibrary.SQLHelpers.StatementFactoryConditionsSingle;
using static DapperHelpersLibrary.MapHelpers.MapBaseHelperClass;
//i think this is the most common things i like to do
namespace DapperHelpersLibrary.Extensions
{
    public static class DeleteSimple
    {
        internal static DapperSQLData PrivateDeleteSingleItem<E>(int ID) where E : class
        {
            StringBuilder builder = new StringBuilder();
            string tablename = GetTableName<E>();
            builder.Append(GetDeleteStatement(tablename));
            DynamicParameters dynamic = GetDynamicIDData(ref builder, ID);
            return new DapperSQLData() { Parameters = dynamic, SQLStatement = builder.ToString() };
        }
        public static void Delete<E>(this IDbConnection db, int ID, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class
        {
            DapperSQLData dapper = PrivateDeleteSingleItem<E>(ID);
            db.Execute(dapper.SQLStatement, dapper.Parameters, ThisTran, ConnectionTimeOut);
        }

        public static void DeleteEverythingFromTable<E>(this IDbConnection db, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class
        {
            string tablename = GetTableName<E>();
            string sqls = GetDeleteStatement(tablename);
            db.Execute(sqls, null, ThisTran, ConnectionTimeOut);
        }
        public static void Delete<E>(this IDbConnection db, E ThisObj, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class, ISimpleDapperEntity
        {
            db.Delete<E>(ThisObj.ID, ThisTran, ConnectionTimeOut);
        }

        public static async Task DeleteAsync<E>(this IDbConnection db, int ID, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class
        {
            DapperSQLData dapper = PrivateDeleteSingleItem<E>(ID);
            await db.ExecuteAsync(dapper.SQLStatement, dapper.Parameters, ThisTran, ConnectionTimeOut);
        }

        public static async Task DeleteEntireTableAsync<E>(this IDbConnection db, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class
        {
            string tablename = GetTableName<E>();
            string sqls = GetDeleteStatement(tablename);
            await db.ExecuteAsync(sqls, null, ThisTran, ConnectionTimeOut);
        }
        public static async Task DeleteAsync<E>(this IDbConnection db, E ThisObj, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class, ISimpleDapperEntity
        {
            await db.DeleteAsync<E>(ThisObj.ID, ThisTran, ConnectionTimeOut);
        }

        public static async Task DeleteAsync<E>(this IDbConnection db, CustomBasicList<ICondition> Conditions, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class, ISimpleDapperEntity
        {
            CustomBasicList<DatabaseMapping> ThisList = GetMappingList<E>(out string TableName);
            EnumDatabaseCategory Database = db.GetDatabaseCategory();
            var (sqls, ParameterMappings) = GetConditionalStatement(ThisList, TableName, Conditions, null, Database, EnumSQLCategory.Delete);
            DapperSQLData ThisData = new DapperSQLData();
            ThisData.SQLStatement = sqls;
            PopulateSimple(ParameterMappings, ThisData, EnumCategory.Conditional);
            await db.ExecuteAsync(sqls, ThisData.Parameters, ThisTran, ConnectionTimeOut);
        }
        public static void Delete<E>(this IDbConnection db, CustomBasicList<ICondition> Conditions, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class, ISimpleDapperEntity
        {
            EnumDatabaseCategory Database = db.GetDatabaseCategory();
            CustomBasicList<DatabaseMapping> ThisList = GetMappingList<E>(out string TableName);
            var (sqls, ParameterMappings) = GetConditionalStatement(ThisList, TableName, Conditions, null, Database, EnumSQLCategory.Delete);
            DapperSQLData ThisData = new DapperSQLData();
            ThisData.SQLStatement = sqls;
            PopulateSimple(ParameterMappings, ThisData, EnumCategory.Conditional);
            db.Execute(sqls, ThisData.Parameters, ThisTran, ConnectionTimeOut);
        }

    }
}