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
using System.Data;
using System.Data.SQLite;
using CommonBasicStandardLibraries.AdvancedGeneralFunctionsAndProcesses.Misc;
using Dapper;
using static DapperHelpersLibrary.MapHelpers.MapBaseHelperClass;
using static DapperHelpersLibrary.Extensions.ReflectionDatabase;
using System.Collections.Generic;
using DapperHelpersLibrary.EntityInterfaces;
using static DapperHelpersLibrary.SQLHelpers.SimpleStatementHelpers;
using System.Data.SqlClient;
using DapperHelpersLibrary.SQLHelpers;
using DapperHelpersLibrary.ConditionClasses;
using cs = DapperHelpersLibrary.ConditionClasses.ConditionOperators;
using static DapperHelpersLibrary.SQLHelpers.SortInfo;
//i think this is the most common things i like to do
namespace DapperHelpersLibrary.Extensions
{
    public static class ListExtensions
    {
        public static string GetNewValue(this Dictionary<string, int> ThisDict, string Parameter)
        {
            bool rets;
            rets = ThisDict.TryGetValue(Parameter, out int Value);
            if (rets == true)
            {
                Value++;
                ThisDict[Parameter] = Value;
                return $"{Parameter}{Value}";
            }
            ThisDict.Add(Parameter, 1);
            return $"{Parameter}1";
        }

        public static CustomBasicList<int> GetIDList<E>(this CustomBasicList<E> ThisList) where E : ISimpleDapperEntity
        {
            return ThisList.Select(Items => Items.ID).ToCustomBasicList();
        }

        public static void InitalizeAll<E> (this CustomBasicList<E> ThisList) where E: IUpdatableEntity
        {
            ThisList.ForEach(Items => Items.Initialize());
        }

        public static CustomBasicList<SortInfo> Append(this CustomBasicList<SortInfo> SortList, string Property)
        {
            return SortList.Append(Property, EnumOrderBy.Ascending);
        }

        public static CustomBasicList<SortInfo> Append(this CustomBasicList<SortInfo> SortList, string Property, EnumOrderBy OrderBy)
        {
            SortList.Add(new SortInfo() { Property = Property, OrderBy = OrderBy });
            return SortList;
        }


        public static CustomBasicList<UpdateFieldInfo> Append(this CustomBasicList<UpdateFieldInfo> TempList, string ThisProperty) //if it needs to be something else. rethink
        {
            TempList.Add(new UpdateFieldInfo(ThisProperty));
            return TempList;
            //ConditionActionPair<T> ThisC = new ConditionActionPair<T>(Match, Action, Value);
            //TempList.Add(ThisC);
            //return TempList;
        }

        public static CustomBasicList<UpdateFieldInfo> Append(this CustomBasicList<UpdateFieldInfo> TempList, CustomBasicList<string> PropList)
        {
            PropList.ForEach(Items =>
            {
                TempList.Add(new UpdateFieldInfo(Items));
            });
            return TempList;
        }

        public static CustomBasicList<UpdateEntity> Append(this CustomBasicList<UpdateEntity> TempList, string ThisProperty, object Value) //if it needs to be something else. rethink
        {
            TempList.Add(new UpdateEntity(ThisProperty, Value));
            return TempList;
            //ConditionActionPair<T> ThisC = new ConditionActionPair<T>(Match, Action, Value);
            //TempList.Add(ThisC);
            //return TempList;
        }

       

    }
}
