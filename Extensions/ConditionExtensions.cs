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
using Dapper;
using System.Data.SqlClient;
using System.Data;
using System.Collections.Generic; //probably going to be forced to use a dictionary for multiple tables.  especially for one to many relationships
using DapperHelpersLibrary.Extensions;
using DapperHelpersLibrary.ConditionClasses;
using cs = DapperHelpersLibrary.ConditionClasses.ConditionOperators; // just in case you need conditions.
using DapperHelpersLibrary.EntityInterfaces;
using static DapperHelpersLibrary.SQLHelpers.StatementFactoryUpdates;
using DapperHelpersLibrary.SQLHelpers;
//i think this is the most common things i like to do
namespace DapperHelpersLibrary.Extensions
{
    public static class ConditionExtensions
    {

       
        public static CustomBasicList<ICondition> AppendCondition(this CustomBasicList<ICondition> TempList, string Property, object Value)
        {
            return TempList.AppendCondition(Property, cs.Equals, Value);
        }

        public static CustomBasicList<ICondition> AppendRangeCondition(this CustomBasicList<ICondition> TempList, string Property,
            object LowRange, object HighRange)
        {
            return TempList.AppendCondition(Property, cs.GreaterOrEqual, LowRange).AppendCondition(Property, cs.LessThanOrEqual, HighRange);
        }

        public static CustomBasicList<ICondition> AppendCondition(this CustomBasicList<ICondition> TempList, string Property, string Operator, object Value)
        {
            AndCondition ThisCon = new AndCondition();
            ThisCon.Property = Property;
            ThisCon.Operator = Operator;
            ThisCon.Value = Value;
            TempList.Add(ThisCon);
            return TempList;
        }

        public static CustomBasicList<ICondition> AppendContains(this CustomBasicList<ICondition> TempList, CustomBasicList<int> ContainList)
        {
            SpecificListCondition ThisCon = new SpecificListCondition();
            ThisCon.ItemList = ContainList;
            TempList.Add(ThisCon);
            return TempList;
        }
        public static CustomBasicList<ICondition> AppendsNot(this CustomBasicList<ICondition> TempList, CustomBasicList<int> NotList)
        {
            NotListCondition Thiscon = new NotListCondition();
            Thiscon.ItemList = NotList;
            TempList.Add(Thiscon);
            return TempList;
        }

        public static OrCondition AppendOr(this OrCondition ThisOr , string Property, object Value)
        {
            var ThisCon = new AndCondition();
            ThisCon.Property = Property;
            ThisCon.Value = Value;
            ThisOr.ConditionList.Add(ThisCon);
            return ThisOr;
        }

        public static OrCondition AppendOr(this OrCondition ThisOr , string Property, string Operator, object Value)
        {
            var ThisCon = new AndCondition();
            ThisCon.Property = Property;
            ThisCon.Operator = Operator;
            ThisCon.Value = Value;
            ThisOr.ConditionList.Add(ThisCon);
            return ThisOr;
        }
    }
}
