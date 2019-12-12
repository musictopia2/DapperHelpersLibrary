using CommonBasicStandardLibraries.AdvancedGeneralFunctionsAndProcesses.BasicExtensions;
using CommonBasicStandardLibraries.AdvancedGeneralFunctionsAndProcesses.Misc;
using CommonBasicStandardLibraries.CollectionClasses;
using CommonBasicStandardLibraries.DatabaseHelpers.EntityInterfaces;
using CommonBasicStandardLibraries.DatabaseHelpers.MiscClasses;
using CommonBasicStandardLibraries.Exceptions;
using DapperHelpersLibrary.Extensions;
using DapperHelpersLibrary.MapHelpers;
using System.Linq;
using System.Text;
using static DapperHelpersLibrary.MapHelpers.MapBaseHelperClass;
namespace DapperHelpersLibrary.SQLHelpers
{
    public static class StatementFactoryUpdates
    {
        public enum EnumUpdateCategory
        {
            Auto,
            All,
            Common //could decide to only update the common ones.
        }
        internal static (string sqls, CustomBasicList<DatabaseMapping> ParameterMappings) GetUpdateStatement<E>(E thisEntity, CustomBasicList<UpdateFieldInfo> manuelList) where E : class
        {
            if (manuelList.Count == 0)
                throw new BasicBlankException("If you are manually updating, you havve to send at least one field to update");
            CustomBasicList<DatabaseMapping> updateList = GetParameterMappings<E>(thisEntity, manuelList);
            return GetUpdateStatement(updateList);
        }

        internal static (string sqls, CustomBasicList<DatabaseMapping> ParameterMappings) GetUpdateStatement<E>(CustomBasicList<UpdateEntity> manuelList) where E : class
        {
            if (manuelList.Count == 0)
                throw new BasicBlankException("If you are manually updating, you havve to send at least one field to update");
            CustomBasicList<DatabaseMapping> updateList = GetParameterMappings<E>(manuelList);
            return GetUpdateStatement(updateList);
        }

        internal static (string sqls, CustomBasicList<DatabaseMapping> ParameterMappings) GetUpdateStatement<E>(E thisEntity) where E : class, IUpdatableEntity
        {
            CustomBasicList<DatabaseMapping> updateList = GetParameterMappings(thisEntity);
            return GetUpdateStatement(updateList);
        }

        internal static (string sqls, CustomBasicList<DatabaseMapping> ParameterMappings) GetUpdateStatement<E>(E thisEntity, EnumUpdateCategory category) where E : class
        {
            CustomBasicList<DatabaseMapping> updateList = GetParameterMappings(thisEntity, category);
            return GetUpdateStatement(updateList);
        }
        private static (string sqls, CustomBasicList<DatabaseMapping> ParameterMappings) GetUpdateStatement(CustomBasicList<DatabaseMapping> updateList)
        {
            if (updateList.Count == 0)
                return ("", new CustomBasicList<DatabaseMapping>());
            StringBuilder thisStr = new StringBuilder();
            string tableName = updateList.First().TableName; //found a use for this.
            thisStr.Append("update ");
            thisStr.Append(tableName);
            thisStr.Append(" set ");
            StrCat cats = new StrCat();
            updateList.ForEach(Items =>
            {
                cats.AddToString($"{Items.DatabaseName} = @{Items.DatabaseName}", ", ");
            });
            thisStr.Append(cats.GetInfo());
            thisStr.Append(" where ID = @ID");
            return (thisStr.ToString(), updateList);
        }
        private static CustomBasicList<DatabaseMapping> GetParameterMappings<E>(CustomBasicList<UpdateEntity> updateList) where E : class
        {
            if (updateList.Count == 0)
                return new CustomBasicList<DatabaseMapping>();
            CustomBasicList<DatabaseMapping> mapList = GetMappingList<E>(out string _);
            CustomBasicList<DatabaseMapping> newList = new CustomBasicList<DatabaseMapping>();
            updateList.ForEach(items =>
            {
                DatabaseMapping thisMap = FindMappingForProperty(items, mapList);
                thisMap.Value = items.Value;
                if (items.Property == "ID")
                    throw new BasicBlankException("You are not allowed to update the ID");
                newList.Add(thisMap); //if you are doing manually, then you don't even update the object.
            });
            return newList; //hopefully its that simple.
        }
        private static CustomBasicList<DatabaseMapping> GetParameterMappings<E>(E thisEntity, CustomBasicList<UpdateFieldInfo> updateList) where E : class
        {
            if (updateList.Count == 0)
                return new CustomBasicList<DatabaseMapping>();
            CustomBasicList<DatabaseMapping> mapList = GetMappingList(thisEntity, out string _);
            CustomBasicList<DatabaseMapping> newList = new CustomBasicList<DatabaseMapping>();
            updateList.ForEach(items =>
            {
                DatabaseMapping thisMap = FindMappingForProperty(items, mapList);
                thisMap.Value = thisMap.PropertyDetails.GetValue(thisEntity, null);
                if (items.Property == "ID")
                    throw new BasicBlankException("You are not allowed to update the ID");
                newList.Add(thisMap); //if you are doing manually, then you don't even update the object.
            });
            return newList; //hopefully its that simple.
        }
        private static CustomBasicList<DatabaseMapping> GetParameterMappings<E>(E thisEntity) where E : class, IUpdatableEntity
        {
            CustomBasicList<UpdateFieldInfo> updateList = new CustomBasicList<UpdateFieldInfo>();
            CustomBasicList<string> firstList = thisEntity.GetChanges();
            updateList.Append(firstList);
            return GetParameterMappings(thisEntity, updateList);
        }
        private static CustomBasicList<DatabaseMapping> GetParameterMappings<E>(E thisEntity, EnumUpdateCategory category) where E : class
        {
            CustomBasicList<UpdateFieldInfo> updateList = new CustomBasicList<UpdateFieldInfo>();
            if (category == EnumUpdateCategory.Auto)
            {
                return GetParameterMappings((IUpdatableEntity)thisEntity);
            }
            CustomBasicList<DatabaseMapping> mapList = GetMappingList(thisEntity, out string _); //maybe that will fix that problem.
            if (category == EnumUpdateCategory.Common)
                mapList.RemoveAllOnly(Items => Items.CommonForUpdating == false);
            else
                mapList.RemoveAllOnly(Items => Items.DatabaseName == "ID");
            mapList.ForEach(items =>
            {
                items.Value = items.PropertyDetails.GetValue(thisEntity, null);
            });
            return mapList;
        }
    }
}