namespace DapperHelpersLibrary.SQLHelpers;
internal class SimpleStatementHelpers
{
    public enum EnumSQLCategory
    {
        Normal,
        Count,
        Bool,
        Delete
    }
    public static string GetInsertStatement(EnumDatabaseCategory category, BasicList<DatabaseMapping> thisList, string tableName, bool isAutoIncremented)
    {
        if (category == EnumDatabaseCategory.None)
        {
            throw new CustomBasicException("Must choose what database to use");
        }
        StringBuilder thisStr = new("insert into ");
        thisStr.Append(tableName);
        thisStr.Append(" (");
        StrCat cat1 = new();
        StrCat cat2 = new();
        thisList.ForEach(Items =>
        {
            cat1.AddToString(Items.DatabaseName, ", ");
            cat2.AddToString($"@{Items.DatabaseName}", ", ");
        });
        thisStr.Append(cat1.GetInfo());
        thisStr.Append(") values (");
        thisStr.Append(cat2.GetInfo());
        thisStr.Append(')');
        if (isAutoIncremented == true)
        {
            if (category == EnumDatabaseCategory.SQLite)
            {
                thisStr.Append("; SELECT last_insert_rowid()");
            }
            else if (category == EnumDatabaseCategory.SQLServer)
            {
                thisStr.Append("; SELECT CAST(SCOPE_IDENTITY()  AS BIGINT) AS [id]");
            }
            else if (category == EnumDatabaseCategory.MySQL)
            {
                thisStr.Append("; SELECT LAST_INSERT_ID();");
            }
            else
            {
                throw new CustomBasicException("Not Supported");
            }
        }
        return thisStr.ToString();
    }
    public static string GetLimitSQLite(EnumDatabaseCategory database, int howMany)
    {
        if (database == EnumDatabaseCategory.SQLServer)
        {
            return "";
        }
        if (howMany <= 0)
        {
            return "";
        }
        return $"Limit {howMany}";
    }
    public static string GetSortStatement(BasicList<DatabaseMapping> mapList, BasicList<SortInfo>? sortList, bool isJoined)
    {
        if (sortList == null)
        {
            return "";
        }
        if (sortList.Count == 0)
        {
            throw new CustomBasicException("If you are not sending nothing. you must have at least one condition");
        }
        StringBuilder thisStr = new();
        thisStr.Append(" order by ");
        string extras;
        StrCat cats = new();
        sortList.ForEach(items =>
        {
            DatabaseMapping thisMap = FindMappingForProperty(items, mapList);
            if (items.OrderBy == SortInfo.EnumOrderBy.Descending)
            {
                extras = " desc";
            }
            else
            {
                extras = "";
            }
            if (isJoined == false)
            {
                cats.AddToString($"{thisMap.DatabaseName}{extras}", ", ");
            }
            else
            {
                cats.AddToString($"{thisMap.Prefix}.{thisMap.DatabaseName}{extras}", ", ");
            }
        });
        thisStr.Append(cats.GetInfo());
        return thisStr.ToString();
    }
    public static string GetDeleteStatement(string tableName)
    {
        StringBuilder thisStr = new("delete from ");
        thisStr.Append(tableName);
        return thisStr.ToString();
    }
    public static (string sqls, BasicList<DatabaseMapping> MapList) GetSimpleSelectStatement<E>(EnumDatabaseCategory database, int howMany = 0) where E : class
    {
        BasicList<DatabaseMapping> thisList = GetMappingList<E>(out string TableName);
        string thisStr = GetSimpleSelectStatement(thisList, TableName, database, howMany: howMany);
        return (thisStr, thisList);
    }
    public static string GetSimpleSelectStatement(BasicList<DatabaseMapping> thisList, string tableName, EnumDatabaseCategory database, EnumSQLCategory category = EnumSQLCategory.Normal, int howMany = 0, string property = "")
    {
        StringBuilder thisStr = new("select ");
        if (howMany > 0 && database == EnumDatabaseCategory.SQLServer)
        {
            thisStr.Append($"top {howMany} ");
        }
        if (category == EnumSQLCategory.Normal && property == "")
        {
            if (thisList.TrueForAll(Items => Items.HasMatch == true))
            {
                thisStr.Append(" * from ");
                thisStr.Append(tableName);
                return thisStr.ToString();
            }
            StrCat cats = new();
            thisList.ForEach(xx =>
            {
                if (xx.HasMatch == false)
                {
                    cats.AddToString($"{xx.DatabaseName} as {xx.ObjectName}", ", ");
                }
                else
                {
                    cats.AddToString(xx.DatabaseName, ", ");
                }
            });
            thisStr.Append(cats.GetInfo());
        }
        else if (category == EnumSQLCategory.Normal)
        {
            DatabaseMapping thisMap = thisList.Where(xx => xx.ObjectName == property).Single();
            if (thisMap.HasMatch == false)
            {
                thisStr.Append($"{thisMap.DatabaseName} as {thisMap.ObjectName} ");
            }
            else
            {
                thisStr.Append($"{thisMap.DatabaseName} ");
            }
        }
        else if (category == EnumSQLCategory.Count)
        {
            thisStr.Append("count (*)");
        }
        else if (category == EnumSQLCategory.Bool)
        {
            thisStr.Append('1');
        }
        else if (category == EnumSQLCategory.Delete)
        {
            throw new CustomBasicException("Deleting is not supposed to get a select statement.  Try delete statement instead");
        }
        else
        {
            throw new CustomBasicException("Not supported");
        }
        thisStr.Append(" from ");
        thisStr.Append(tableName);
        return thisStr.ToString();
    }
}