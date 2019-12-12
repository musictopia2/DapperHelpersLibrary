using CommonBasicStandardLibraries.CollectionClasses;
using Dapper;
using DapperHelpersLibrary.MapHelpers;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using static DapperHelpersLibrary.MapHelpers.MapBaseHelperClass;
using static DapperHelpersLibrary.SQLHelpers.PopulateDynamics;
using static DapperHelpersLibrary.SQLHelpers.SimpleStatementHelpers;
using static DapperHelpersLibrary.SQLHelpers.StatementFactoryConditionsSingle;
using static CommonBasicStandardLibraries.DatabaseHelpers.Extensions.ReflectionDatabase;
using CommonBasicStandardLibraries.DatabaseHelpers.ConditionClasses;
using CommonBasicStandardLibraries.DatabaseHelpers.EntityInterfaces;
using CommonBasicStandardLibraries.DatabaseHelpers.MiscClasses;
using CommonBasicStandardLibraries.DatabaseHelpers.MiscInterfaces;
namespace DapperHelpersLibrary.Extensions
{
    public static class DeleteSimple
    {
        internal static DapperSQLData PrivateDeleteSingleItem<E>(int id) where E : class
        {
            StringBuilder builder = new StringBuilder();
            string tablename = GetTableName<E>();
            builder.Append(GetDeleteStatement(tablename));
            DynamicParameters dynamic = GetDynamicIDData(ref builder, id);
            return new DapperSQLData() { Parameters = dynamic, SQLStatement = builder.ToString() };
        }
        public static void Delete<E>(this IDbConnection db, int id, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class
        {
            DapperSQLData dapper = PrivateDeleteSingleItem<E>(id);
            db.Execute(dapper.SQLStatement, dapper.Parameters, thisTran, connectionTimeOut);
        }
        public static void DeleteEverythingFromTable<E>(this IDbConnection db, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class
        {
            string tablename = GetTableName<E>();
            string sqls = GetDeleteStatement(tablename);
            db.Execute(sqls, null, thisTran, connectionTimeOut);
        }
        public static void Delete<E>(this IDbConnection db, E ThisObj, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class, ISimpleDapperEntity
        {
            db.Delete<E>(ThisObj.ID, thisTran, connectionTimeOut);
        }
        public static async Task DeleteAsync<E>(this IDbConnection db, int ID, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class
        {
            DapperSQLData dapper = PrivateDeleteSingleItem<E>(ID);
            await db.ExecuteAsync(dapper.SQLStatement, dapper.Parameters, thisTran, connectionTimeOut);
        }
        public static async Task DeleteEntireTableAsync<E>(this IDbConnection db, IDbTransaction? thisTran = null, int? ConnectionTimeOut = null) where E : class
        {
            string tablename = GetTableName<E>();
            string sqls = GetDeleteStatement(tablename);
            await db.ExecuteAsync(sqls, null, thisTran, ConnectionTimeOut);
        }
        public static async Task DeleteAsync<E>(this IDbConnection db, E thisObj, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class, ISimpleDapperEntity
        {
            await db.DeleteAsync<E>(thisObj.ID, thisTran, connectionTimeOut);
        }
        public static async Task DeleteAsync<E>(this IDbConnection db, CustomBasicList<ICondition> conditions, IDbConnector conn, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class, ISimpleDapperEntity
        {
            CustomBasicList<DatabaseMapping> thisList = GetMappingList<E>(out string TableName);
            EnumDatabaseCategory database = db.GetDatabaseCategory(conn);
            var (sqls, ParameterMappings) = GetConditionalStatement(thisList, TableName, conditions, null, database, EnumSQLCategory.Delete);
            DapperSQLData thisData = new DapperSQLData();
            thisData.SQLStatement = sqls;
            PopulateSimple(ParameterMappings, thisData, EnumCategory.Conditional);
            await db.ExecuteAsync(sqls, thisData.Parameters, thisTran, connectionTimeOut);
        }
        public static void Delete<E>(this IDbConnection db, CustomBasicList<ICondition> conditions, IDbConnector conn, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class, ISimpleDapperEntity
        {
            EnumDatabaseCategory database = db.GetDatabaseCategory(conn);
            CustomBasicList<DatabaseMapping> thisList = GetMappingList<E>(out string TableName);
            var (sqls, ParameterMappings) = GetConditionalStatement(thisList, TableName, conditions, null, database, EnumSQLCategory.Delete);
            DapperSQLData thisData = new DapperSQLData();
            thisData.SQLStatement = sqls;
            PopulateSimple(ParameterMappings, thisData, EnumCategory.Conditional);
            db.Execute(sqls, thisData.Parameters, thisTran, connectionTimeOut);
        }
    }
}