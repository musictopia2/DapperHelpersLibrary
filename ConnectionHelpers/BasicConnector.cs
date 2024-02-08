namespace DapperHelpersLibrary.ConnectionHelpers;
public class BasicConnector : IConnector
{
    #region Main Functions
    private EnumDatabaseCategory _category;
    private string _connectionString = "";
    bool IConnector.IsTesting => _isTesting;
    public IDbConnector GetConnector { get; private set; }
    IDbConnector IConnector.GetConnector
    {
        get => GetConnector;
        set => GetConnector = value;
    }
    EnumDatabaseCategory IConnector.Category
    {
        get => _category; set => _category = value;
    }
    string IConnector.ConnectionString
    {
        get => _connectionString;
        set => _connectionString = value;
    }
    public static BasicConnector GetSQLiteTestHelper()
    {
        return new BasicConnector();
    }
    private readonly bool _isTesting;
    private static string GetInMemorySQLiteString()
    {
        return "Data Source=:memory:";
    }
    private BasicConnector()
    {
        _isTesting = true;
        _connectionString = GetInMemorySQLiteString();
        _category = EnumDatabaseCategory.SQLite;
        if (GlobalClass.SQLiteConnector is null)
        {
            throw new CustomBasicException("You never registered the interface for sqlite data connector");
        }
        GetConnector = GlobalClass.SQLiteConnector;
    }
    public BasicConnector(string key, EnumDatabaseCategory category = EnumDatabaseCategory.SQLServer) //sql server is most common here.
    {
        Init(category, key, "");
        GetConnector = this.PrivateConnector();
    }
    private void Init(EnumDatabaseCategory category, string key, string proposedPath)
    {
        if (Configuration is null)
        {
            throw new CustomBasicException("Never registered configuration for the basic connection");
        }
        var configuration = Configuration;
        _category = category; //period no matter what.
        string? possibility = null;
        string? path = null;
        if (category == EnumDatabaseCategory.SQLite)
        {
            //this means you can specify a different path if you want (do in configuration).  only useful for sqlite
            path = configuration.GetValue<string>($"{key}Path");
        }
        else
        {
            possibility = configuration.GetConnectionString(key); //sqlite cannot consider the possibility.
        }
        if (possibility is not null)
        {
            if (_category == EnumDatabaseCategory.SQLite)
            {
                throw new CustomBasicException("Sqlite can never have any possibility");
            }
            _connectionString = possibility;
        }
        else if (category == EnumDatabaseCategory.SQLServer)
        {
            _connectionString = SQLServerConnectionClass!.GetConnectionString(key); //this can never be mysql
        }
        else if (category == EnumDatabaseCategory.SQLite)
        {
            string realPath;
            if (path is null)
            {
                realPath = proposedPath;
            }
            else
            {
                realPath = path;
            }
            if (realPath == "")
            {
                throw new CustomBasicException("Path to sqlite database cannot be blank");
            }
            if (ff1.FileExists(realPath) == false)
            {
                throw new CustomBasicException($"Sqlite database at {realPath} does not exist.  Cannot create automatically because its not generic");
            }
            _connectionString = $@"Data Source = {GetCleanedPath(realPath)}";
        }
        else
        {
            throw new CustomBasicException("Based on database category, unable to get connection string to even connect to a database");
        }
    }
    public BasicConnector(string key, string proposedPath)
    {
        Init(EnumDatabaseCategory.SQLite, key, proposedPath);
        GetConnector = this.PrivateConnector();
    }
    #endregion
    #region Work Functions
    public void DoWork(Action<IDbConnection> action)
    {
        using IDbConnection cons = this.GetConnection();
        cons.Open();
        action.Invoke(cons);
        cons.Close();
    }
    public async Task DoWorkAsync(Func<IDbConnection, Task> action)
    {
        using IDbConnection cons = this.GetConnection();
        cons.Open();
        await action.Invoke(cons);
        cons.Close();
    }
    public async Task DoBulkWorkAsync(Func<IDbConnection, IDbTransaction, Task> action,
        IsolationLevel isolationLevel = IsolationLevel.Unspecified)
    {
        using IDbConnection cons = this.GetConnection();
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
        using IDbConnection cons = this.GetConnection();
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
        using IDbConnection cons = this.GetConnection();
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
        using IDbConnection cons = this.GetConnection();
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
        using IDbConnection cons = this.GetConnection();
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
        using IDbConnection cons = this.GetConnection();
        await cons.UpdateEntityAsync(thisEntity, EnumUpdateCategory.Common);
    }

    public async Task UpdateAllAsync<E>(E thisEntity) where E : class, ISimpleDapperEntity
    {
        using IDbConnection cons = this.GetConnection();
        await cons.UpdateEntityAsync(thisEntity, EnumUpdateCategory.All);
    }
    public void UpdateCommonOnly<E>(E thisEntity) where E : class, ISimpleDapperEntity
    {
        using IDbConnection cons = this.GetConnection();
        cons.UpdateEntity(thisEntity, EnumUpdateCategory.Common);
    }
    public void UpdateAll<E>(E thisEntity) where E : class, ISimpleDapperEntity
    {
        using IDbConnection cons = this.GetConnection();
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
        using IDbConnection cons = this.GetConnection();
        return await cons.InsertSingleAsync(entity, GetConnector);
    }
    public int Insert<E>(E entity) where E : class, ISimpleDapperEntity
    {
        using IDbConnection cons = this.GetConnection();
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
        using IDbConnection cons = this.GetConnection();
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
        using IDbConnection cons = this.GetConnection();
        thisEntity.ID = await cons.InsertSingleAsync(thisEntity, GetConnector);
    }
    public void AddEntity<E>(E thisEntity) where E : class, ISimpleDapperEntity
    {
        using IDbConnection cons = this.GetConnection();
        thisEntity.ID = cons.InsertSingle(thisEntity, GetConnector);
    }
    public void DeleteOnly<E>(E thisEntity) where E : class, ISimpleDapperEntity
    {
        using IDbConnection cons = this.GetConnection();
        cons.Delete(thisEntity);
    }
    public async Task DeleteOnlyAsync<E>(E thisEntity) where E : class, ISimpleDapperEntity
    {
        using IDbConnection cons = this.GetConnection();
        await cons.DeleteAsync(thisEntity);
    }
    public void DeleteOnly<E>(BasicList<ICondition> conditions) where E : class, ISimpleDapperEntity
    {
        using IDbConnection cons = this.GetConnection();
        cons.Delete<E>(conditions, conn: GetConnector);
    }
    public async Task DeleteOnlyAsync<E>(BasicList<ICondition> conditions) where E : class, ISimpleDapperEntity
    {
        using IDbConnection cons = this.GetConnection();
        await cons.DeleteAsync<E>(conditions, conn: GetConnector);
    }
    public void DeleteOnly<E>(int id) where E : class, ISimpleDapperEntity
    {
        using IDbConnection cons = this.GetConnection();
        cons.Delete<E>(id);
    }
    public async Task DeleteOnlyAsync<E>(int id) where E : class, ISimpleDapperEntity
    {
        using IDbConnection cons = this.GetConnection();
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
        using IDbConnection cons = this.GetConnection();
        return cons.GetSingleObject<E, R>(property, sortList, GetConnector, conditions);
    }
    public async Task<R?> GetSingleObjectAsync<E, R>(string property, BasicList<SortInfo> sortList, BasicList<ICondition>? conditions = null) where E : class, ISimpleDapperEntity
    {
        using IDbConnection cons = this.GetConnection();
        return await cons.GetSingleObjectAsync<E, R>(property, sortList, GetConnector, conditions);
    }
    public BasicList<R> GetObjectList<E, R>(string property, BasicList<ICondition>? conditions = null, BasicList<SortInfo>? sortList = null, int howMany = 0) where E : class
    {
        using IDbConnection cons = this.GetConnection();
        return cons.GetObjectList<E, R>(property, GetConnector, conditions, sortList, howMany);
    }
    public async Task<BasicList<R>> GetObjectListAsync<E, R>(string property, BasicList<ICondition>? conditions = null, BasicList<SortInfo>? sortList = null, int howMany = 0) where E : class
    {
        using IDbConnection cons = this.GetConnection();
        return await cons.GetObjectListAsync<E, R>(property, GetConnector, conditions, sortList, howMany);
    }
    public E Get<E>(int id) where E : class
    {
        using IDbConnection cons = this.GetConnection();
        return cons.Get<E>(id, GetConnector);
    }
    public IEnumerable<E> Get<E>(BasicList<SortInfo>? sortList = null, int howMany = 0) where E : class
    {
        using IDbConnection cons = this.GetConnection();
        return cons.Get<E>(GetConnector, sortList, howMany);
    }
    public async Task<E> GetAsync<E>(int id) where E : class
    {
        using IDbConnection cons = this.GetConnection();
        return await cons.GetAsync<E>(id, GetConnector);
    }
    public async Task<IEnumerable<E>> GetAsync<E>(BasicList<SortInfo>? sortList = null, int howMany = 0) where E : class
    {
        using IDbConnection cons = this.GetConnection();
        return await cons.GetAsync<E>(GetConnector, sortList, howMany);
    }
    public E Get<E, D1>(int id) where E : class, IJoinedEntity where D1 : class
    {
        using IDbConnection cons = this.GetConnection();
        return cons.Get<E, D1>(id, GetConnector);
    }
    public IEnumerable<E> Get<E, D1>(BasicList<SortInfo>? sortList = null, int howMany = 0) where E : class, IJoinedEntity where D1 : class
    {
        using IDbConnection cons = this.GetConnection();
        return cons.Get<E, D1>(sortList, GetConnector, howMany);
    }
    public async Task<E> GetAsync<E, D1>(int id) where E : class, IJoinedEntity where D1 : class
    {
        using IDbConnection cons = this.GetConnection();
        return await cons.GetAsync<E, D1>(id, GetConnector);
    }
    public async Task<IEnumerable<E>> GetAsync<E, D1>(BasicList<SortInfo>? sortList, int howMany = 0) where E : class, IJoinedEntity where D1 : class
    {
        using IDbConnection cons = this.GetConnection();
        return await cons.GetAsync<E, D1>(sortList, GetConnector, howMany);
    }
    public E Get<E, D1, D2>(int id) where E : class, IJoin3Entity<D1, D2> where D1 : class where D2 : class
    {
        using IDbConnection cons = this.GetConnection();
        return cons.Get<E, D1, D2>(id, GetConnector);
    }
    public IEnumerable<E> Get<E, D1, D2>(BasicList<SortInfo> sortList, int howMany = 0) where E : class, IJoin3Entity<D1, D2> where D1 : class where D2 : class
    {
        using IDbConnection cons = this.GetConnection();
        return cons.Get<E, D1, D2>(sortList, GetConnector, howMany);
    }
    public async Task<E> GetAsync<E, D1, D2>(int id) where E : class, IJoin3Entity<D1, D2> where D1 : class where D2 : class
    {
        using IDbConnection cons = this.GetConnection();
        return await cons.GetAsync<E, D1, D2>(id, GetConnector);
    }
    public async Task<IEnumerable<E>> GetAsync<E, D1, D2>(BasicList<SortInfo> sortList, int howMany = 0) where E : class, IJoin3Entity<D1, D2> where D1 : class where D2 : class
    {
        using IDbConnection cons = this.GetConnection();
        return await cons.GetAsync<E, D1, D2>(sortList, GetConnector, howMany);
    }
    public E GetOneToMany<E, D1>(int id) where E : class, IJoinedEntity where D1 : class
    {
        using IDbConnection cons = this.GetConnection();
        return cons.GetOneToMany<E, D1>(id, GetConnector);
    }
    public IEnumerable<E> GetOneToMany<E, D1>(BasicList<SortInfo>? sortList = null) where E : class, IJoinedEntity where D1 : class
    {
        using IDbConnection cons = this.GetConnection();
        return cons.GetOneToMany<E, D1>(GetConnector, sortList);
    }
    public async Task<E> GetOneToManyAsync<E, D1>(int id) where E : class, IJoinedEntity where D1 : class
    {
        using IDbConnection cons = this.GetConnection();
        return await cons.GetOneToManyAsync<E, D1>(id, GetConnector);
    }
    public async Task<IEnumerable<E>> GetOneToManyAsync<E, D1>(BasicList<SortInfo>? sortList = null) where E : class, IJoinedEntity where D1 : class
    {
        using IDbConnection cons = this.GetConnection();
        return await cons.GetOneToManyAsync<E, D1>(GetConnector, sortList);
    }
    public BasicList<E> Get<E>(BasicList<ICondition> conditions, BasicList<SortInfo>? sortList = null, int howMany = 0) where E : class
    {
        using IDbConnection cons = this.GetConnection();
        return cons.Get<E>(conditions, GetConnector, sortList, howMany);
    }
    public async Task<BasicList<E>> GetAsync<E>(BasicList<ICondition> conditions, BasicList<SortInfo>? sortList = null, int howMany = 0) where E : class
    {
        using IDbConnection cons = this.GetConnection();
        return await cons.GetAsync<E>(conditions, GetConnector, sortList, howMany);
    }
    public BasicList<E> GetOneToMany<E, D1>(BasicList<ICondition> conditionList, BasicList<SortInfo>? sortList = null) where E : class, IJoinedEntity where D1 : class
    {
        using IDbConnection cons = this.GetConnection();
        return cons.GetOneToMany<E, D1>(conditionList, GetConnector, sortList);
    }
    public BasicList<E> GetOneToMany<E, D1, D2>(BasicList<ICondition> conditionList, BasicList<SortInfo>? sortList = null) where E : class, IJoin3Entity<D1, D2> where D1 : class where D2 : class
    {
        using IDbConnection cons = this.GetConnection();
        return cons.GetOneToMany<E, D1, D2>(conditionList, GetConnector, sortList);
    }
    public async Task<BasicList<E>> GetOneToManyAsync<E, D1>(BasicList<ICondition> conditionList, BasicList<SortInfo>? sortList = null) where E : class, IJoinedEntity where D1 : class
    {
        using IDbConnection cons = this.GetConnection();
        return await cons.GetOneToManyAsync<E, D1>(conditionList, GetConnector, sortList);
    }
    public async Task<BasicList<E>> GetOneToManyAsync<E, D1, D2>(BasicList<ICondition> conditionList, BasicList<SortInfo>? sortList = null) where E : class, IJoin3Entity<D1, D2> where D1 : class where D2 : class
    {
        using IDbConnection cons = this.GetConnection();
        return await GetOneToManyAsync<E, D1, D2>(conditionList, sortList);
    }
    public IEnumerable<E> Get<E, D1>(BasicList<ICondition> conditionList, BasicList<SortInfo>? sortList = null, int howMany = 0) where E : class, IJoinedEntity where D1 : class
    {
        using IDbConnection cons = this.GetConnection();
        return cons.Get<E, D1>(conditionList, GetConnector, sortList, howMany);
    }
    public async Task<IEnumerable<E>> GetAsync<E, D1>(BasicList<ICondition> conditionList, BasicList<SortInfo>? sortList = null, int howMany = 0) where E : class, IJoinedEntity where D1 : class
    {
        using IDbConnection cons = this.GetConnection();
        return await cons.GetAsync<E, D1>(conditionList, GetConnector, sortList, howMany);
    }
    public IEnumerable<E> Get<E, D1, D2>(BasicList<ICondition> conditionList, BasicList<SortInfo>? sortList = null, int howMany = 0) where E : class, IJoin3Entity<D1, D2> where D1 : class where D2 : class
    {
        using IDbConnection cons = this.GetConnection();
        return cons.Get<E, D1, D2>(conditionList, GetConnector, sortList, howMany);
    }
    public async Task<IEnumerable<E>> GetAsync<E, D1, D2>(BasicList<ICondition> conditionList, BasicList<SortInfo>? sortList = null, int howMany = 0) where E : class, IJoin3Entity<D1, D2> where D1 : class where D2 : class
    {
        using IDbConnection cons = this.GetConnection();
        return await cons.GetAsync<E, D1, D2>(conditionList, GetConnector, sortList, howMany);
    }
    /// <summary>
    /// this is when you just want to get all the data from the database.  most simple but no parameters this time.
    /// </summary>
    /// <typeparam name="E"></typeparam>
    /// <returns></returns>
    public async Task<BasicList<E>> GetDataListAsync<E>() where E : class, ISimpleDapperEntity //this should still work.
    {
        BasicList<E> output = [];
        await DoWorkAsync(async cons =>
        {
            var results = await cons.GetAsync<E>(GetConnector);
            output = results.ToBasicList();
        });
        return output;
    }
    public BasicList<T> LoadData<T, U>(string sqlStatement, U parameters)
    {
        using IDbConnection cons = this.GetConnection();
        BasicList<T> rows = cons.Query<T>(sqlStatement, parameters).ToBasicList();
        return rows;
    }
    public async Task<BasicList<T>> LoadDataAsync<T, U>(string sqlStatement, U parameters)
    {
        using IDbConnection cons = this.GetConnection();
        var rows = await cons.QueryAsync<T>(sqlStatement, parameters);
        return rows.ToBasicList();
    }
    public void SaveData<T>(string sqlStatement, T parameters)
    {
        using IDbConnection cons = this.GetConnection();
        cons.Execute(sqlStatement, parameters);
    }
    public async Task SaveDataAsync<T>(string sqlStatement, T parameters)
    {
        using IDbConnection cons = this.GetConnection();
        await cons.ExecuteAsync(sqlStatement, parameters);
    }
    public BasicList<T> LoadData<T, U>(string sqlStatement, U parameters, bool isStoredProcedure)
    {
        using IDbConnection cons = this.GetConnection();
        CommandType commandType = CommandType.Text;
        if (isStoredProcedure == true)
        {
            commandType = CommandType.StoredProcedure;
        }
        BasicList<T> rows = cons.Query<T>(sqlStatement, parameters, commandType: commandType).ToBasicList();
        return rows;
    }
    public async Task<BasicList<T>> LoadDataAsync<T, U>(string sqlStatement, U parameters, bool isStoredProcedure)
    {
        using IDbConnection cons = this.GetConnection();
        CommandType commandType = CommandType.Text;
        if (isStoredProcedure == true)
        {
            commandType = CommandType.StoredProcedure;
        }
        var rows = await cons.QueryAsync<T>(sqlStatement, parameters, commandType: commandType);
        return rows.ToBasicList();

    }
    //sql server portion
    public void SaveData<T>(string sqlStatement, T parameters, bool isStoredProcedure)
    {
        using IDbConnection cons = this.GetConnection();
        CommandType commandType = CommandType.Text;
        if (isStoredProcedure == true)
        {
            commandType = CommandType.StoredProcedure;
        }
        cons.Execute(sqlStatement, parameters, commandType: commandType);
    }
    public async Task SaveDataAsync<T>(string sqlStatement, T parameters, bool isStoredProcedure)
    {
        using IDbConnection cons = this.GetConnection();
        CommandType commandType = CommandType.Text;
        if (isStoredProcedure == true)
        {
            commandType = CommandType.StoredProcedure;
        }
        await cons.ExecuteAsync(sqlStatement, parameters, commandType: commandType);
    }
    #endregion
}