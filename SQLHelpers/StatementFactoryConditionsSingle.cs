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
using DapperHelpersLibrary.ConditionClasses;
using static DapperHelpersLibrary.SQLHelpers.SimpleStatementHelpers;
using cs = DapperHelpersLibrary.ConditionClasses.ConditionOperators;
using System.Collections.Generic;
using DapperHelpersLibrary.Extensions;
//i think this is the most common things i like to do
namespace DapperHelpersLibrary.SQLHelpers
{
    internal static class StatementFactoryConditionsSingle //this is single table ones.
    {
        //start out with public so i can do some testing.
        //once i test this part, apply to something else.

        

        public static (string sqls, CustomBasicList<DatabaseMapping> ParameterMappings)  GetConditionalStatement(CustomBasicList<DatabaseMapping> MapList, string TableName,
            CustomBasicList<ICondition> ConditionList, CustomBasicList<SortInfo> SortList, EnumDatabaseCategory Database, EnumSQLCategory Category = EnumSQLCategory.Normal,
            int HowMany = 0, string Property = "")
        {
            StringBuilder ThisStr = new StringBuilder();
            if (Category != EnumSQLCategory.Delete)
                ThisStr.Append(GetSimpleSelectStatement(MapList, TableName, Database, Category, HowMany, Property)); //for now, just also delete.
            else
                ThisStr.Append(GetDeleteStatement(TableName));
            var ParamList = new CustomBasicList<DatabaseMapping>();
            if (ConditionList == null)
            {
                ThisStr.Append(GetSortStatement(MapList, SortList, false));
                return (ThisStr.ToString(), null); //there are times when we don't even have one
            }
            ThisStr.Append(" Where ");
            Dictionary<string, int> ThisDict = new Dictionary<string, int>();
            //for now, has to see what i expect and do unit tests on it.
            //for now, use all.  can change later if necessary.

            CustomBasicList<AndCondition> AndList = ConditionList.Where(Items => Items.ConditionCategory == EnumConditionCategory.And).ToCastedList<AndCondition>();


            bool NeedsAppend;
            if (AndList.Count > 0)
                NeedsAppend = true;
            else
                NeedsAppend = false;
            ThisStr.Append(PopulatAnds(AndList, MapList, " and ", ParamList, ThisDict));
            CustomBasicList<OrCondition> OrList = ConditionList.Where(Items => Items.ConditionCategory == EnumConditionCategory.Or).ToCastedList<OrCondition>();
            StrCat cats = new StrCat(); 
            if (OrList.Count > 0)
            {
                if (NeedsAppend == true)
                    ThisStr.Append(" and ");
                NeedsAppend = true;
                ThisStr.Append("(");
                OrList.ForEach(Items => cats.AddToString(PopulatAnds(Items.ConditionList, MapList, " or ", ParamList, ThisDict), ") and (" ));
                ThisStr.Append(cats.GetInfo());
                ThisStr.Append(")");
            }
            CustomBasicList<SpecificListCondition> IncludeList = ConditionList.Where(Items => Items.ConditionCategory == EnumConditionCategory.ListInclude).ToCastedList<SpecificListCondition>();
            if (IncludeList.Count > 1)
            {
                //looks like we could have more than one include list.  the way to fix this is to append and make into one.
                CustomBasicList<int> NewList = new CustomBasicList<int>();
                IncludeList.ForEach(Items =>
                {
                    NewList.AddRange(Items.ItemList);
                });
                IncludeList = new CustomBasicList<SpecificListCondition>();
                SpecificListCondition ThisI = new SpecificListCondition();
                ThisI.ItemList = NewList;
                IncludeList.Add(ThisI);
            }
                //throw new BasicBlankException("You can only have one include list");
            if (IncludeList.Count == 1)
            {
                if (NeedsAppend == true)
                    ThisStr.Append(" and ");
                NeedsAppend = true;
                ThisStr.Append("ID in (");
                ThisStr.Append(PopulateListInfo(IncludeList.Single().ItemList));
                //cats = new StrCat();
                //IncludeList.Single().ItemList.ForEach(Items => cats.AddToString(Items.ToString(), ", "));
                //ThisStr.Append(cats.GetInfo());
                //ThisStr.Append(")");
            }
            CustomBasicList<NotListCondition> NotList = ConditionList.Where(Items => Items.ConditionCategory == EnumConditionCategory.ListNot).ToCastedList<NotListCondition>();
            if (NotList.Count > 1)
                throw new BasicBlankException("You can only have one not list");
            if (NotList.Count == 1)
            {
                if (NeedsAppend == true)
                    ThisStr.Append(" and ");
                ThisStr.Append("ID not in (");
                ThisStr.Append(PopulateListInfo(NotList.Single().ItemList));
            }
            if (SortList != null)
                ThisStr.Append(GetSortStatement(MapList, SortList, false));
            if (Database == EnumDatabaseCategory.SQLite && HowMany != 0)
                ThisStr.Append($" limit {HowMany}");
            return (ThisStr.ToString(), ParamList);
        }

        private static string PopulateListInfo(CustomBasicList<int> ThisList)
        {
            StrCat cats = new StrCat();
            ThisList.ForEach(Items => cats.AddToString(Items.ToString(), ", "));
            StringBuilder ThisStr = new StringBuilder();
            ThisStr.Append(cats.GetInfo());
            ThisStr.Append(")");
            return ThisStr.ToString();
        }

        private static string PopulatAnds(CustomBasicList<AndCondition> AndList, CustomBasicList<DatabaseMapping> MapList, string Seperator, CustomBasicList<DatabaseMapping> ParamList, Dictionary<string, int> ThisDict)
        {
            StrCat cats = new StrCat();
            AndList.ForEach(Items =>
            {
                DatabaseMapping ThisMap = FindMappingForProperty(Items, MapList);

                if (Items.Operator == cs.IsNotNull || Items.Operator == cs.IsNull)
                {
                    cats.AddToString($"{ThisMap.DatabaseName} {Items.Operator}", Seperator); //i am guessing that with parameters no need to worry about the null parts
                }
                else
                {
                    if (Items.Operator == cs.Like)
                        ThisMap.Like = true;
                    ParamList.Add(ThisMap); //i think
                    object RealValue;
                    if (bool.TryParse(Items.Value.ToString(), out bool NewBool) == false)
                        RealValue = Items.Value;
                    else if (NewBool == true)
                        RealValue = 1;
                    else
                        RealValue = 0;
                    ThisMap.Value = RealValue;
                    //cats.AddToString($"{ThisMap.DatabaseName} {Items.Operator} {RealValue}", Seperator);
                    ThisMap.ObjectName = ThisDict.GetNewValue(ThisMap.DatabaseName);
                    cats.AddToString($"{ThisMap.DatabaseName} {Items.Operator} @{ThisMap.ObjectName}", Seperator);
                    //if (Items.Operator == cs.Like)
                    //    cats.AddToString($"{ThisMap.DatabaseName} {Items.Operator} @%{ThisMap.ObjectName}%", Seperator);
                    //else
                        

                    //cats.AddToString($"{ThisMap.DatabaseName} {Items.Operator} @{ThisMap.DatabaseName}", Seperator);


                }
                //for now, has to be this way so i can test the statements.  later will do parameters
                //
            });
            return cats.GetInfo();
        }

    }
}
