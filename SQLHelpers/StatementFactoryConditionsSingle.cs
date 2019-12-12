using CommonBasicStandardLibraries.AdvancedGeneralFunctionsAndProcesses.BasicExtensions;
using CommonBasicStandardLibraries.AdvancedGeneralFunctionsAndProcesses.Misc;
using CommonBasicStandardLibraries.CollectionClasses;
using CommonBasicStandardLibraries.DatabaseHelpers.ConditionClasses;
using CommonBasicStandardLibraries.DatabaseHelpers.MiscClasses;
using CommonBasicStandardLibraries.Exceptions;
using DapperHelpersLibrary.Extensions;
using DapperHelpersLibrary.MapHelpers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static DapperHelpersLibrary.MapHelpers.MapBaseHelperClass;
using static DapperHelpersLibrary.SQLHelpers.SimpleStatementHelpers;
using cs = CommonBasicStandardLibraries.DatabaseHelpers.ConditionClasses.ConditionOperators;
namespace DapperHelpersLibrary.SQLHelpers
{
    internal static class StatementFactoryConditionsSingle //this is single table ones.
    {
        public static (string sqls, CustomBasicList<DatabaseMapping> ParameterMappings) GetConditionalStatement(CustomBasicList<DatabaseMapping> mapList, string tableName,
            CustomBasicList<ICondition>? conditionList, CustomBasicList<SortInfo>? sortList, EnumDatabaseCategory database, EnumSQLCategory category = EnumSQLCategory.Normal,
            int howMany = 0, string property = "")
        {
            StringBuilder thisStr = new StringBuilder();
            if (category != EnumSQLCategory.Delete)
                thisStr.Append(GetSimpleSelectStatement(mapList, tableName, database, category, howMany, property)); //for now, just also delete.
            else
                thisStr.Append(GetDeleteStatement(tableName));
            var paramList = new CustomBasicList<DatabaseMapping>();
            if (conditionList == null)
            {
                thisStr.Append(GetSortStatement(mapList, sortList, false));
                return (thisStr.ToString()!, null!); //there are times when we don't even have one
            }
            thisStr.Append(" Where ");
            Dictionary<string, int> thisDict = new Dictionary<string, int>();
            CustomBasicList<AndCondition> andList = conditionList.Where(Items => Items.ConditionCategory == EnumConditionCategory.And).ToCastedList<AndCondition>();
            bool needsAppend;
            if (andList.Count > 0)
                needsAppend = true;
            else
                needsAppend = false;
            thisStr.Append(PopulatAnds(andList, mapList, " and ", paramList, thisDict));
            CustomBasicList<OrCondition> orList = conditionList.Where(Items => Items.ConditionCategory == EnumConditionCategory.Or).ToCastedList<OrCondition>();
            StrCat cats = new StrCat();
            if (orList.Count > 0)
            {
                if (needsAppend == true)
                    thisStr.Append(" and ");
                needsAppend = true;
                thisStr.Append("(");
                orList.ForEach(Items => cats.AddToString(PopulatAnds(Items.ConditionList, mapList, " or ", paramList, thisDict), ") and ("));
                thisStr.Append(cats.GetInfo());
                thisStr.Append(")");
            }
            CustomBasicList<SpecificListCondition> includeList = conditionList.Where(Items => Items.ConditionCategory == EnumConditionCategory.ListInclude).ToCastedList<SpecificListCondition>();
            if (includeList.Count > 1)
            {
                //looks like we could have more than one include list.  the way to fix this is to append and make into one.
                CustomBasicList<int> newList = new CustomBasicList<int>();
                includeList.ForEach(Items =>
                {
                    newList.AddRange(Items.ItemList);
                });
                includeList = new CustomBasicList<SpecificListCondition>();
                SpecificListCondition thisI = new SpecificListCondition();
                thisI.ItemList = newList;
                includeList.Add(thisI);
            }
            if (includeList.Count == 1)
            {
                if (needsAppend == true)
                    thisStr.Append(" and ");
                needsAppend = true;
                thisStr.Append("ID in (");
                thisStr.Append(PopulateListInfo(includeList.Single().ItemList));
            }
            CustomBasicList<NotListCondition> notList = conditionList.Where(Items => Items.ConditionCategory == EnumConditionCategory.ListNot).ToCastedList<NotListCondition>();
            if (notList.Count > 1)
                throw new BasicBlankException("You can only have one not list");
            if (notList.Count == 1)
            {
                if (needsAppend == true)
                    thisStr.Append(" and ");
                thisStr.Append("ID not in (");
                thisStr.Append(PopulateListInfo(notList.Single().ItemList));
            }
            if (sortList != null)
                thisStr.Append(GetSortStatement(mapList, sortList, false));
            if (database == EnumDatabaseCategory.SQLite && howMany != 0)
                thisStr.Append($" limit {howMany}");
            return (thisStr.ToString(), paramList);
        }
        private static string PopulateListInfo(CustomBasicList<int> thisList)
        {
            StrCat cats = new StrCat();
            thisList.ForEach(items => cats.AddToString(items.ToString(), ", "));
            StringBuilder thisStr = new StringBuilder();
            thisStr.Append(cats.GetInfo());
            thisStr.Append(")");
            return thisStr.ToString();
        }
        private static string PopulatAnds(CustomBasicList<AndCondition> andList, CustomBasicList<DatabaseMapping> mapList, string seperator, CustomBasicList<DatabaseMapping> paramList, Dictionary<string, int> thisDict)
        {
            StrCat cats = new StrCat();
            andList.ForEach(items =>
            {
                DatabaseMapping thisMap = FindMappingForProperty(items, mapList);
                if (items.Operator == cs.IsNotNull || items.Operator == cs.IsNull)
                {
                    cats.AddToString($"{thisMap.DatabaseName} {items.Operator}", seperator); //i am guessing that with parameters no need to worry about the null parts
                }
                else
                {
                    if (items.Operator == cs.Like)
                        thisMap.Like = true;
                    paramList.Add(thisMap); //i think
                    object realValue;
                    if (bool.TryParse(items.Value!.ToString(), out bool NewBool) == false)
                        realValue = items.Value;
                    else if (NewBool == true)
                        realValue = 1;
                    else
                        realValue = 0;
                    thisMap.Value = realValue;
                    thisMap.ObjectName = thisDict.GetNewValue(thisMap.DatabaseName);
                    cats.AddToString($"{thisMap.DatabaseName} {items.Operator} @{thisMap.ObjectName}", seperator);
                }
            });
            return cats.GetInfo();
        }
    }
}