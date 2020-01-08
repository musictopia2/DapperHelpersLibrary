using CommonBasicStandardLibraries.AdvancedGeneralFunctionsAndProcesses.BasicExtensions;
using CommonBasicStandardLibraries.AdvancedGeneralFunctionsAndProcesses.ConfigProcesses;
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
    public class ConnectionHelper : ISqliteManuelDataAccess, ISqlServerManuelDataAccess
    {
        //no more static functions because its now part of common functions.
        #region Main Functions
        //this only supports sql server and sqlite for now.
        public EnumDatabaseCategory Category;
        private string _connectionString = "";
        public static ConnectionHelper GetSQLiteTestHelper()
        {
            return new ConnectionHelper();
        }
        private ConnectionHelper()
        {
            _isTesting = true;
            _connectionString = GetInMemorySQLiteString();
            Category = EnumDatabaseCategory.SQLite; //only sqlite can be used for testing
            GetConnector = cons!.Resolve<IDbConnector>();
        }
        private readonly bool _isTesting;
        private string GetInMemorySQLiteString()
        {
            return "Data Source=:memory:";
        }
        public IDbConnector GetConnector { get; private set; } //i guess this is fine.
        public ConnectionHelper(EnumDatabaseCategory category, string key, ISimpleConfig config)
        {
            if (_isTesting == true)
                throw new BasicBlankException("You already decided to test this");
            SetUpStandard(category, key, config);
            GetConnector = cons!.Resolve<IDbConnector>(); //risk doing it this way now.
        }
        public ConnectionHelper(ISimpleConfig config, Func<EnumDatabaseCategory, string> key, Func<Task<EnumDatabaseCategory>> functs)
        {
            if (_isTesting == true)
                throw new BasicBlankException("You already decided to test this");

            SetUpStandard(config, key, functs);
            GetConnector = cons!.Resolve<IDbConnector>(); //risk doing it this way now.
        }
        private async void SetUpStandard(ISimpleConfig config, Func<EnumDatabaseCategory, string> key, Func<Task<EnumDatabaseCategory>> functs)
        {
            Category = await functs(); //the category first.  that will determine the key as well.
            string temps = key(Category);
            _connectionString = await config.GetStringAsync(temps);
            
        }
        private async void SetUpStandard(EnumDatabaseCategory category, string key, ISimpleConfig config)
        {
            _connectionString = await config.GetStringAsync(key);
            Category = category;
        }
        
        public ConnectionHelper(EnumDatabaseCategory category, string pathOrDatabaseName)
        {
            if (_isTesting == true)
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
            GetConnector = cons!.Resolve<IDbConnector>();
            Category = category;
        }
        public IDbConnection GetConnection() //if you want the most flexibility
        {
            if (Category == EnumDatabaseCategory.SQLite)
            {
                IDbConnection output = GetConnector.GetConnection(EnumDatabaseCategory.SQLite, _connectionString);
                //IDbConnection output = new SQLiteConnection(ConnectionString);
                if (_isTesting == true)
                    output.Open(); //for testing, has to open connection.
                output.Dispose(); //i think.
                //CommonBasicStandardLibraries.BasicDataSettingsAndProcesses.VBCompat.Stop();
                return GetConnector.GetConnection(EnumDatabaseCategory.SQLite, _connectionString);
            }
            else if (Category == EnumDatabaseCategory.SQLServer)
            {
                if (_isTesting == true)
                    throw new BasicBlankException("You can't be testing on a sql server database");
                return GetConnector.GetConnection(EnumDatabaseCategory.SQLServer, _connectionString);
            }
            else if (Category == EnumDatabaseCategory.MySQL)
            {
                if (_isTesting == true)
                {
                    throw new BasicBlankException("You can't be testing on mySQL database");
                }
                return GetConnector.GetConnection(EnumDatabaseCategory.MySQL, _connectionString);
            }
            else
            {
                throw new BasicBlankException("Only SQL Server, SQLite, and MySQL Databases Are Currently Supported");
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
        public async Task InsertRangeAsync<E>(CustomBasicList<E> insertList, IsolationLevel isolationLevel = IsolationLevel.Unspecified, bool isStarterData = false, Action? recordsExisted = null) where E : class, ISimpleDapperEntity
        {
            await DoWorkAsync(async (cons, tran) =>
            {
                if (isStarterData)
                {
                    int count = cons.Count<E>(GetConnector, tran);
                    if (count > 0)
                    {
                        recordsExisted?.Invoke(); //so a process can do something to show records already existed.
                        return; //because already exist.

                    }
                }
                await cons.InsertRangeAsync(insertList, tran, GetConnector);
                tran.Commit(); //maybe forgot this part (?)
            }, isolationLevel);
        }
        public async Task<int> InsertAsync<E>(E entity) where E: class, ISimpleDapperEntity
        {
            using IDbConnection cons = GetConnection();
            return await cons.InsertSingleAsync(entity, GetConnector); //this will not use transaction.  otherwise, will use another method.
        } //this means i need a new version of this now.
        public int Insert<E>(E entity) where E : class, ISimpleDapperEntity
        {
            using IDbConnection cons = GetConnection();
            return cons.InsertSingle(entity, GetConnector);
        }
        public void InsertRange<E>(CustomBasicList<E> insertList, IsolationLevel isolationLevel = IsolationLevel.Unspecified, bool isStarterData = false) where E : class, ISimpleDapperEntity
        {
            DoWork((cons, tran) =>
            {
                if (isStarterData)
                {
                    int count = cons.Count<E>(GetConnector, tran);
                    if (count > 0)
                        return; //because already exist.
                }
                cons.InsertRange(insertList, tran, GetConnector);
            }, isolationLevel);
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
                await cons.InsertRangeAsync(addList, tran, GetConnector);
                tran.Commit();
            }
            else
            {
                IDbTransaction tran = cons.BeginTransaction(isolationLevel);
                await cons.InsertRangeAsync(addList, tran, GetConnector);
                tran.Commit();
            }
            cons.Close();
        }
        public async Task AddEntityAsync<E>(E thisEntity) where E : class, ISimpleDapperEntity
        {
            using IDbConnection cons = GetConnection();
            thisEntity.ID = await cons.InsertSingleAsync(thisEntity, GetConnector); //i think if doing it this way, let this give the id.
        }
        public void AddEntity<E>(E thisEntity) where E : class, ISimpleDapperEntity
        {
            using IDbConnection cons = GetConnection();
            thisEntity.ID = cons.InsertSingle(thisEntity, GetConnector); //i think if doing it this way, let this give the id.
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
                rets = cons.Exists<E>(conditions, GetConnector);
            });
            return rets;
        }
        #endregion
        #region Direct To Extensions For Getting
        //this is when you only need to get something and do nothing else.

       

        public R GetSingleObject<E, R>(string property, CustomBasicList<SortInfo> sortList, CustomBasicList<ICondition>? conditions = null) where E : class, ISimpleDapperEntity
        {
            using IDbConnection cons = GetConnection();
            return cons.GetSingleObject<E, R>(property, sortList, GetConnector, conditions);
        }
        public async Task<R> GetSingleObjectAsync<E, R>(string property, CustomBasicList<SortInfo> sortList, CustomBasicList<ICondition>? conditions = null) where E : class, ISimpleDapperEntity
        {
            using IDbConnection cons = GetConnection();
            return await cons.GetSingleObjectAsync<E, R>(property, sortList, GetConnector, conditions);
        }
        public CustomBasicList<R> GetObjectList<E, R>(string property, CustomBasicList<ICondition>? conditions = null, CustomBasicList<SortInfo>? sortList = null, int howMany = 0) where E : class
        {
            using IDbConnection cons = GetConnection();
            return cons.GetObjectList<E, R>(property, GetConnector, conditions, sortList, howMany);
        }
        public async Task<CustomBasicList<R>> GetObjectListAsync<E, R>(string property, CustomBasicList<ICondition>? conditions = null, CustomBasicList<SortInfo>? sortList = null, int howMany = 0) where E : class
        {
            using IDbConnection cons = GetConnection();
            return await cons.GetObjectListAsync<E, R>(property, GetConnector, conditions, sortList, howMany);
        }
        public E Get<E>(int id) where E : class
        {
            using IDbConnection cons = GetConnection();
            return cons.Get<E>(id, GetConnector);
        }
        public IEnumerable<E> Get<E>(CustomBasicList<SortInfo>? sortList = null, int howMany = 0) where E : class
        {
            using IDbConnection cons = GetConnection();
            return cons.Get<E>(GetConnector, sortList, howMany);
        }
        public async Task<E> GetAsync<E>(int id) where E : class
        {
            using IDbConnection cons = GetConnection();
            return await cons.GetAsync<E>(id, GetConnector);
        }
        public async Task<IEnumerable<E>> GetAsync<E>(CustomBasicList<SortInfo>? sortList = null, int howMany = 0) where E : class
        {
            using IDbConnection cons = GetConnection();
            return await cons.GetAsync<E>(GetConnector, sortList, howMany);
        }
        public E Get<E, D1>(int id) where E : class, IJoinedEntity where D1 : class
        {
            using IDbConnection cons = GetConnection();
            return cons.Get<E, D1>(id, GetConnector);
        }
        public IEnumerable<E> Get<E, D1>(CustomBasicList<SortInfo>? sortList = null, int howMany = 0) where E : class, IJoinedEntity where D1 : class
        {
            using IDbConnection cons = GetConnection();
            return cons.Get<E, D1>(sortList, GetConnector, howMany);
        }
        public async Task<E> GetAsync<E, D1>(int id) where E : class, IJoinedEntity where D1 : class
        {
            using IDbConnection cons = GetConnection();
            return await cons.GetAsync<E, D1>(id, GetConnector);
        }
        public async Task<IEnumerable<E>> GetAsync<E, D1>(CustomBasicList<SortInfo>? sortList, int howMany = 0) where E : class, IJoinedEntity where D1 : class
        {
            using IDbConnection cons = GetConnection();
            return await cons.GetAsync<E, D1>(sortList, GetConnector, howMany);
        }
        public E Get<E, D1, D2>(int id) where E : class, IJoin3Entity<D1, D2> where D1 : class where D2 : class
        {
            using IDbConnection cons = GetConnection();
            return cons.Get<E, D1, D2>(id, GetConnector);
        }
        public IEnumerable<E> Get<E, D1, D2>(CustomBasicList<SortInfo> sortList, int howMany = 0) where E : class, IJoin3Entity<D1, D2> where D1 : class where D2 : class
        {
            using IDbConnection cons = GetConnection();
            return cons.Get<E, D1, D2>(sortList, GetConnector, howMany);
        }
        public async Task<E> GetAsync<E, D1, D2>(int id) where E : class, IJoin3Entity<D1, D2> where D1 : class where D2 : class
        {
            using IDbConnection cons = GetConnection();
            return await cons.GetAsync<E, D1, D2>(id, GetConnector);
        }
        public async Task<IEnumerable<E>> GetAsync<E, D1, D2>(CustomBasicList<SortInfo> sortList, int howMany = 0) where E : class, IJoin3Entity<D1, D2> where D1 : class where D2 : class
        {
            using IDbConnection cons = GetConnection();
            return await cons.GetAsync<E, D1, D2>(sortList, GetConnector, howMany);
        }
        public E GetOneToMany<E, D1>(int id) where E : class, IJoinedEntity where D1 : class
        {
            using IDbConnection cons = GetConnection();
            return cons.GetOneToMany<E, D1>(id, GetConnector);
        }
        public IEnumerable<E> GetOneToMany<E, D1>(CustomBasicList<SortInfo>? sortList = null) where E : class, IJoinedEntity where D1 : class
        {
            using IDbConnection cons = GetConnection();
            return cons.GetOneToMany<E, D1>(GetConnector, sortList);
        }
        public async Task<E> GetOneToManyAsync<E, D1>(int id) where E : class, IJoinedEntity where D1 : class
        {
            using IDbConnection cons = GetConnection();
            return await cons.GetOneToManyAsync<E, D1>(id, GetConnector);
        }
        public async Task<IEnumerable<E>> GetOneToManyAsync<E, D1>(CustomBasicList<SortInfo>? sortList = null) where E : class, IJoinedEntity where D1 : class
        {
            using IDbConnection cons = GetConnection();
            return await cons.GetOneToManyAsync<E, D1>(GetConnector, sortList);
        }
        public CustomBasicList<E> Get<E>(CustomBasicList<ICondition> conditions, CustomBasicList<SortInfo>? sortList = null, int howMany = 0) where E : class
        {
            using IDbConnection cons = GetConnection();
            return cons.Get<E>(conditions, GetConnector, sortList, howMany);
        }
        public async Task<CustomBasicList<E>> GetAsync<E>(CustomBasicList<ICondition> conditions, CustomBasicList<SortInfo>? sortList = null, int howMany = 0) where E : class
        {
            using IDbConnection cons = GetConnection();
            return await cons.GetAsync<E>(conditions, GetConnector, sortList, howMany);
        }
        public CustomBasicList<E> GetOneToMany<E, D1>(CustomBasicList<ICondition> conditionList, CustomBasicList<SortInfo>? sortList = null) where E : class, IJoinedEntity where D1 : class
        {
            using IDbConnection cons = GetConnection();
            return cons.GetOneToMany<E, D1>(conditionList, GetConnector, sortList);
        }
        public CustomBasicList<E> GetOneToMany<E, D1, D2>(CustomBasicList<ICondition> conditionList, CustomBasicList<SortInfo>? sortList = null) where E : class, IJoin3Entity<D1, D2> where D1 : class where D2 : class
        {
            using IDbConnection cons = GetConnection();
            return cons.GetOneToMany<E, D1, D2>(conditionList, GetConnector, sortList);
        }
        public async Task<CustomBasicList<E>> GetOneToManyAsync<E, D1>(CustomBasicList<ICondition> conditionList, CustomBasicList<SortInfo>? sortList = null) where E : class, IJoinedEntity where D1 : class
        {
            using IDbConnection cons = GetConnection();
            return await cons.GetOneToManyAsync<E, D1>(conditionList, GetConnector, sortList);
        }
        public async Task<CustomBasicList<E>> GetOneToManyAsync<E, D1, D2>(CustomBasicList<ICondition> conditionList, CustomBasicList<SortInfo>? sortList = null) where E : class, IJoin3Entity<D1, D2> where D1 : class where D2 : class
        {
            using IDbConnection cons = GetConnection();
            return await GetOneToManyAsync<E, D1, D2>(conditionList, sortList);
        }
        public IEnumerable<E> Get<E, D1>(CustomBasicList<ICondition> conditionList, CustomBasicList<SortInfo>? sortList = null, int howMany = 0) where E : class, IJoinedEntity where D1 : class
        {
            using IDbConnection cons = GetConnection();
            return cons.Get<E, D1>(conditionList, GetConnector, sortList, howMany);
        }
        public async Task<IEnumerable<E>> GetAsync<E, D1>(CustomBasicList<ICondition> conditionList, CustomBasicList<SortInfo>? sortList = null, int howMany = 0) where E : class, IJoinedEntity where D1 : class
        {
            using IDbConnection cons = GetConnection();
            return await cons.GetAsync<E, D1>(conditionList, GetConnector, sortList, howMany);
        }
        public IEnumerable<E> Get<E, D1, D2>(CustomBasicList<ICondition> conditionList, CustomBasicList<SortInfo>? sortList = null, int howMany = 0) where E : class, IJoin3Entity<D1, D2> where D1 : class where D2 : class
        {
            using IDbConnection cons = GetConnection();
            return cons.Get<E, D1, D2>(conditionList, GetConnector, sortList, howMany);
        }
        public async Task<IEnumerable<E>> GetAsync<E, D1, D2>(CustomBasicList<ICondition> conditionList, CustomBasicList<SortInfo>? sortList = null, int howMany = 0) where E : class, IJoin3Entity<D1, D2> where D1 : class where D2 : class
        {
            using IDbConnection cons = GetConnection();
            return await cons.GetAsync<E, D1, D2>(conditionList, GetConnector, sortList, howMany);
        }
        //decided that this only works via interface.  probably best to keep it separate.
        //changed my mind because sometimes i just want to do directly.
        public CustomBasicList<T> LoadData<T, U>(string sqlStatement, U parameters)
        {
            using IDbConnection conn = GetConnection();
            CustomBasicList<T> rows = conn.Query<T>(sqlStatement, parameters).ToCustomBasicList();
            return rows;
        }
        public async Task<CustomBasicList<T>> LoadDataAsync<T, U>(string sqlStatement, U parameters)
        {
            using IDbConnection conn = GetConnection();
            var rows = await conn.QueryAsync<T>(sqlStatement, parameters);
            return rows.ToCustomBasicList();
        }
        public void SaveData<T>(string sqlStatement, T parameters)
        {
            using IDbConnection conn = GetConnection();
            conn.Execute(sqlStatement, parameters);
        }
        public async Task SaveDataAsync<T>(string sqlStatement, T parameters)
        {
            using IDbConnection conn = GetConnection();
            await conn.ExecuteAsync(sqlStatement, parameters);
        }
        public CustomBasicList<T> LoadData<T, U>(string sqlStatement, U parameters, bool isStoredProcedure)
        {
            using IDbConnection conn = GetConnection();
            CommandType commandType = CommandType.Text;
            if (isStoredProcedure == true)
            {
                commandType = CommandType.StoredProcedure;
            }
            CustomBasicList<T> rows = conn.Query<T>(sqlStatement, parameters, commandType: commandType).ToCustomBasicList();
            return rows;
        }
        public async Task<CustomBasicList<T>> LoadDataAsync<T, U>(string sqlStatement, U parameters, bool isStoredProcedure)
        {
            using IDbConnection conn = GetConnection();
            CommandType commandType = CommandType.Text;
            if (isStoredProcedure == true)
            {
                commandType = CommandType.StoredProcedure;
            }
            var rows = await conn.QueryAsync<T>(sqlStatement, parameters, commandType: commandType);
            return rows.ToCustomBasicList();

        }
        //sql server portion
        public void SaveData<T>(string sqlStatement, T parameters, bool isStoredProcedure)
        {
            using IDbConnection conn = GetConnection();
            CommandType commandType = CommandType.Text;
            if (isStoredProcedure == true)
            {
                commandType = CommandType.StoredProcedure;
            }
            conn.Execute(sqlStatement, parameters, commandType: commandType);
        }
        public async Task SaveDataAsync<T>(string sqlStatement, T parameters, bool isStoredProcedure)
        {
            using IDbConnection conn = GetConnection();
            CommandType commandType = CommandType.Text;
            if (isStoredProcedure == true)
            {
                commandType = CommandType.StoredProcedure;
            }
            await conn.ExecuteAsync(sqlStatement, parameters, commandType: commandType);
        }
        #endregion
    }
}