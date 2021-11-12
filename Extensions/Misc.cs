namespace DapperHelpersLibrary.Extensions;
public static class Misc
{
    internal static EnumDatabaseCategory GetDatabaseCategory(this IDbConnection db, IDbConnector conn)
    {
        return conn.GetCategory(db);
    }
    public static void CreateTableSQLite<E>(this IDbConnection db, IDbConnector conn) where E : class
    {
        EnumDatabaseCategory dcat = conn.GetCategory(db);
        if (dcat != EnumDatabaseCategory.SQLite)
        {
            throw new CustomBasicException("Currently, Only SQLite can create tables since the variable types will be all strings");
        }
        var thisList = GetMappingList<E>(out string TableName);
        if (thisList.Exists(xx => xx.DatabaseName.ToUpper() == "ID") == false)
        {
            throw new CustomBasicException("You must have ID in order to create table  Its needed for the primary key part");
        }
        string sqls;
        StrCat cats = new();
        StringBuilder thisStr = new("create table ");
        thisStr.Append(TableName);
        bool autoIncrementID = IsAutoIncremented<E>();
        if (autoIncrementID == true)
        {
            thisStr.Append(" (ID integer primary key autoincrement, ");
        }
        else
        {
            thisStr.Append(" (ID integer primary key, ");
        }
        thisList.RemoveAllOnly(xx => xx.DatabaseName.ToLower() == "id");
        thisList.ForEach(xx => cats.AddToString($"{xx.DatabaseName} {xx.GetDataType()}", ", "));
        thisStr.Append(cats.GetInfo());
        thisStr.Append(')');
        sqls = thisStr.ToString();
        db.Execute(sqls);
    }
}