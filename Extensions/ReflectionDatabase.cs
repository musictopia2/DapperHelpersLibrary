using DapperHelpersLibrary.Attributes;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using CommonBasicStandardLibraries.AdvancedGeneralFunctionsAndProcesses.BasicExtensions;
using CommonBasicStandardLibraries.CollectionClasses;
using DapperHelpersLibrary.EntityInterfaces;
using System.Linq;
using CommonBasicStandardLibraries.Exceptions;

namespace DapperHelpersLibrary.Extensions
{
    public static class ReflectionDatabase
    {

        public static string GetJoiner<E, D>()
        {
            Type FirstType = typeof(E);
            string SecondName;
            Type SecondType = typeof(D);
            SecondName = SecondType.Name;
            CustomBasicList<PropertyInfo> ThisList = FirstType.GetPropertiesWithAttribute<ForeignKeyAttribute>().ToCustomBasicList();
            CustomBasicList<PropertyInfo> NewList = ThisList.Where(Items =>
            {
                ForeignKeyAttribute ThisKey = Items.GetCustomAttribute<ForeignKeyAttribute>();
                if (ThisKey.ClassName == SecondName)
                    return true;
                return false;
            }).ToCustomBasicList();
            if (NewList.Count == 0)

                throw new BasicBlankException($"No Key Found Linking {SecondName}");
            if (NewList.Count > 1)
                throw new BasicBlankException($"Duplicate Key Found Linking {SecondName}");
            //return NewList.Single().Name;
            PropertyInfo ThisProp = NewList.Single();
            ColumnAttribute ThisCol = ThisProp.GetCustomAttribute<ColumnAttribute>();
            if (ThisCol == null)
                return ThisProp.Name;
            return ThisCol.ColumnName;
        }

        //try the other way.  not sure if it will cause other problems (?)


        public static bool CanMapToDatabase(this PropertyInfo property)
        {
            //var attribute = property.GetAttribute<NotMappedAttribute>();
            bool rets = property.HasAttribute<NotMappedAttribute>();
            if (rets == true)
                return false; //because its not mapped.
            return property.IsSimpleType();
        }
        //can try to figure out table name.  i think i like this idea.

        public static string GetTableName<E>() //decided to put here even though you start with no parameters.
        {
            Type ThisType = typeof(E);
            return ThisType.GetTableName();
        }
        public static string GetTableName(this Type ThisType)
        {
            TableAttribute ThisTable = ThisType.GetCustomAttribute<TableAttribute>();
            if (ThisTable == null)
                return ThisType.Name;
            return ThisTable.TableName;
        }
        public static bool HasJoiner<E>(string ClassName)
        {
            Type ThisType = typeof(E);
            return ThisType.HasJoiner(ClassName);
        }
        public static bool HasJoiner(this Type ThisType, string ClassName)
        {
            //CustomBasicList<ForeignKeyAttribute> PossibleList = ThisType.GetCustomAttributes<ForeignKeyAttribute>().ToCustomBasicList();
            CustomBasicList<ForeignKeyAttribute> PossibleList = ThisType.GetCustomAttributes<ForeignKeyAttribute>();
            return PossibleList.Any(Items => Items.ClassName == ClassName);
        }
        public static string GetColumnName(this PropertyInfo ThisProperty)
        {
            ColumnAttribute ThisColumn = ThisProperty.GetAttribute<ColumnAttribute>();
            if (ThisColumn != null)
                return ThisColumn.ColumnName;
            return ThisProperty.Name; //i will have to see what i use the interface ones for.
        }
        public static string GetInterfaceName(this PropertyInfo ThisProperty)
        {
            InterfaceAttribute ThisInterface = ThisProperty.GetAttribute<InterfaceAttribute>();
            if (ThisInterface != null)
                return ThisInterface.InterfaceName;
            return ""; //sometimes this is not necessary
        }
        public static CustomBasicList<PropertyInfo> GetJoinedColumns(this Type ThisType)
        {
            return ThisType.GetPropertiesWithAttribute<PrimaryJoinedDataAttribute>().ToCustomBasicList();
        }
        public static bool IsAutoIncremented<E>()
        {
            Type ThisType = typeof(E);
            return ThisType.IsAutoIncremented();
        }
        public static bool IsAutoIncremented(this Type ThisType)
        {
            return  !ThisType.ClassContainsAttribute<NoIncrementAttribute>();

            //return !ThisType.HasAttribute<NoIncrement>(); //i think this time, needs the exclamation point.  otherwise, needs more code.
        }

        //public static object GetValueForDatabase(this PropertyInfo property, object ThisObj)
        //{
        //    bool rets = property.IsBool();
        //    object FirstO = property.GetValue(ThisObj, null);
        //    if (FirstO == null)
        //        return FirstO;
        //    if (rets == true)
        //    {
        //        bool FirstInfo = bool.Parse(FirstO.ToString());
        //        if (FirstInfo == true)
        //            return 1;
        //        return 0;
        //    }
        //    else
        //        return FirstO;
        //}


    }
}