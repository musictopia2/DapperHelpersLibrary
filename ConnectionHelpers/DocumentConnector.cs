namespace DapperHelpersLibrary.ConnectionHelpers;
public class DocumentConnector : IConnector
{
    private EnumDatabaseCategory _category;
    public IDbConnector GetConnector { get; private set; }
    bool IConnector.IsTesting { get; }
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
    private string _connectionString = "";
    public DocumentConnector(string databaseName, string collectionName, string proposedPath)
    {
        if (Configuration is null)
        {
            throw new CustomBasicException("No configuration was registered for document connector");
        }
        var config = Configuration;
        string key = $"DocumentDatabaseSQLServer-{databaseName}-{collectionName}";
        string? possibility = config.GetConnectionString(key);
        key = $"DocumentDatabaseSqlite-{databaseName}-{collectionName}";
        string? path = config.GetValue<string>(key);
        _category = EnumDatabaseCategory.SQLServer;
        key = $"DocumentDatabaseMySQL-{databaseName}-{collectionName}";
        string? possibleMySQL = config.GetValue<string>(key);
        if (possibility is null && path is null && proposedPath == "" && SQLServerConnectionClass is null && string.IsNullOrWhiteSpace(possibleMySQL))
        {
            throw new CustomBasicException("You never registered the interface for the connection helpers and did not provide a connection string via IConfiguration");
        }
        if (possibility is not null && path is null)
        {
            _connectionString = possibility;
        }
        else if (path is null && string.IsNullOrWhiteSpace(proposedPath))
        {
            _connectionString = SQLServerConnectionClass!.GetConnectionString("DocumentDatabase");
        }
        else if (string.IsNullOrWhiteSpace(possibleMySQL))
        {
            _category = EnumDatabaseCategory.SQLite;
            string toUse;
            if (path is not null)
            {
                toUse = path;
            }    
            else
            {
                toUse = proposedPath;
            }
            if (toUse == "")
            {
                throw new CustomBasicException("The toUse cannot be empty");
            }
            if (GlobalClass.CreateSqliteDatabase is null)
            {
                throw new CustomBasicException("Nobody is creating the sqlite database");
            }
            if (ff1.FileExists(toUse) == false)
            {
                GlobalClass.CreateSqliteDatabase(toUse);
            }
            //this means will be sqlite.
            _connectionString = $@"Data Source = {GetCleanedPath(toUse)}";
        }
        if (_category == EnumDatabaseCategory.None)
        {
            if (string.IsNullOrWhiteSpace(possibleMySQL))
            {
                _category = EnumDatabaseCategory.SQLServer;
            }
            else
            {
                _category = EnumDatabaseCategory.MySQL;
            }
        }
        GetConnector = this.PrivateConnector();
    }
    public IDbConnection GetConnection()
    {
        return ConnectionExtensions.GetConnection(this);
    } 
    //cannot do transactions when its document db.
    public async Task DoWorkAsync(Func<IDbConnection, Task> action)
    {
        using IDbConnection cons = GetConnection();
        cons.Open();
        await action.Invoke(cons);
        cons.Close();
    }
}