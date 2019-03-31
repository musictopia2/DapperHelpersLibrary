using System;
using System.Text;
using CommonBasicStandardLibraries.Exceptions;
using CommonBasicStandardLibraries.AdvancedGeneralFunctionsAndProcesses.BasicExtensions;
using System.Linq;
using CommonBasicStandardLibraries.BasicDataSettingsAndProcesses;
using static CommonBasicStandardLibraries.BasicDataSettingsAndProcesses.BasicDataFunctions;
using CommonBasicStandardLibraries.CollectionClasses;
using System.Threading.Tasks; //most of the time, i will be using asyncs.
using fs = CommonBasicStandardLibraries.AdvancedGeneralFunctionsAndProcesses.JsonSerializers.FileHelpers;
using js = CommonBasicStandardLibraries.AdvancedGeneralFunctionsAndProcesses.JsonSerializers.NewtonJsonStrings; //just in case i need those 2.
using DapperHelpersLibrary.Extensions;
using System.Reflection;
using DapperHelpersLibrary.ConditionClasses;
using DapperHelpersLibrary.Attributes;
//i think this is the most common things i like to do
namespace DapperHelpersLibrary.MapHelpers
{
    internal static class MapBaseHelperClass
    {
        public static CustomBasicList<DatabaseMapping> GetMappingList<T>(out string Tablename, bool BeingJoined = false) where T : class //bug here.
        {
            Type ThisType = typeof(T);
            if (ThisType.IsInterface == true)
                throw new BasicBlankException("Interfaces are not supported for getting mappings.  Otherwise, can't read attributes.  Try using generics");
            Tablename = ThisType.GetTableName();
            //CustomBasicList<PropertyInfo> FirstList = ThisType.GetProperties().Where(Items => Items.CanMapToDatabase() == true).ToCustomBasicList();
            var TempList = ThisType.GetProperties().Where(Items => Items.CanMapToDatabase() == true);
            if (BeingJoined == true)
                TempList = TempList.Where(Items => Items.HasAttribute<PrimaryJoinedDataAttribute>() == true || Items.Name == "ID");
            CustomBasicList<PropertyInfo> FirstList = TempList.ToCustomBasicList();
            string TempTable = Tablename;
            CustomBasicList<DatabaseMapping> output = new CustomBasicList<DatabaseMapping>(); //we do want id.  there are many times when we need it.
            FirstList.ForEach(Items => GetIndividualMapping(Items, ref output, TempTable));
            return output;
        }
        private static void GetIndividualMapping(PropertyInfo property, ref CustomBasicList<DatabaseMapping> output, string TableName, object ThisObj = null)
        {
            string OurProperty;
            string Database;
            bool IncludeUpdate;
            OurProperty = property.Name;
            Database = property.GetColumnName();
            if (property.Name == "ID")
                IncludeUpdate = false;
            else if (property.HasAttribute<ForeignKeyAttribute>() == true)
                IncludeUpdate = false;
            else if (property.HasAttribute<ExcludeUpdateListenerAttribute>() == true)
                IncludeUpdate = false;
            else
                IncludeUpdate = true;
            string InterfaceName = property.GetInterfaceName();
            
            if (ThisObj == null)
                output.Add(new DatabaseMapping(OurProperty, Database, TableName, property) { InterfaceName = InterfaceName, IsBoolProperty = property.IsBool(), CommonForUpdating = IncludeUpdate });
            else
            {
                output.Add(new DatabaseMapping(OurProperty, Database, TableName, property)
                {
                    //Value = property.GetValueForDatabase(ThisObj),
                    Value = property.GetValue(ThisObj, null),
                    InterfaceName = InterfaceName,
                    IsBoolProperty = property.IsBool(),
                    CommonForUpdating = IncludeUpdate
                });
            }
        }
        public static CustomBasicList<DatabaseMapping> GetMappingList<T>(T ThisObj, out string Tablename, bool IsAutoIncremented = true, bool BeingJoined = false) where T : class
        {
            Type ThisType = ThisObj.GetType();
            Tablename = ThisType.GetTableName();

            //CustomBasicList<PropertyInfo> FirstList = ThisType.GetProperties().Where(Items => Items.Name != "ID" && Items.CanMapToDatabase() == true).ToCustomBasicList();
            var TempList = ThisType.GetProperties().Where(Items => Items.CanMapToDatabase() == true);
            if (IsAutoIncremented == true)
                TempList = TempList.Where(Items => Items.Name != "ID");
            if (BeingJoined == true)
                TempList = TempList.Where(Items => Items.HasAttribute<PrimaryJoinedDataAttribute>() == true);
            CustomBasicList<PropertyInfo> FirstList = TempList.ToCustomBasicList();
            CustomBasicList<DatabaseMapping> output = new CustomBasicList<DatabaseMapping>();
            string TempTable = Tablename;
            FirstList.ForEach(Items => GetIndividualMapping(Items, ref output, TempTable, ThisObj));
            
            return output;
        }

        public static DatabaseMapping FindMappingForProperty(IProperty ThisProperty, CustomBasicList<DatabaseMapping> OriginalMappings)
        {
            try
            {
                return OriginalMappings.Where(Items => Items.DatabaseName == ThisProperty.Property || Items.ObjectName == ThisProperty.Property || Items.InterfaceName == ThisProperty.Property).First().Clone(); //just the first one should be fine.
            }
            catch(Exception ex)
            {
                throw new BasicBlankException($"Had problems getting mappings for conditions.  Condition Property Name Was {ThisProperty.Property}.  Message Was {ex.Message}");
            }
        }

    }
}
