namespace DapperHelpersLibrary.ConnectionHelpers;
internal static class ConnectionExtensions
{
    #region Main Functions
    public static IDbConnector PrivateConnector(this IConnector db)
    {
        //the warnings is okay for now.
        if (db.Category == EnumDatabaseCategory.SQLServer)
        {
            if (GlobalClass.SQLServerConnector is null)
            {
                throw new CustomBasicException("You never registered the interface for sql server data connector");
            }
            return GlobalClass.SQLServerConnector;
        }
        if (db.Category == EnumDatabaseCategory.SQLite)
        {
            if (GlobalClass.SQLiteConnector is null)
            {
                throw new CustomBasicException("You never registered the interface for sqlite data connector");
            }
            return GlobalClass.SQLiteConnector;
        }
        if (db.Category == EnumDatabaseCategory.MySQL)
        {
            if (GlobalClass.MySQLConnector is null)
            {
                throw new CustomBasicException("You never registered the interface for mysql data connector");
            }
            return GlobalClass.MySQLConnector;
        }
        throw new CustomBasicException("The data connector currently is unknown.  May require rethinking");
    }
    public static IDbConnection GetConnection(this IConnector db)
    {
        if (db.Category == EnumDatabaseCategory.SQLite)
        {
            IDbConnection output = db.GetConnector.GetConnection(EnumDatabaseCategory.SQLite, db.ConnectionString);
            if (db.IsTesting == true)
            {
                output.Open();
            }
            output.Dispose();
            return db.GetConnector.GetConnection(EnumDatabaseCategory.SQLite, db.ConnectionString);
        }
        else if (db.Category == EnumDatabaseCategory.SQLServer)
        {
            if (db.IsTesting == true)
            {
                throw new CustomBasicException("You can't be testing on a sql server database");
            }
            return db.GetConnector.GetConnection(EnumDatabaseCategory.SQLServer, db.ConnectionString);
        }
        else if (db.Category == EnumDatabaseCategory.MySQL)
        {
            if (db.IsTesting == true)
            {
                throw new CustomBasicException("You can't be testing on mySQL database");
            }
            return db.GetConnector.GetConnection(EnumDatabaseCategory.MySQL, db.ConnectionString);
        }
        else
        {
            throw new CustomBasicException("Only SQL Server, SQLite, and MySQL Databases Are Currently Supported");
        }
    }

    #endregion

}