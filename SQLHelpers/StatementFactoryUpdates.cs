using CommonBasicLibraries.AdvancedGeneralFunctionsAndProcesses.BasicExtensions;
using CommonBasicLibraries.AdvancedGeneralFunctionsAndProcesses.Misc;
using CommonBasicLibraries.BasicDataSettingsAndProcesses;
using CommonBasicLibraries.CollectionClasses;
using CommonBasicLibraries.DatabaseHelpers.EntityInterfaces;
using CommonBasicLibraries.DatabaseHelpers.Extensions;
using CommonBasicLibraries.DatabaseHelpers.MiscClasses;
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
        internal static (string sqls, BasicList<DatabaseMapping> ParameterMappings) GetUpdateStatement<E>(E thisEntity, BasicList<UpdateFieldInfo> manuelList) where E : class
        {
            if (manuelList.Count == 0)
            {
                throw new CustomBasicException("If you are manually updating, you havve to send at least one field to update");
            }
            BasicList<DatabaseMapping> updateList = GetParameterMappings<E>(thisEntity, manuelList);
            return GetUpdateStatement(updateList);
        }

        internal static (string sqls, BasicList<DatabaseMapping> ParameterMappings) GetUpdateStatement<E>(BasicList<UpdateEntity> manuelList) where E : class
        {
            if (manuelList.Count == 0)
            {
                throw new CustomBasicException("If you are manually updating, you havve to send at least one field to update");
            }
            BasicList<DatabaseMapping> updateList = GetParameterMappings<E>(manuelList);
            return GetUpdateStatement(updateList);
        }

        internal static (string sqls, BasicList<DatabaseMapping> ParameterMappings) GetUpdateStatement<E>(E thisEntity) where E : class, IUpdatableEntity
        {
            BasicList<DatabaseMapping> updateList = GetParameterMappings(thisEntity);
            return GetUpdateStatement(updateList);
        }

        internal static (string sqls, BasicList<DatabaseMapping> ParameterMappings) GetUpdateStatement<E>(E thisEntity, EnumUpdateCategory category) where E : class
        {
            BasicList<DatabaseMapping> updateList = GetParameterMappings(thisEntity, category);
            return GetUpdateStatement(updateList);
        }
        private static (string sqls, BasicList<DatabaseMapping> ParameterMappings) GetUpdateStatement(BasicList<DatabaseMapping> updateList)
        {
            if (updateList.Count == 0)
            {
                return ("", new ());
            }
            StringBuilder thisStr = new ();
            string tableName = updateList.First().TableName; //found a use for this.
            thisStr.Append("update ");
            thisStr.Append(tableName);
            thisStr.Append(" set ");
            StrCat cats = new ();
            updateList.ForEach(xx =>
            {
                cats.AddToString($"{xx.DatabaseName} = @{xx.DatabaseName}", ", ");
            });
            thisStr.Append(cats.GetInfo());
            thisStr.Append(" where ID = @ID");
            return (thisStr.ToString(), updateList);
        }
        private static BasicList<DatabaseMapping> GetParameterMappings<E>(BasicList<UpdateEntity> updateList) where E : class
        {
            if (updateList.Count == 0)
                return new ();
            BasicList<DatabaseMapping> mapList = GetMappingList<E>(out string _);
            BasicList<DatabaseMapping> newList = new();
            updateList.ForEach(items =>
            {
                DatabaseMapping thisMap = FindMappingForProperty(items, mapList);
                thisMap.Value = items.Value;
                if (items.Property == "ID")
                {
                    throw new CustomBasicException("You are not allowed to update the ID");
                }
                newList.Add(thisMap); //if you are doing manually, then you don't even update the object.
            });
            return newList; //hopefully its that simple.
        }
        private static BasicList<DatabaseMapping> GetParameterMappings<E>(E thisEntity, BasicList<UpdateFieldInfo> updateList) where E : class
        {
            if (updateList.Count == 0)
            {
                return new BasicList<DatabaseMapping>();
            }
            BasicList<DatabaseMapping> mapList = GetMappingList(thisEntity, out string _);
            BasicList<DatabaseMapping> newList = new ();
            updateList.ForEach(items =>
            {
                DatabaseMapping thisMap = FindMappingForProperty(items, mapList);
                thisMap.Value = thisMap.PropertyDetails.GetValue(thisEntity, null);
                if (items.Property == "ID")
                {
                    throw new CustomBasicException("You are not allowed to update the ID");
                }
                newList.Add(thisMap); //if you are doing manually, then you don't even update the object.
            });
            return newList; //hopefully its that simple.
        }
        private static BasicList<DatabaseMapping> GetParameterMappings<E>(E thisEntity) where E : class, IUpdatableEntity
        {
            BasicList<UpdateFieldInfo> updateList = new ();
            BasicList<string> firstList = thisEntity.GetChanges();
            updateList.Append(firstList);
            return GetParameterMappings(thisEntity, updateList);
        }
        private static BasicList<DatabaseMapping> GetParameterMappings<E>(E thisEntity, EnumUpdateCategory category) where E : class
        {
            BasicList<UpdateFieldInfo> updateList = new ();
            if (category == EnumUpdateCategory.Auto)
            {
                return GetParameterMappings((IUpdatableEntity)thisEntity);
            }
            BasicList<DatabaseMapping> mapList = GetMappingList(thisEntity, out string _); //maybe that will fix that problem.
            if (category == EnumUpdateCategory.Common)
            {
                mapList.RemoveAllOnly(Items => Items.CommonForUpdating == false);
            }
            else
            {
                mapList.RemoveAllOnly(Items => Items.DatabaseName == "ID");
            }
            mapList.ForEach(items =>
            {
                items.Value = items.PropertyDetails.GetValue(thisEntity, null);
            });
            return mapList;
        }
    }
}