using CommonBasicLibraries.AdvancedGeneralFunctionsAndProcesses.BasicExtensions;
using CommonBasicLibraries.BasicDataSettingsAndProcesses;
using CommonBasicLibraries.CollectionClasses;
using CommonBasicLibraries.DatabaseHelpers.Attributes;
using CommonBasicLibraries.DatabaseHelpers.Extensions;
using CommonBasicLibraries.DatabaseHelpers.MiscInterfaces;
using System;
using System.Linq;
using System.Reflection;
namespace DapperHelpersLibrary.MapHelpers
{
    internal static class MapBaseHelperClass
    {
        public static BasicList<DatabaseMapping> GetMappingList<T>(out string tablename, bool beingJoined = false) where T : class //bug here.
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
            BasicList<DatabaseMapping> output = new (); //we do want id.  there are many times when we need it.
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
            var tempList = thisType.GetProperties().Where(xx => xx.CanMapToDatabase() == true);
            if (isAutoIncremented == true)
            {
                tempList = tempList.Where(xx => xx.Name != "ID");
            }
            if (beingJoined == true)
            {
                tempList = tempList.Where(xx => xx.HasAttribute<PrimaryJoinedDataAttribute>() == true);
            }
            BasicList<PropertyInfo> firstList = tempList.ToBasicList();
            BasicList<DatabaseMapping> output = new ();
            string tempTable = tablename;
            firstList.ForEach(xx => GetIndividualMapping(xx, ref output, tempTable, thisObj));
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
}