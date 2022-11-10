namespace DapperHelpersLibrary;
public class ConnectionHelper : ISqliteManuelDataAccess, ISqlServerManuelDataAccess
{
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
        Category = EnumDatabaseCategory.SQLite;
        if (GlobalClass.SQLiteConnector is null)
        {
            throw new CustomBasicException("You never registered the interface for sqlite data connector");
        }
        GetConnector = GlobalClass.SQLiteConnector;
    }
    public ConnectionHelper(EnumDatabaseCategory category)
    {
        if (_isTesting)
        {
            throw new CustomBasicException("You already decided to test this");
        }
        Category = category;
        GetConnector = PrivateConnector();
    }
    public async Task InitAsync(string key, ISimpleConfig config)
    {
        if (_isTesting)
        {
            throw new CustomBasicException("You already decided to test this");
        }
        _connectionString = await config.GetStringAsync(key);
    }
    private readonly bool _isTesting;
    private static string GetInMemorySQLiteString()
    {
        return "Data Source=:memory:";
    }
    public IDbConnector GetConnector { get; private set; }
    //private bool _processing;
    //public ConnectionHelper(EnumDatabaseCategory category, string key, ISimpleConfig config)
    //{
    //    if (_isTesting == true)
    //    {
    //        throw new CustomBasicException("You already decided to test this");
    //    }
    //    _processing = true;
    //    SetUpStandard(category, key, config);
    //    do
    //    {
    //        if (_processing == false)
    //        {
    //            break;
    //        }
    //        Thread.Sleep(10);
    //    } while (true);
    //    GetConnector = PrivateConnector();
    //}
    private IDbConnector PrivateConnector()
    {
        if (Category == EnumDatabaseCategory.SQLServer)
        {
            if (GlobalClass.SQLServerConnector is null)
            {
                throw new CustomBasicException("You never registered the interface for sql server data connector");
            }
            return GlobalClass.SQLServerConnector;
        }
        if (Category == EnumDatabaseCategory.SQLite)
        {
            if (GlobalClass.SQLiteConnector is null)
            {
                throw new CustomBasicException("You never registered the interface for sqlite data connector");
            }
            return GlobalClass.SQLiteConnector;
        }
        if (Category == EnumDatabaseCategory.MySQL)
        {
            if (GlobalClass.MySQLConnector is null)
            {
                throw new CustomBasicException("You never registered the interface for mysql data connector");
            }
            return GlobalClass.MySQLConnector;
        }
        throw new CustomBasicException("The data connector currently is unknown.  May require rethinking");
    }
    //public ConnectionHelper(ISimpleConfig config, Func<EnumDatabaseCategory, string> key, Func<Task<EnumDatabaseCategory>> functs)
    //{
    //    if (_isTesting == true)
    //    {
    //        throw new CustomBasicException("You already decided to test this");
    //    }
    //    _processing = true;
    //    SetUpStandard(config, key, functs);
    //    do
    //    {
    //        if (_processing == false)
    //        {
    //            break;
    //        }
    //        Thread.Sleep(10);
    //    } while (true);
    //    GetConnector = PrivateConnector();
    //}
    //private async void SetUpStandard(ISimpleConfig config, Func<EnumDatabaseCategory, string> key, Func<Task<EnumDatabaseCategory>> functs)
    //{
    //    Category = await functs(); //the category first.  that will determine the key as well.
    //    string temps = key(Category);
    //    _connectionString = await config.GetStringAsync(temps);
    //    _processing = false;
    //}
    //private async void SetUpStandard(EnumDatabaseCategory category, string key, ISimpleConfig config)
    //{
    //    _connectionString = await config.GetStringAsync(key);
    //    Category = category;
    //    _processing = false;
    //}
    public ConnectionHelper(EnumDatabaseCategory category, string pathOrDatabaseName)
    {
        if (_isTesting == true)
        {
            throw new CustomBasicException("You already decided to test this");
        }
        if (category == EnumDatabaseCategory.SQLServer)
        {
            if (SQLServerConnectionClass is null)
            {
                throw new CustomBasicException("You never registered the ISQLServer Interface.  Try setting the variable to the basicdatafunctions static class the variable to represent how you are connecting to the database");
            }
            _connectionString = SQLServerConnectionClass.GetConnectionString(pathOrDatabaseName);
        }
        else
        {
            _connectionString = $@"Data Source = {GetCleanedPath(pathOrDatabaseName)}";
        }
        Category = category;
        GetConnector = PrivateConnector();
    }
    public IDbConnection GetConnection()
    {
        if (Category == EnumDatabaseCategory.SQLite)
        {
            IDbConnection output = GetConnector.GetConnection(EnumDatabaseCategory.SQLite, _connectionString);
            if (_isTesting == true)
            {
                output.Open();
            }
            output.Dispose();
            return GetConnector.GetConnection(EnumDatabaseCategory.SQLite, _connectionString);
        }
        else if (Category == EnumDatabaseCategory.SQLServer)
        {
            if (_isTesting == true)
            {
                throw new CustomBasicException("You can't be testing on a sql server database");
            }
            return GetConnector.GetConnection(EnumDatabaseCategory.SQLServer, _connectionString);
        }
        else if (Category == EnumDatabaseCategory.MySQL)
        {
            if (_isTesting == true)
            {
                throw new CustomBasicException("You can't be testing on mySQL database");
            }
            return GetConnector.GetConnection(EnumDatabaseCategory.MySQL, _connectionString);
        }
        else
        {
            throw new CustomBasicException("Only SQL Server, SQLite, and MySQL Databases Are Currently Supported");
        }
    }
    #endregion
    #region Work Functions
    public void DoWork(Action<IDbConnection> action)
    {
        using IDbConnection cons = GetConnection();
        cons.Open();
        action.Invoke(cons);
        cons.Close();
    }
    public async Task DoWorkAsync(Func<IDbConnection, Task> action)
    {
        using IDbConnection cons = GetConnection();
        cons.Open();
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
            await action.Invoke(cons, tran);
        }
        else
        {
            IDbTransaction tran = cons.BeginTransaction(isolationLevel);
            await action.Invoke(cons, tran);
        }
        cons.Close();
    }
    public void DoBulkWork<E>(Action<IDbConnection, IDbTransaction, E> action,
        BasicList<E> thisList, IsolationLevel isolationLevel = IsolationLevel.Unspecified,
        Action<IDbConnection>? beforeWork = null, Action<IDbConnection>? afterWork = null)
    {
        using IDbConnection cons = GetConnection();
        cons.Open();
        beforeWork?.Invoke(cons);
        thisList.ForEach(items =>
        {
            if (isolationLevel == IsolationLevel.Unspecified)
            {
                using IDbTransaction tran = cons.BeginTransaction();
                action.Invoke(cons, tran, items);
            }
            else
            {
                IDbTransaction tran = cons.BeginTransaction(isolationLevel);
                action.Invoke(cons, tran, items); 
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
            action.Invoke(cons, tran);
        }
        else
        {
            IDbTransaction tran = cons.BeginTransaction(isolationLevel);
            action.Invoke(cons, tran);
        }
        cons.Close();
    }
    public async Task DoBulkWorkAsync<E>(Func<IDbConnection, IDbTransaction, E, Task> action, BasicList<E> thisList, IsolationLevel isolationLevel = IsolationLevel.Unspecified,
        Action<IDbConnection>? beforeWork = null, Func<IDbConnection, Task>? afterWork = null)
    {
        using IDbConnection cons = GetConnection();
        cons.Open();
        beforeWork?.Invoke(cons);
        await thisList.ForEachAsync(async items =>
        {
            if (isolationLevel == IsolationLevel.Unspecified)
            {
                using IDbTransaction tran = cons.BeginTransaction();
                await action.Invoke(cons, tran, items);
            }
            else
            {
                IDbTransaction tran = cons.BeginTransaction(isolationLevel);
                await action.Invoke(cons, tran, items);
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
            await action.Invoke(cons, tran);
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
    public async Task UpdateListOnlyAsync<E>(BasicList<E> updateList, EnumUpdateCategory category = EnumUpdateCategory.Common, IsolationLevel isolationLevel = IsolationLevel.Unspecified) where E : class, ISimpleDapperEntity
    {
        await DoBulkWorkAsync<E>(async (cons, tran, thisEntity) =>
        {
            await cons.UpdateEntityAsync(thisEntity, category: category, thisTran: tran);
            tran.Commit();
        }, updateList, isolationLevel);
    }
    public async Task UpdateListAutoOnlyAsync<E>(BasicList<E> updateList, IsolationLevel isolationLevel = IsolationLevel.Unspecified) where E : class, ISimpleDapperEntity, IUpdatableEntity
    {
        await UpdateListOnlyAsync(updateList, category: EnumUpdateCategory.Auto, isolationLevel);
    }
    public async Task UpdateListOnlyAsync<E>(BasicList<E> updateList, BasicList<UpdateFieldInfo> manuelList, IsolationLevel isolationLevel = IsolationLevel.Unspecified) where E : class, ISimpleDapperEntity
    {
        await DoBulkWorkAsync(async (cons, tran, thisEntity) =>
        {
            await cons.UpdateEntityAsync(thisEntity, manuelList, thisTran: tran);
            tran.Commit();
        }, updateList, isolationLevel);
    }
    public void UpdateCommonListOnly<E>(BasicList<E> updateList, IsolationLevel isolationLevel = IsolationLevel.Unspecified) where E : class, ISimpleDapperEntity
    {
        DoBulkWork((cons, tran, thisEntity) =>
        {
            cons.UpdateEntity(thisEntity, EnumUpdateCategory.Common, thisTran: tran);
            tran.Commit();
        }, updateList, isolationLevel);
    }
    public async Task UpdateCommonOnlyAsync<E>(E thisEntity) where E : class, ISimpleDapperEntity
    {
        using IDbConnection cons = GetConnection();
        await cons.UpdateEntityAsync(thisEntity, EnumUpdateCategory.Common);
    }

    public async Task UpdateAllAsync<E>(E thisEntity) where E : class, ISimpleDapperEntity
    {
        using IDbConnection cons = GetConnection();
        await cons.UpdateEntityAsync(thisEntity, EnumUpdateCategory.All);
    }
    public void UpdateCommonOnly<E>(E thisEntity) where E : class, ISimpleDapperEntity
    {
        using IDbConnection cons = GetConnection();
        cons.UpdateEntity(thisEntity, EnumUpdateCategory.Common);
    }
    public void UpdateAll<E>(E thisEntity) where E : class, ISimpleDapperEntity
    {
        using IDbConnection cons = GetConnection();
        cons.UpdateEntity(thisEntity, EnumUpdateCategory.All);
    }
    public async Task InsertRangeAsync<E>(BasicList<E> insertList, IsolationLevel isolationLevel = IsolationLevel.Unspecified, bool isStarterData = false, Action? recordsExisted = null) where E : class, ISimpleDapperEntity
    {
        await DoWorkAsync(async (cons, tran) =>
        {
            if (isStarterData)
            {
                int count = cons.Count<E>(GetConnector, tran);
                if (count > 0)
                {
                    recordsExisted?.Invoke();
                    return;
                }
            }
            await cons.InsertRangeAsync(insertList, tran, GetConnector);
            tran.Commit();
        }, isolationLevel);
    }
    public async Task<int> InsertAsync<E>(E entity) where E : class, ISimpleDapperEntity
    {
        using IDbConnection cons = GetConnection();
        return await cons.InsertSingleAsync(entity, GetConnector);
    } 
    public int Insert<E>(E entity) where E : class, ISimpleDapperEntity
    {
        using IDbConnection cons = GetConnection();
        return cons.InsertSingle(entity, GetConnector);
    }
    public void InsertRange<E>(BasicList<E> insertList, IsolationLevel isolationLevel = IsolationLevel.Unspecified, bool isStarterData = false) where E : class, ISimpleDapperEntity
    {
        DoWork((cons, tran) =>
        {
            if (isStarterData)
            {
                int count = cons.Count<E>(GetConnector, tran);
                if (count > 0)
                {
                    return;
                }
            }
            cons.InsertRange(insertList, tran, GetConnector);
        }, isolationLevel);
    }
    #endregion
    #region Direct To Extensions Except Get
    public async Task AddListOnlyAsync<E>(BasicList<E> addList, IsolationLevel isolationLevel = IsolationLevel.Unspecified) where E : class, ISimpleDapperEntity
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
        thisEntity.ID = await cons.InsertSingleAsync(thisEntity, GetConnector);
    }
    public void AddEntity<E>(E thisEntity) where E : class, ISimpleDapperEntity
    {
        using IDbConnection cons = GetConnection();
        thisEntity.ID = cons.InsertSingle(thisEntity, GetConnector);
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
    public void DeleteOnly<E>(BasicList<ICondition> conditions) where E : class, ISimpleDapperEntity
    {
        using IDbConnection cons = GetConnection();
        cons.Delete<E>(conditions, conn: GetConnector);
    }
    public async Task DeleteOnlyAsync<E>(BasicList<ICondition> conditions) where E : class, ISimpleDapperEntity
    {
        using IDbConnection cons = GetConnection();
        await cons.DeleteAsync<E>(conditions, conn: GetConnector);
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
    public async Task ExecuteAsync(string sqls)
    {
        await DoWorkAsync(async cons =>
        {
            await cons.ExecuteAsync(sqls);
        });
    }
    public async Task ExecuteAsync(BasicList<string> sqlList, IsolationLevel isolationLevel = IsolationLevel.Unspecified)
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
    public bool Exists<E>(BasicList<ICondition> conditions) where E : class
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
    public R GetSingleObject<E, R>(string property, BasicList<SortInfo> sortList, BasicList<ICondition>? conditions = null) where E : class, ISimpleDapperEntity
    {
        using IDbConnection cons = GetConnection();
        return cons.GetSingleObject<E, R>(property, sortList, GetConnector, conditions);
    }
    public async Task<R> GetSingleObjectAsync<E, R>(string property, BasicList<SortInfo> sortList, BasicList<ICondition>? conditions = null) where E : class, ISimpleDapperEntity
    {
        using IDbConnection cons = GetConnection();
        return await cons.GetSingleObjectAsync<E, R>(property, sortList, GetConnector, conditions);
    }
    public BasicList<R> GetObjectList<E, R>(string property, BasicList<ICondition>? conditions = null, BasicList<SortInfo>? sortList = null, int howMany = 0) where E : class
    {
        using IDbConnection cons = GetConnection();
        return cons.GetObjectList<E, R>(property, GetConnector, conditions, sortList, howMany);
    }
    public async Task<BasicList<R>> GetObjectListAsync<E, R>(string property, BasicList<ICondition>? conditions = null, BasicList<SortInfo>? sortList = null, int howMany = 0) where E : class
    {
        using IDbConnection cons = GetConnection();
        return await cons.GetObjectListAsync<E, R>(property, GetConnector, conditions, sortList, howMany);
    }
    public E Get<E>(int id) where E : class
    {
        using IDbConnection cons = GetConnection();
        return cons.Get<E>(id, GetConnector);
    }
    public IEnumerable<E> Get<E>(BasicList<SortInfo>? sortList = null, int howMany = 0) where E : class
    {
        using IDbConnection cons = GetConnection();
        return cons.Get<E>(GetConnector, sortList, howMany);
    }
    public async Task<E> GetAsync<E>(int id) where E : class
    {
        using IDbConnection cons = GetConnection();
        return await cons.GetAsync<E>(id, GetConnector);
    }
    public async Task<IEnumerable<E>> GetAsync<E>(BasicList<SortInfo>? sortList = null, int howMany = 0) where E : class
    {
        using IDbConnection cons = GetConnection();
        return await cons.GetAsync<E>(GetConnector, sortList, howMany);
    }
    public E Get<E, D1>(int id) where E : class, IJoinedEntity where D1 : class
    {
        using IDbConnection cons = GetConnection();
        return cons.Get<E, D1>(id, GetConnector);
    }
    public IEnumerable<E> Get<E, D1>(BasicList<SortInfo>? sortList = null, int howMany = 0) where E : class, IJoinedEntity where D1 : class
    {
        using IDbConnection cons = GetConnection();
        return cons.Get<E, D1>(sortList, GetConnector, howMany);
    }
    public async Task<E> GetAsync<E, D1>(int id) where E : class, IJoinedEntity where D1 : class
    {
        using IDbConnection cons = GetConnection();
        return await cons.GetAsync<E, D1>(id, GetConnector);
    }
    public async Task<IEnumerable<E>> GetAsync<E, D1>(BasicList<SortInfo>? sortList, int howMany = 0) where E : class, IJoinedEntity where D1 : class
    {
        using IDbConnection cons = GetConnection();
        return await cons.GetAsync<E, D1>(sortList, GetConnector, howMany);
    }
    public E Get<E, D1, D2>(int id) where E : class, IJoin3Entity<D1, D2> where D1 : class where D2 : class
    {
        using IDbConnection cons = GetConnection();
        return cons.Get<E, D1, D2>(id, GetConnector);
    }
    public IEnumerable<E> Get<E, D1, D2>(BasicList<SortInfo> sortList, int howMany = 0) where E : class, IJoin3Entity<D1, D2> where D1 : class where D2 : class
    {
        using IDbConnection cons = GetConnection();
        return cons.Get<E, D1, D2>(sortList, GetConnector, howMany);
    }
    public async Task<E> GetAsync<E, D1, D2>(int id) where E : class, IJoin3Entity<D1, D2> where D1 : class where D2 : class
    {
        using IDbConnection cons = GetConnection();
        return await cons.GetAsync<E, D1, D2>(id, GetConnector);
    }
    public async Task<IEnumerable<E>> GetAsync<E, D1, D2>(BasicList<SortInfo> sortList, int howMany = 0) where E : class, IJoin3Entity<D1, D2> where D1 : class where D2 : class
    {
        using IDbConnection cons = GetConnection();
        return await cons.GetAsync<E, D1, D2>(sortList, GetConnector, howMany);
    }
    public E GetOneToMany<E, D1>(int id) where E : class, IJoinedEntity where D1 : class
    {
        using IDbConnection cons = GetConnection();
        return cons.GetOneToMany<E, D1>(id, GetConnector);
    }
    public IEnumerable<E> GetOneToMany<E, D1>(BasicList<SortInfo>? sortList = null) where E : class, IJoinedEntity where D1 : class
    {
        using IDbConnection cons = GetConnection();
        return cons.GetOneToMany<E, D1>(GetConnector, sortList);
    }
    public async Task<E> GetOneToManyAsync<E, D1>(int id) where E : class, IJoinedEntity where D1 : class
    {
        using IDbConnection cons = GetConnection();
        return await cons.GetOneToManyAsync<E, D1>(id, GetConnector);
    }
    public async Task<IEnumerable<E>> GetOneToManyAsync<E, D1>(BasicList<SortInfo>? sortList = null) where E : class, IJoinedEntity where D1 : class
    {
        using IDbConnection cons = GetConnection();
        return await cons.GetOneToManyAsync<E, D1>(GetConnector, sortList);
    }
    public BasicList<E> Get<E>(BasicList<ICondition> conditions, BasicList<SortInfo>? sortList = null, int howMany = 0) where E : class
    {
        using IDbConnection cons = GetConnection();
        return cons.Get<E>(conditions, GetConnector, sortList, howMany);
    }
    public async Task<BasicList<E>> GetAsync<E>(BasicList<ICondition> conditions, BasicList<SortInfo>? sortList = null, int howMany = 0) where E : class
    {
        using IDbConnection cons = GetConnection();
        return await cons.GetAsync<E>(conditions, GetConnector, sortList, howMany);
    }
    public BasicList<E> GetOneToMany<E, D1>(BasicList<ICondition> conditionList, BasicList<SortInfo>? sortList = null) where E : class, IJoinedEntity where D1 : class
    {
        using IDbConnection cons = GetConnection();
        return cons.GetOneToMany<E, D1>(conditionList, GetConnector, sortList);
    }
    public BasicList<E> GetOneToMany<E, D1, D2>(BasicList<ICondition> conditionList, BasicList<SortInfo>? sortList = null) where E : class, IJoin3Entity<D1, D2> where D1 : class where D2 : class
    {
        using IDbConnection cons = GetConnection();
        return cons.GetOneToMany<E, D1, D2>(conditionList, GetConnector, sortList);
    }
    public async Task<BasicList<E>> GetOneToManyAsync<E, D1>(BasicList<ICondition> conditionList, BasicList<SortInfo>? sortList = null) where E : class, IJoinedEntity where D1 : class
    {
        using IDbConnection cons = GetConnection();
        return await cons.GetOneToManyAsync<E, D1>(conditionList, GetConnector, sortList);
    }
    public async Task<BasicList<E>> GetOneToManyAsync<E, D1, D2>(BasicList<ICondition> conditionList, BasicList<SortInfo>? sortList = null) where E : class, IJoin3Entity<D1, D2> where D1 : class where D2 : class
    {
        using IDbConnection cons = GetConnection();
        return await GetOneToManyAsync<E, D1, D2>(conditionList, sortList);
    }
    public IEnumerable<E> Get<E, D1>(BasicList<ICondition> conditionList, BasicList<SortInfo>? sortList = null, int howMany = 0) where E : class, IJoinedEntity where D1 : class
    {
        using IDbConnection cons = GetConnection();
        return cons.Get<E, D1>(conditionList, GetConnector, sortList, howMany);
    }
    public async Task<IEnumerable<E>> GetAsync<E, D1>(BasicList<ICondition> conditionList, BasicList<SortInfo>? sortList = null, int howMany = 0) where E : class, IJoinedEntity where D1 : class
    {
        using IDbConnection cons = GetConnection();
        return await cons.GetAsync<E, D1>(conditionList, GetConnector, sortList, howMany);
    }
    public IEnumerable<E> Get<E, D1, D2>(BasicList<ICondition> conditionList, BasicList<SortInfo>? sortList = null, int howMany = 0) where E : class, IJoin3Entity<D1, D2> where D1 : class where D2 : class
    {
        using IDbConnection cons = GetConnection();
        return cons.Get<E, D1, D2>(conditionList, GetConnector, sortList, howMany);
    }
    public async Task<IEnumerable<E>> GetAsync<E, D1, D2>(BasicList<ICondition> conditionList, BasicList<SortInfo>? sortList = null, int howMany = 0) where E : class, IJoin3Entity<D1, D2> where D1 : class where D2 : class
    {
        using IDbConnection cons = GetConnection();
        return await cons.GetAsync<E, D1, D2>(conditionList, GetConnector, sortList, howMany);
    }
    /// <summary>
    /// this is when you just want to get all the data from the database.  most simple but no parameters this time.
    /// </summary>
    /// <typeparam name="E"></typeparam>
    /// <returns></returns>
    public async Task<BasicList<E>> GetDataListAsync<E>() where E : class, ISimpleDapperEntity //this should still work.
    {
        BasicList<E> output = new();
        await DoWorkAsync(async cons =>
        {
            var results = await cons.GetAsync<E>(GetConnector);
            output = results.ToBasicList();
        });
        return output;
    }
    public BasicList<T> LoadData<T, U>(string sqlStatement, U parameters)
    {
        using IDbConnection conn = GetConnection();
        BasicList<T> rows = conn.Query<T>(sqlStatement, parameters).ToBasicList();
        return rows;
    }
    public async Task<BasicList<T>> LoadDataAsync<T, U>(string sqlStatement, U parameters)
    {
        using IDbConnection conn = GetConnection();
        var rows = await conn.QueryAsync<T>(sqlStatement, parameters);
        return rows.ToBasicList();
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
    public BasicList<T> LoadData<T, U>(string sqlStatement, U parameters, bool isStoredProcedure)
    {
        using IDbConnection conn = GetConnection();
        CommandType commandType = CommandType.Text;
        if (isStoredProcedure == true)
        {
            commandType = CommandType.StoredProcedure;
        }
        BasicList<T> rows = conn.Query<T>(sqlStatement, parameters, commandType: commandType).ToBasicList();
        return rows;
    }
    public async Task<BasicList<T>> LoadDataAsync<T, U>(string sqlStatement, U parameters, bool isStoredProcedure)
    {
        using IDbConnection conn = GetConnection();
        CommandType commandType = CommandType.Text;
        if (isStoredProcedure == true)
        {
            commandType = CommandType.StoredProcedure;
        }
        var rows = await conn.QueryAsync<T>(sqlStatement, parameters, commandType: commandType);
        return rows.ToBasicList();

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