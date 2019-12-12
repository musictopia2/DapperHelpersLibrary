using CommonBasicStandardLibraries.BasicDataSettingsAndProcesses;
using CommonBasicStandardLibraries.CollectionClasses;
using CommonBasicStandardLibraries.DatabaseHelpers.ConditionClasses;
using CommonBasicStandardLibraries.DatabaseHelpers.EntityInterfaces;
using CommonBasicStandardLibraries.DatabaseHelpers.MiscClasses;
using CommonBasicStandardLibraries.DatabaseHelpers.MiscInterfaces;
using CommonBasicStandardLibraries.Exceptions;
using Dapper;
using DapperHelpersLibrary.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks; //most of the time, i will be using asyncs.
using static CommonBasicStandardLibraries.BasicDataSettingsAndProcesses.BasicDataFunctions;
using static DapperHelpersLibrary.SQLHelpers.StatementFactoryUpdates;
namespace DapperHelpersLibrary
{
    public class ConnectionHelper
    {
        //no more static functions because its now part of common functions.
        #region Main Functions
        //this only supports sql server and sqlite for now.
        private readonly EnumDatabaseCategory _category;
        private readonly string _connectionString;
        public static ConnectionHelper GetSQLiteTestHelper()
        {
            return new ConnectionHelper();
        }
        private ConnectionHelper()
        {
            IsTesting = true;
            _connectionString = GetInMemorySQLiteString();
            _category = EnumDatabaseCategory.SQLite; //only sqlite can be used for testing
            _connection = cons!.Resolve<IDbConnector>();
        }
        private readonly bool IsTesting;
        private string GetInMemorySQLiteString()
        {
            return "Data Source=:memory:";
        }
        readonly IDbConnector _connection;
        public ConnectionHelper(EnumDatabaseCategory category, string pathOrDatabaseName)
        {
            if (IsTesting == true)
                throw new BasicBlankException("You already decided to test this");
            if (category == EnumDatabaseCategory.SQLServer)
            {
                ISQLServer sqls = cons!.Resolve<ISQLServer>();
                _connectionString = sqls.GetConnectionString(pathOrDatabaseName);
            }
            else
            {
                _connectionString = $@"Data Source = {pathOrDatabaseName}";
            }
            _connection = cons!.Resolve<IDbConnector>();
            _category = category;
        }
        public IDbConnection GetConnection() //if you want the most flexibility
        {
            if (_category == EnumDatabaseCategory.SQLite)
            {
                IDbConnection output = _connection.GetConnection(EnumDatabaseCategory.SQLite, _connectionString);
                //IDbConnection output = new SQLiteConnection(ConnectionString);
                if (IsTesting == true)
                    output.Open(); //for testing, has to open connection.
                output.Dispose(); //i think.
                return _connection.GetConnection(EnumDatabaseCategory.SQLite, _connectionString);
            }
            else if (_category == EnumDatabaseCategory.SQLServer)
            {
                if (IsTesting == true)
                    throw new BasicBlankException("You can't be testing on a sql server database");
                return _connection.GetConnection(EnumDatabaseCategory.SQLServer, _connectionString);
            }
            else
            {
                throw new BasicBlankException("Only SQL Server And SQLite Databases Are Currently Supported");
            }
        }
        #endregion
        #region Work Functions
        public void DoWork(Action<IDbConnection> action)
        {
            using IDbConnection cons = GetConnection();
            cons.Open(); //i think we should be in a habit of opening/closing transactions.
            action.Invoke(cons);
            cons.Close();
        }
        public async Task DoWorkAsync(Func<IDbConnection, Task> action)
        {
            using IDbConnection cons = GetConnection();
            cons.Open(); //i think we should be in a habit of opening/closing transactions.
            await action.Invoke(cons);
            cons.Close();
        }
        public async Task DoBulkWorkAsync(Func<IDbConnection, IDbTransaction, Task> action,
            IsolationLevel isolationLevel = IsolationLevel.Unspecified)
        {
            using IDbConnection cons = GetConnection();
            cons.Open();
            if (isolationLevel == IsolationLevel.Unspecified)
            {

                using IDbTransaction tran = cons.BeginTransaction();
                await action.Invoke(cons, tran); //the client is responsible for committing or rolling back transaction.
            }
            else
            {
                IDbTransaction tran = cons.BeginTransaction(isolationLevel);
                await action.Invoke(cons, tran);  //clients are responsible for commiting the transaction here too.
            }
            cons.Close();
        }
        public void DoBulkWork<E>(Action<IDbConnection, IDbTransaction, E> action,
            ICustomBasicList<E> thisList, IsolationLevel isolationLevel = IsolationLevel.Unspecified,
            Action<IDbConnection>? beforeWork = null, Action<IDbConnection>? afterWork = null)
        {
            using IDbConnection cons = GetConnection();
            cons.Open();
            beforeWork?.Invoke(cons);
            thisList.ForEach(Items =>
            {
                if (isolationLevel == IsolationLevel.Unspecified)
                {
                    using IDbTransaction tran = cons.BeginTransaction();
                    action.Invoke(cons, tran, Items); //the client is responsible for committing or rolling back transaction.
                }
                else
                {
                    IDbTransaction tran = cons.BeginTransaction(isolationLevel);
                    action.Invoke(cons, tran, Items);  //clients are responsible for commiting the transaction here too.
                }
            });
            afterWork?.Invoke(cons);
            cons.Close();
        }
        public void DoWork(Action<IDbConnection, IDbTransaction> action, IsolationLevel isolationLevel = IsolationLevel.Unspecified)
        {
            using IDbConnection cons = GetConnection();
            cons.Open();
            if (isolationLevel == IsolationLevel.Unspecified)
            {
                using IDbTransaction tran = cons.BeginTransaction();
                action.Invoke(cons, tran); //the client is responsible for committing or rolling back transaction
            }
            else
            {
                IDbTransaction tran = cons.BeginTransaction(isolationLevel);
                action.Invoke(cons, tran);
            }
            cons.Close();
        }
        public async Task DoBulkWorkAsync<E>(Func<IDbConnection, IDbTransaction, E, Task> action, ICustomBasicList<E> thisList, IsolationLevel isolationLevel = IsolationLevel.Unspecified,
            Action<IDbConnection>? beforeWork = null, Func<IDbConnection, Task>? afterWork = null)
        {
            using IDbConnection cons = GetConnection();
            cons.Open();
            beforeWork?.Invoke(cons);
            await thisList.ForEachAsync(async Items =>
            {
                if (isolationLevel == IsolationLevel.Unspecified)
                {
                    using IDbTransaction tran = cons.BeginTransaction();
                    await action.Invoke(cons, tran, Items); //the client is responsible for committing or rolling back transaction.
                }
                else
                {
                    IDbTransaction tran = cons.BeginTransaction(isolationLevel);
                    await action.Invoke(cons, tran, Items);
                }
            });
            afterWork?.Invoke(cons);
            cons.Close();
        }
        public async Task DoWorkAsync(Func<IDbConnection, IDbTransaction, Task> action, IsolationLevel isolationLevel = IsolationLevel.Unspecified)
        {
            using IDbConnection cons = GetConnection();
            cons.Open();
            if (isolationLevel == IsolationLevel.Unspecified)
            {
                using IDbTransaction tran = cons.BeginTransaction();
                await action.Invoke(cons, tran); //the client is responsible for committing or rolling back transaction.
            }
            else
            {
                IDbTransaction tran = cons.BeginTransaction(isolationLevel);
                await action.Invoke(cons, tran);
            }
            cons.Close();
        }
        #endregion
        #region Unique Functions
        public async Task UpdateListOnlyAsync<E>(ICustomBasicList<E> updateList, EnumUpdateCategory category = EnumUpdateCategory.Common, IsolationLevel isolationLevel = IsolationLevel.Unspecified) where E : class, ISimpleDapperEntity
        {
            await DoBulkWorkAsync<E>(async (cons, tran, thisEntity) =>
            {
                await cons.UpdateEntityAsync(thisEntity, category: category, thisTran: tran);
                tran.Commit();
            }, updateList, isolationLevel);
        }
        public async Task UpdateListAutoOnlyAsync<E>(CustomBasicList<E> updateList, IsolationLevel isolationLevel = IsolationLevel.Unspecified) where E : class, ISimpleDapperEntity, IUpdatableEntity
        {
            await UpdateListOnlyAsync(updateList, category: EnumUpdateCategory.Auto, isolationLevel);
        }
        public async Task UpdateListOnlyAsync<E>(CustomBasicList<E> updateList, CustomBasicList<UpdateFieldInfo> manuelList, IsolationLevel isolationLevel = IsolationLevel.Unspecified) where E : class, ISimpleDapperEntity
        {
            await DoBulkWorkAsync(async (cons, tran, thisEntity) =>
            {
                await cons.UpdateEntityAsync(thisEntity, manuelList, thisTran: tran);
                tran.Commit(); //i think i forgot this.
            }, updateList, isolationLevel);
        }
        public void UpdateCommonListOnly<E>(CustomBasicList<E> updateList, IsolationLevel isolationLevel = IsolationLevel.Unspecified) where E : class, ISimpleDapperEntity
        {
            DoBulkWork((cons, tran, thisEntity) =>
            {
                cons.UpdateEntity(thisEntity, EnumUpdateCategory.Common, thisTran: tran);
                tran.Commit(); //i think i forgot this.
            }, updateList, isolationLevel);

        }
        public async Task UpdateCommonOnlyAsync<E>(E thisEntity) where E : class, ISimpleDapperEntity
        {
            using IDbConnection cons = GetConnection();
            await cons.UpdateEntityAsync(thisEntity, EnumUpdateCategory.Common);
        }
        public void UpdateCommonOnly<E>(E thisEntity) where E : class, ISimpleDapperEntity
        {
            using IDbConnection cons = GetConnection();
            cons.UpdateEntity(thisEntity, EnumUpdateCategory.Common);
        }
        #endregion
        #region Direct To Extensions Except Get
        public async Task AddListOnlyAsync<E>(CustomBasicList<E> addList, IsolationLevel isolationLevel = IsolationLevel.Unspecified) where E : class, ISimpleDapperEntity
        {
            using IDbConnection cons = GetConnection();
            cons.Open();
            if (isolationLevel == IsolationLevel.Unspecified)
            {
                using IDbTransaction tran = cons.BeginTransaction();
                await cons.InsertRangeAsync(addList, tran, _connection);
                tran.Commit();
            }
            else
            {
                IDbTransaction tran = cons.BeginTransaction(isolationLevel);
                await cons.InsertRangeAsync(addList, tran, _connection);
                tran.Commit();
            }
            cons.Close();
        }
        public async Task AddEntityAsync<E>(E thisEntity) where E : class, ISimpleDapperEntity
        {
            using IDbConnection cons = GetConnection();
            thisEntity.ID = await cons.InsertSingleAsync(thisEntity, _connection); //i think if doing it this way, let this give the id.
        }
        public void AddEntity<E>(E thisEntity) where E : class, ISimpleDapperEntity
        {
            using IDbConnection cons = GetConnection();
            thisEntity.ID = cons.InsertSingle(thisEntity, _connection); //i think if doing it this way, let this give the id.
        }
        public void DeleteOnly<E>(E thisEntity) where E : class, ISimpleDapperEntity
        {
            using IDbConnection cons = GetConnection();
            cons.Delete(thisEntity);
        }
        public async Task DeleteOnlyAsync<E>(E thisEntity) where E : class, ISimpleDapperEntity
        {
            using IDbConnection cons = GetConnection();
            await cons.DeleteAsync(thisEntity);
        }
        public void DeleteOnly<E>(int id) where E : class, ISimpleDapperEntity
        {
            using IDbConnection cons = GetConnection();
            cons.Delete<E>(id);
        }
        public async Task DeleteOnlyAsync<E>(int id) where E : class, ISimpleDapperEntity
        {
            using IDbConnection cons = GetConnection();
            await cons.DeleteAsync<E>(id);
        }
        public async Task ExecuteAsync(string sqls) //in this case, can't be in transaction obviously
        {
            await DoWorkAsync(async cons =>
            {
                await cons.ExecuteAsync(sqls);
            });
        }
        public async Task ExecuteAsync(CustomBasicList<string> sqlList, IsolationLevel isolationLevel = IsolationLevel.Unspecified)
        {
            await DoWorkAsync(async (cons, trans) =>
            {
                await sqlList.ForEachAsync(async items =>
                {
                    await cons.ExecuteAsync(items, null, trans);
                });
                trans.Commit();
            }, isolationLevel);
        }
        public bool Exists<E>(CustomBasicList<ICondition> conditions) where E : class
        {
            bool rets = false;
            DoWork(cons =>
            {
                rets = cons.Exists<E>(conditions, _connection);
            });
            return rets;
        }
        #endregion
        #region Direct To Extensions For Getting
        //this is when you only need to get something and do nothing else.
        public R GetSingleObject<E, R>(string property, CustomBasicList<SortInfo> sortList, CustomBasicList<ICondition>? conditions = null) where E : class, ISimpleDapperEntity
        {
            using IDbConnection cons = GetConnection();
            return cons.GetSingleObject<E, R>(property, sortList, _connection, conditions);
        }
        public async Task<R> GetSingleObjectAsync<E, R>(string property, CustomBasicList<SortInfo> sortList, CustomBasicList<ICondition>? conditions = null) where E : class, ISimpleDapperEntity
        {
            using IDbConnection cons = GetConnection();
            return await cons.GetSingleObjectAsync<E, R>(property, sortList, _connection, conditions);
        }
        public CustomBasicList<R> GetObjectList<E, R>(string property, CustomBasicList<ICondition>? conditions = null, CustomBasicList<SortInfo>? sortList = null, int howMany = 0) where E : class
        {
            using IDbConnection cons = GetConnection();
            return cons.GetObjectList<E, R>(property, _connection, conditions, sortList, howMany);
        }
        public async Task<CustomBasicList<R>> GetObjectListAsync<E, R>(string property, CustomBasicList<ICondition>? conditions = null, CustomBasicList<SortInfo>? sortList = null, int howMany = 0) where E : class
        {
            using IDbConnection cons = GetConnection();
            return await cons.GetObjectListAsync<E, R>(property, _connection, conditions, sortList, howMany);
        }
        public E Get<E>(int id) where E : class
        {
            using IDbConnection cons = GetConnection();
            return cons.Get<E>(id, _connection);
        }
        public IEnumerable<E> Get<E>(CustomBasicList<SortInfo>? sortList = null, int howMany = 0) where E : class
        {
            using IDbConnection cons = GetConnection();
            return cons.Get<E>(_connection, sortList, howMany);
        }
        public async Task<E> GetAsync<E>(int id) where E : class
        {
            using IDbConnection cons = GetConnection();
            return await cons.GetAsync<E>(id, _connection);
        }
        public async Task<IEnumerable<E>> GetAsync<E>(CustomBasicList<SortInfo>? sortList = null, int howMany = 0) where E : class
        {
            using IDbConnection cons = GetConnection();
            return await cons.GetAsync<E>(_connection, sortList, howMany);
        }
        public E Get<E, D1>(int id) where E : class, IJoinedEntity where D1 : class
        {
            using IDbConnection cons = GetConnection();
            return cons.Get<E, D1>(id, _connection);
        }
        public IEnumerable<E> Get<E, D1>(CustomBasicList<SortInfo>? sortList = null, int howMany = 0) where E : class, IJoinedEntity where D1 : class
        {
            using IDbConnection cons = GetConnection();
            return cons.Get<E, D1>(sortList, _connection, howMany);
        }
        public async Task<E> GetAsync<E, D1>(int id) where E : class, IJoinedEntity where D1 : class
        {
            using IDbConnection cons = GetConnection();
            return await cons.GetAsync<E, D1>(id, _connection);
        }
        public async Task<IEnumerable<E>> GetAsync<E, D1>(CustomBasicList<SortInfo>? sortList, int howMany = 0) where E : class, IJoinedEntity where D1 : class
        {
            using IDbConnection cons = GetConnection();
            return await cons.GetAsync<E, D1>(sortList, _connection, howMany);
        }
        public E Get<E, D1, D2>(int id) where E : class, IJoin3Entity<D1, D2> where D1 : class where D2 : class
        {
            using IDbConnection cons = GetConnection();
            return cons.Get<E, D1, D2>(id, _connection);
        }
        public IEnumerable<E> Get<E, D1, D2>(CustomBasicList<SortInfo> sortList, int howMany = 0) where E : class, IJoin3Entity<D1, D2> where D1 : class where D2 : class
        {
            using IDbConnection cons = GetConnection();
            return cons.Get<E, D1, D2>(sortList, _connection, howMany);
        }
        public async Task<E> GetAsync<E, D1, D2>(int id) where E : class, IJoin3Entity<D1, D2> where D1 : class where D2 : class
        {
            using IDbConnection cons = GetConnection();
            return await cons.GetAsync<E, D1, D2>(id, _connection);
        }
        public async Task<IEnumerable<E>> GetAsync<E, D1, D2>(CustomBasicList<SortInfo> sortList, int howMany = 0) where E : class, IJoin3Entity<D1, D2> where D1 : class where D2 : class
        {
            using IDbConnection cons = GetConnection();
            return await cons.GetAsync<E, D1, D2>(sortList, _connection, howMany);
        }
        public E GetOneToMany<E, D1>(int id) where E : class, IJoinedEntity where D1 : class
        {
            using IDbConnection cons = GetConnection();
            return cons.GetOneToMany<E, D1>(id, _connection);
        }
        public IEnumerable<E> GetOneToMany<E, D1>(CustomBasicList<SortInfo>? sortList = null) where E : class, IJoinedEntity where D1 : class
        {
            using IDbConnection cons = GetConnection();
            return cons.GetOneToMany<E, D1>(_connection, sortList);
        }
        public async Task<E> GetOneToManyAsync<E, D1>(int id) where E : class, IJoinedEntity where D1 : class
        {
            using IDbConnection cons = GetConnection();
            return await cons.GetOneToManyAsync<E, D1>(id, _connection);
        }
        public async Task<IEnumerable<E>> GetOneToManyAsync<E, D1>(CustomBasicList<SortInfo>? sortList = null) where E : class, IJoinedEntity where D1 : class
        {
            using IDbConnection cons = GetConnection();
            return await cons.GetOneToManyAsync<E, D1>(_connection, sortList);
        }
        public CustomBasicList<E> Get<E>(CustomBasicList<ICondition> conditions, CustomBasicList<SortInfo>? sortList = null, int howMany = 0) where E : class
        {
            using IDbConnection cons = GetConnection();
            return cons.Get<E>(conditions, _connection, sortList, howMany);
        }
        public async Task<CustomBasicList<E>> GetAsync<E>(CustomBasicList<ICondition> conditions, CustomBasicList<SortInfo>? sortList = null, int howMany = 0) where E : class
        {
            using IDbConnection cons = GetConnection();
            return await cons.GetAsync<E>(conditions, _connection, sortList, howMany);
        }
        public CustomBasicList<E> GetOneToMany<E, D1>(CustomBasicList<ICondition> conditionList, CustomBasicList<SortInfo>? sortList = null) where E : class, IJoinedEntity where D1 : class
        {
            using IDbConnection cons = GetConnection();
            return cons.GetOneToMany<E, D1>(conditionList, _connection, sortList);
        }
        public CustomBasicList<E> GetOneToMany<E, D1, D2>(CustomBasicList<ICondition> conditionList, CustomBasicList<SortInfo>? sortList = null) where E : class, IJoin3Entity<D1, D2> where D1 : class where D2 : class
        {
            using IDbConnection cons = GetConnection();
            return cons.GetOneToMany<E, D1, D2>(conditionList, _connection, sortList);
        }
        public async Task<CustomBasicList<E>> GetOneToManyAsync<E, D1>(CustomBasicList<ICondition> conditionList, CustomBasicList<SortInfo>? sortList = null) where E : class, IJoinedEntity where D1 : class
        {
            using IDbConnection cons = GetConnection();
            return await cons.GetOneToManyAsync<E, D1>(conditionList, _connection, sortList);
        }
        public async Task<CustomBasicList<E>> GetOneToManyAsync<E, D1, D2>(CustomBasicList<ICondition> conditionList, CustomBasicList<SortInfo>? sortList = null) where E : class, IJoin3Entity<D1, D2> where D1 : class where D2 : class
        {
            using IDbConnection cons = GetConnection();
            return await GetOneToManyAsync<E, D1, D2>(conditionList, sortList);
        }
        public IEnumerable<E> Get<E, D1>(CustomBasicList<ICondition> conditionList, CustomBasicList<SortInfo>? sortList = null, int howMany = 0) where E : class, IJoinedEntity where D1 : class
        {
            using IDbConnection cons = GetConnection();
            return cons.Get<E, D1>(conditionList, _connection, sortList, howMany);
        }
        public async Task<IEnumerable<E>> GetAsync<E, D1>(CustomBasicList<ICondition> conditionList, CustomBasicList<SortInfo>? sortList = null, int howMany = 0) where E : class, IJoinedEntity where D1 : class
        {
            using IDbConnection cons = GetConnection();
            return await cons.GetAsync<E, D1>(conditionList, _connection, sortList, howMany);
        }
        public IEnumerable<E> Get<E, D1, D2>(CustomBasicList<ICondition> conditionList, CustomBasicList<SortInfo>? sortList = null, int howMany = 0) where E : class, IJoin3Entity<D1, D2> where D1 : class where D2 : class
        {
            using IDbConnection cons = GetConnection();
            return cons.Get<E, D1, D2>(conditionList, _connection, sortList, howMany);
        }
        public async Task<IEnumerable<E>> GetAsync<E, D1, D2>(CustomBasicList<ICondition> conditionList, CustomBasicList<SortInfo>? sortList = null, int howMany = 0) where E : class, IJoin3Entity<D1, D2> where D1 : class where D2 : class
        {
            using IDbConnection cons = GetConnection();
            return await cons.GetAsync<E, D1, D2>(conditionList, _connection, sortList, howMany);
        }
        #endregion
    }
}