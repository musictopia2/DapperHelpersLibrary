using System.Reflection;
namespace DapperHelpersLibrary.MapHelpers;
internal static class MapBaseHelperClass
{
    public static BasicList<DatabaseMapping> GetMappingList<T>(out string tablename, bool beingJoined = false) where T : class 
    {
        Type thisType = typeof(T);
        if (thisType.IsInterface == true)
        {
            throw new CustomBasicException("Interfaces are not supported for getting mappings.  Otherwise, can't read attributes.  Try using generics");
        }
        tablename = thisType.GetTableName();
        var tempList = thisType.GetProperties().Where(xx => xx.CanMapToDatabase() == true);
        if (beingJoined == true)
        {
            tempList = tempList.Where(xx => xx.HasAttribute<PrimaryJoinedDataAttribute>() == true || xx.Name == "ID");
        }
        BasicList<PropertyInfo> firstList = tempList.ToBasicList();
        string tempTable = tablename;
        BasicList<DatabaseMapping> output = new();
        firstList.ForEach(xx => GetIndividualMapping(xx, ref output, tempTable));
        return output;
    }
    private static void GetIndividualMapping(PropertyInfo property, ref BasicList<DatabaseMapping> output, string tableName, object? thisObj = null)
    {
        string ourProperty;
        string database;
        bool includeUpdate;
        ourProperty = property.Name;
        database = property.GetColumnName();
        if (property.Name == "ID")
        {
            includeUpdate = false;
        }
        else if (property.HasAttribute<ForeignKeyAttribute>() == true)
        {
            includeUpdate = false;
        }
        else if (property.HasAttribute<ExcludeUpdateListenerAttribute>() == true)
        {
            includeUpdate = false;
        }
        else
        {
            includeUpdate = true;
        }
        string interfaceName = property.GetInterfaceName();
        if (thisObj == null)
        {
            output.Add(new DatabaseMapping(ourProperty, database, tableName, property) { InterfaceName = interfaceName, IsBoolProperty = property.IsBool(), CommonForUpdating = includeUpdate });
        }
        else
        {
            output.Add(new DatabaseMapping(ourProperty, database, tableName, property)
            {
                Value = property.GetValue(thisObj, null),
                InterfaceName = interfaceName,
                IsBoolProperty = property.IsBool(),
                CommonForUpdating = includeUpdate
            });
        }
    }
    public static BasicList<DatabaseMapping> GetMappingList<T>(T thisObj, out string tablename, bool isAutoIncremented = true, bool beingJoined = false) where T : class
    {
        Type thisType = thisObj.GetType();
        tablename = thisType.GetTableName();
        var tempList = thisType.GetProperties().Where(xx => xx.CanMapToDatabase() == true).ToBasicList();
        if (isAutoIncremented == true)
        {
            var firsts = tempList.Where(x => x.Name == "ID").SingleOrDefault();
            if (firsts is not null)
            {
                tempList.RemoveSpecificItem(firsts);
            }
            else
            {
                firsts = tempList.First();
                if (firsts.Name != "ID")
                {
                    ColumnAttribute? column = firsts.GetAttribute<ColumnAttribute>();
                    if (column is null)
                    {
                        throw new CustomBasicException("The column attribute was not found first.  Rethinking is necessary now");
                    }
                    if (column.ColumnName != "ID")
                    {
                        throw new CustomBasicException("The first column was supposed to have the name of ID.  Rethink");
                    }
                }
                tempList.RemoveFirstItem();
            }
        }
        if (beingJoined == true)
        {
            tempList.KeepConditionalItems(xx => xx.HasAttribute<PrimaryJoinedDataAttribute>());
        }
        BasicList<DatabaseMapping> output = new();
        string tempTable = tablename;
        tempList.ForEach(xx => GetIndividualMapping(xx, ref output, tempTable, thisObj));
        return output;
    }
    public static DatabaseMapping FindMappingForProperty(IProperty thisProperty, BasicList<DatabaseMapping> originalMappings)
    {
        try
        {
            return originalMappings.Where(Items => Items.DatabaseName == thisProperty.Property || Items.ObjectName == thisProperty.Property || Items.InterfaceName == thisProperty.Property).First().Clone(); //just the first one should be fine.
        }
        catch (Exception ex)
        {
            throw new CustomBasicException($"Had problems getting mappings for conditions.  Condition Property Name Was {thisProperty.Property}.  Message Was {ex.Message}");
        }
    }
}