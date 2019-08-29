using CommonBasicStandardLibraries.CollectionClasses;
using Dapper;
using DapperHelpersLibrary.EntityInterfaces;
using DapperHelpersLibrary.MapHelpers;
using DapperHelpersLibrary.SQLHelpers;
using System.Data;
using System.Linq;
using System.Threading.Tasks; //most of the time, i will be using asyncs.
using static DapperHelpersLibrary.MapHelpers.MapBaseHelperClass;
using static DapperHelpersLibrary.SQLHelpers.PopulateDynamics;
using static DapperHelpersLibrary.SQLHelpers.SimpleStatementHelpers;
using static DapperHelpersLibrary.SQLHelpers.StatementFactoryUpdates;
//i think this is the most common things i like to do
namespace DapperHelpersLibrary.Extensions
{
    public static class UpdateDatabase
    {

        public static void UpdateEntity<E>(this IDbConnection db, E ThisEntity, EnumUpdateCategory Category, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E: class, ISimpleDapperEntity
        {
            var (sqls, ParameterMappings) = GetUpdateStatement(ThisEntity, Category);
            db.PrivateUpdateEntity(ThisEntity, sqls, ParameterMappings, ThisTran, ConnectionTimeOut);
        }

        public static void UpdateEntity<E>(this IDbConnection db, E ThisEntity, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class, IUpdatableEntity
        {
            var (sqls, ParameterMappings) = GetUpdateStatement(ThisEntity);
            db.PrivateUpdateEntity(ThisEntity, sqls, ParameterMappings, ThisTran, ConnectionTimeOut);
        }

        public static void UpdateEntity<E>(this IDbConnection db, E ThisEntity, CustomBasicList<UpdateFieldInfo> UpdateList , IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E: class, ISimpleDapperEntity
        {
            var (sqls, ParameterMappings) = GetUpdateStatement(ThisEntity, UpdateList);
            db.PrivateUpdateEntity(ThisEntity, sqls, ParameterMappings, ThisTran, ConnectionTimeOut);
        }

        private static void PrivateUpdateEntity<E>(this IDbConnection db, E ThisEntity, string sqls, CustomBasicList<DatabaseMapping> UpdateList, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E:class, ISimpleDapperEntity
        {
            if (sqls == "")
                return;
            DapperSQLData ThisData = new DapperSQLData();
            ThisData.SQLStatement = sqls;

            PopulateSimple(UpdateList, ThisData, EnumCategory.UseDatabaseMapping);
            ThisData.Parameters.Add("ID", ThisEntity.ID);
            db.Execute(sqls, ThisData.Parameters, ThisTran, ConnectionTimeOut);
        }

        public static async Task UpdateEntityAsync<E>(this IDbConnection db, E ThisEntity, EnumUpdateCategory Category, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class, ISimpleDapperEntity
        {
            var (sqls, ParameterMappings) = GetUpdateStatement(ThisEntity, Category);
            await db.PrivateUpdateEntityAsync(ThisEntity, sqls, ParameterMappings, ThisTran, ConnectionTimeOut);
        }

        public static async Task UpdateEntityAsync<E>(this IDbConnection db, E ThisEntity, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class, IUpdatableEntity
        {
            var (sqls, ParameterMappings) = GetUpdateStatement(ThisEntity);
            await db.PrivateUpdateEntityAsync(ThisEntity, sqls, ParameterMappings, ThisTran, ConnectionTimeOut);
        }

        public static async Task UpdateEntityAsync<E>(this IDbConnection db, E ThisEntity, CustomBasicList<UpdateFieldInfo> UpdateList, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class,ISimpleDapperEntity
        {
            var (sqls, ParameterMappings) = GetUpdateStatement(ThisEntity, UpdateList);
            await db.PrivateUpdateEntityAsync(ThisEntity, sqls, ParameterMappings, ThisTran, ConnectionTimeOut);
        }

        private static async Task PrivateUpdateEntityAsync<E>(this IDbConnection db, E ThisEntity, string sqls, CustomBasicList<DatabaseMapping> UpdateList, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E:class, ISimpleDapperEntity
        {
            if (sqls == "")
                return;
            DapperSQLData ThisData = new DapperSQLData();
            ThisData.SQLStatement = sqls;
            ThisData.Parameters.Add("ID", ThisEntity.ID);
            PopulateSimple(UpdateList, ThisData, EnumCategory.UseDatabaseMapping);
            await db.ExecuteAsync(sqls, ThisData.Parameters, ThisTran, ConnectionTimeOut);
        }
        public static void Update<E>(this IDbConnection db, int ID, CustomBasicList<UpdateEntity> UpdateList, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class, ISimpleDapperEntity
        {
            var (sqls, ParameterMappings) = GetUpdateStatement<E>(UpdateList);
            DapperSQLData ThisData = new DapperSQLData();
            ThisData.SQLStatement = sqls;
            ThisData.Parameters.Add("ID", ID);
            PopulateSimple(ParameterMappings, ThisData, EnumCategory.UseDatabaseMapping);
            db.Execute(sqls, ThisData.Parameters, ThisTran, ConnectionTimeOut);
        }
        public static async Task UpdateAsync<E>(this IDbConnection db, int ID, CustomBasicList<UpdateEntity> UpdateList, IDbTransaction ThisTran = null, int? ConnectionTimeOut = null) where E : class, ISimpleDapperEntity
        {
            var (sqls, ParameterMappings) = GetUpdateStatement<E>(UpdateList);
            DapperSQLData ThisData = new DapperSQLData();
            ThisData.SQLStatement = sqls;
            ThisData.Parameters.Add("ID", ID);
            PopulateSimple(ParameterMappings, ThisData, EnumCategory.UseDatabaseMapping);
            await db.ExecuteAsync(sqls, ThisData.Parameters, ThisTran, ConnectionTimeOut);
        }
    }
}
