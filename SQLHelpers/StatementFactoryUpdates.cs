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
using DapperHelpersLibrary.MapHelpers;
using CommonBasicStandardLibraries.AdvancedGeneralFunctionsAndProcesses.Misc;
using static DapperHelpersLibrary.MapHelpers.MapBaseHelperClass;
using System.Collections.Generic;
using DapperHelpersLibrary.EntityInterfaces;
using DapperHelpersLibrary.Attributes;
using System.Reflection;
using DapperHelpersLibrary.Extensions;
//i think this is the most common things i like to do
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

        internal static (string sqls, CustomBasicList<DatabaseMapping> ParameterMappings) GetUpdateStatement<E>(E ThisEntity, CustomBasicList<UpdateFieldInfo> ManuelList) where E : class
        {
            //could return ""  something else has to decide what to do.
            if (ManuelList.Count == 0)
                throw new BasicBlankException("If you are manually updating, you havve to send at least one field to update");
            CustomBasicList<DatabaseMapping> UpdateList = GetParameterMappings<E>(ThisEntity, ManuelList);
            return GetUpdateStatement(UpdateList);
        }

        internal static (string sqls, CustomBasicList<DatabaseMapping> ParameterMappings) GetUpdateStatement<E>(CustomBasicList<UpdateEntity> ManuelList) where E : class
        {
            //could return ""  something else has to decide what to do.
            if (ManuelList.Count == 0)
                throw new BasicBlankException("If you are manually updating, you havve to send at least one field to update");
            CustomBasicList<DatabaseMapping> UpdateList = GetParameterMappings<E>(ManuelList);
            return GetUpdateStatement(UpdateList);
        }

        internal static (string sqls, CustomBasicList<DatabaseMapping> ParameterMappings) GetUpdateStatement<E>(E ThisEntity) where E : class, IUpdatableEntity
        {
            CustomBasicList<DatabaseMapping> UpdateList = GetParameterMappings(ThisEntity);
            return GetUpdateStatement(UpdateList);
        }

        internal static (string sqls, CustomBasicList<DatabaseMapping> ParameterMappings) GetUpdateStatement<E>(E ThisEntity, EnumUpdateCategory Category) where E : class
        {
            //could return ""  something else has to decide what to do.
            CustomBasicList<DatabaseMapping> UpdateList = GetParameterMappings(ThisEntity, Category);
            return GetUpdateStatement(UpdateList);
        }

        private static (string sqls, CustomBasicList<DatabaseMapping> ParameterMappings) GetUpdateStatement(CustomBasicList<DatabaseMapping> UpdateList)
        {
            if (UpdateList.Count == 0)
                return ("", new CustomBasicList<DatabaseMapping>());
            StringBuilder ThisStr = new StringBuilder();
            string TableName = UpdateList.First().TableName; //found a use for this.
            ThisStr.Append("update ");
            ThisStr.Append(TableName);
            ThisStr.Append(" set ");
            StrCat cats = new StrCat();
            UpdateList.ForEach(Items =>
            {
                cats.AddToString($"{Items.DatabaseName} = @{Items.DatabaseName}", ", ");
            });
            ThisStr.Append(cats.GetInfo());
            ThisStr.Append(" where ID = @ID");
            return (ThisStr.ToString(), UpdateList);
        }




        private static CustomBasicList<DatabaseMapping> GetParameterMappings<E>(CustomBasicList<UpdateEntity> UpdateList) where E : class
        {
            if (UpdateList.Count == 0)
                return new CustomBasicList<DatabaseMapping>();
            CustomBasicList<DatabaseMapping> MapList = GetMappingList<E>(out string _);
            CustomBasicList<DatabaseMapping> NewList = new CustomBasicList<DatabaseMapping>();
            UpdateList.ForEach(Items =>
            {
                DatabaseMapping ThisMap = FindMappingForProperty(Items, MapList);
                ThisMap.Value = Items.Value;
                if (Items.Property == "ID")
                    throw new BasicBlankException("You are not allowed to update the ID");
                NewList.Add(ThisMap); //if you are doing manually, then you don't even update the object.
            });
            return NewList; //hopefully its that simple.
        }


        private static CustomBasicList<DatabaseMapping> GetParameterMappings<E>(E ThisEntity, CustomBasicList<UpdateFieldInfo> UpdateList) where E : class
        {
            if (UpdateList.Count == 0)
                return new CustomBasicList<DatabaseMapping>();
            //CustomBasicList<DatabaseMapping> MapList = GetMappingList<E>(out string _);
            CustomBasicList<DatabaseMapping> MapList = GetMappingList(ThisEntity, out string _);
            CustomBasicList<DatabaseMapping> NewList = new CustomBasicList<DatabaseMapping>();
            UpdateList.ForEach(Items =>
            {
                DatabaseMapping ThisMap = FindMappingForProperty(Items, MapList);
                ThisMap.Value = ThisMap.PropertyDetails.GetValue(ThisEntity, null);
                if (Items.Property == "ID")
                    throw new BasicBlankException("You are not allowed to update the ID");
                NewList.Add(ThisMap); //if you are doing manually, then you don't even update the object.
            });
            return NewList; //hopefully its that simple.
        }

        private static CustomBasicList<DatabaseMapping> GetParameterMappings<E>(E ThisEntity) where E : class, IUpdatableEntity
        {
            CustomBasicList<UpdateFieldInfo> UpdateList = new CustomBasicList<UpdateFieldInfo>();
            CustomBasicList<string> FirstList = ThisEntity.GetChanges();
            UpdateList.Append(FirstList);
            return GetParameterMappings(ThisEntity, UpdateList);
        }

        //if i decide to have conditional updates not just id or updating based on those, rethinking is required.
        //of course a person can always do a manuel sql statement as well.
        private static CustomBasicList<DatabaseMapping> GetParameterMappings<E>(E ThisEntity, EnumUpdateCategory Category) where E: class
        {
            CustomBasicList<UpdateFieldInfo> UpdateList = new CustomBasicList<UpdateFieldInfo>();
            if (Category == EnumUpdateCategory.Auto)
            {
                return GetParameterMappings((IUpdatableEntity) ThisEntity);
            }
            CustomBasicList<DatabaseMapping> MapList = GetMappingList(ThisEntity,  out string _); //maybe that will fix that problem.
            if (Category == EnumUpdateCategory.Common)
                MapList.RemoveAllOnly(Items => Items.CommonForUpdating == false);
            else
                MapList.RemoveAllOnly(Items => Items.DatabaseName == "ID");
            //return GetParameterMappings(ThisEntity, UpdateList, MapList);
            MapList.ForEach(Items =>
            {
                Items.Value = Items.PropertyDetails.GetValue(ThisEntity, null);
            });
            return MapList;
        }

    }
}