using CommonBasicStandardLibraries.CollectionClasses;
using CommonBasicStandardLibraries.DatabaseHelpers.EntityInterfaces;
using CommonBasicStandardLibraries.DatabaseHelpers.MiscClasses;
using Dapper;
using DapperHelpersLibrary.MapHelpers;
using System.Data;
using System.Threading.Tasks; //most of the time, i will be using asyncs.
using static DapperHelpersLibrary.SQLHelpers.PopulateDynamics;
using static DapperHelpersLibrary.SQLHelpers.StatementFactoryUpdates;
namespace DapperHelpersLibrary.Extensions
{
    public static class UpdateDatabase
    {
        public static void UpdateEntity<E>(this IDbConnection db, E thisEntity, EnumUpdateCategory category, IDbTransaction? thisTran = null, int? ConnectionTimeOut = null) where E : class, ISimpleDapperEntity
        {
            var (sqls, ParameterMappings) = GetUpdateStatement(thisEntity, category);
            db.PrivateUpdateEntity(thisEntity, sqls, ParameterMappings, thisTran, ConnectionTimeOut);
        }
        public static void UpdateEntity<E>(this IDbConnection db, E thisEntity, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class, IUpdatableEntity
        {
            var (sqls, ParameterMappings) = GetUpdateStatement(thisEntity);
            db.PrivateUpdateEntity(thisEntity, sqls, ParameterMappings, thisTran, connectionTimeOut);
        }
        public static void UpdateEntity<E>(this IDbConnection db, E thisEntity, CustomBasicList<UpdateFieldInfo> updateList, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class, ISimpleDapperEntity
        {
            var (sqls, ParameterMappings) = GetUpdateStatement(thisEntity, updateList);
            db.PrivateUpdateEntity(thisEntity, sqls, ParameterMappings, thisTran, connectionTimeOut);
        }
        private static void PrivateUpdateEntity<E>(this IDbConnection db, E thisEntity, string sqls, CustomBasicList<DatabaseMapping>? updateList, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class, ISimpleDapperEntity
        {
            if (sqls == "")
                return;
            DapperSQLData thisData = new DapperSQLData();
            thisData.SQLStatement = sqls;
            PopulateSimple(updateList!, thisData, EnumCategory.UseDatabaseMapping);
            thisData.Parameters.Add("ID", thisEntity.ID);
            db.Execute(sqls, thisData.Parameters, thisTran, connectionTimeOut);
        }
        public static async Task UpdateEntityAsync<E>(this IDbConnection db, E thisEntity, EnumUpdateCategory category, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class, ISimpleDapperEntity
        {
            var (sqls, ParameterMappings) = GetUpdateStatement(thisEntity, category);
            await db.PrivateUpdateEntityAsync(thisEntity, sqls, ParameterMappings, thisTran, connectionTimeOut);
        }
        public static async Task UpdateEntityAsync<E>(this IDbConnection db, E thisEntity, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class, IUpdatableEntity
        {
            var (sqls, ParameterMappings) = GetUpdateStatement(thisEntity);
            await db.PrivateUpdateEntityAsync(thisEntity, sqls, ParameterMappings, thisTran, connectionTimeOut);
        }
        public static async Task UpdateEntityAsync<E>(this IDbConnection db, E thisEntity, CustomBasicList<UpdateFieldInfo> updateList, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class, ISimpleDapperEntity
        {
            var (sqls, ParameterMappings) = GetUpdateStatement(thisEntity, updateList);
            await db.PrivateUpdateEntityAsync(thisEntity, sqls, ParameterMappings, thisTran, connectionTimeOut);
        }
        private static async Task PrivateUpdateEntityAsync<E>(this IDbConnection db, E thisEntity, string sqls, CustomBasicList<DatabaseMapping> updateList, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class, ISimpleDapperEntity
        {
            if (sqls == "")
                return;
            DapperSQLData thisData = new DapperSQLData();
            thisData.SQLStatement = sqls;
            thisData.Parameters.Add("ID", thisEntity.ID);
            PopulateSimple(updateList, thisData, EnumCategory.UseDatabaseMapping);
            await db.ExecuteAsync(sqls, thisData.Parameters, thisTran, connectionTimeOut);
        }
        public static void Update<E>(this IDbConnection db, int id, CustomBasicList<UpdateEntity> updateList, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class, ISimpleDapperEntity
        {
            var (sqls, ParameterMappings) = GetUpdateStatement<E>(updateList);
            DapperSQLData thisData = new DapperSQLData();
            thisData.SQLStatement = sqls;
            thisData.Parameters.Add("ID", id);
            PopulateSimple(ParameterMappings, thisData, EnumCategory.UseDatabaseMapping);
            db.Execute(sqls, thisData.Parameters, thisTran, connectionTimeOut);
        }
        public static async Task UpdateAsync<E>(this IDbConnection db, int id, CustomBasicList<UpdateEntity> updateList, IDbTransaction? thisTran = null, int? connectionTimeOut = null) where E : class, ISimpleDapperEntity
        {
            var (sqls, ParameterMappings) = GetUpdateStatement<E>(updateList);
            DapperSQLData thisData = new DapperSQLData();
            thisData.SQLStatement = sqls;
            thisData.Parameters.Add("ID", id);
            PopulateSimple(ParameterMappings, thisData, EnumCategory.UseDatabaseMapping);
            await db.ExecuteAsync(sqls, thisData.Parameters, thisTran, connectionTimeOut);
        }
    }
}