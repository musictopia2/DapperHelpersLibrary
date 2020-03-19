using CommonBasicStandardLibraries.AdvancedGeneralFunctionsAndProcesses.BasicExtensions;
using CommonBasicStandardLibraries.AdvancedGeneralFunctionsAndProcesses.Misc;
using CommonBasicStandardLibraries.CollectionClasses;
using CommonBasicStandardLibraries.DatabaseHelpers.ConditionClasses;
using CommonBasicStandardLibraries.DatabaseHelpers.EntityInterfaces;
using CommonBasicStandardLibraries.DatabaseHelpers.Extensions;
using CommonBasicStandardLibraries.DatabaseHelpers.MiscClasses;
using CommonBasicStandardLibraries.Exceptions;
using DapperHelpersLibrary.MapHelpers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static CommonBasicStandardLibraries.DatabaseHelpers.Extensions.ReflectionDatabase;
using static DapperHelpersLibrary.MapHelpers.MapBaseHelperClass;
using static DapperHelpersLibrary.SQLHelpers.SimpleStatementHelpers;
using cs = CommonBasicStandardLibraries.DatabaseHelpers.ConditionClasses.ConditionOperators;
namespace DapperHelpersLibrary.SQLHelpers
{
    internal static class StatementSelectFactoryJoin
    {
        //this will handle joining.
        //only needed for select statements.
        //start out with joining 2 of them.  once mastered, work on 3.
        //even get the conditions with 2 done as well.
        //the simple join will not have parameters (because no where clause).
        //looks like i need an optional parameter.  if set, then opposite needs to be done.
        #region No Conditions
        public static string GetSimpleSelectStatement<E, D1, D2>(CustomBasicList<SortInfo>? sortList, EnumDatabaseCategory category, int howMany = 0) where E : class, ISimpleDapperEntity where D1 : class where D2 : class
        {
            StartList<E>(out CustomBasicList<DatabaseMapping> thisList, out CustomBasicList<string> joinList, out string tableName);
            //hopefully we don't need it with no conditions.  because that part only seems to be put in with conditions.
            //i may have to rethink some things too.
            AppendList<E, D1>(thisList, joinList, "b");
            AppendList<E, D2>(thisList, joinList, "c");
            string sqls = GetSimpleSelectStatement(thisList, joinList, tableName, category, howMany);
            if (sortList == null)
                return sqls;
            StringBuilder thisStr = new StringBuilder(sqls);
            thisStr.Append(GetSortStatement(thisList, sortList, false));
            thisStr.Append(GetLimitSQLite(category, howMany));
            return thisStr.ToString();
        }
        private static void StartList<E>(out CustomBasicList<DatabaseMapping> thisList, out CustomBasicList<string> joinList, out string tableName) where E : class
        {
            thisList = GetMappingList<E>(out tableName);
            joinList = new CustomBasicList<string>();
            thisList.ForEach(Items => Items.Prefix = "a");
        }
        private static void AppendList<E, D>(CustomBasicList<DatabaseMapping> thisList, CustomBasicList<string> joinList, string prefix, bool isOneToOne = true, string firsts = "a", string newTable = "") where D : class where E : class
        {
            CustomBasicList<DatabaseMapping> newList = GetMappingList<D>(out string tableName, isOneToOne); //unfortunately, it needs them all in that case since queries can be used for it.
            newList.ForEach(items => items.Prefix = prefix);
            thisList.AddRange(newList);
            string foreign;
            string otherTable;
            string thisStr;
            if (newTable != "")
                otherTable = newTable;
            else
                otherTable = typeof(D).GetTableName();
            if (isOneToOne == true)
            {
                foreign = GetJoiner<E, D>();
                thisStr = $"{otherTable} {prefix} on {firsts}.{foreign}={prefix}.ID";
            }
            else
            {
                foreign = GetJoiner<D, E>();
                thisStr = $"{otherTable} {prefix} on {firsts}.ID={prefix}.{foreign}";
            }
            joinList.Add(thisStr);
        }
        public static string GetSimpleSelectStatement<E, D1>(bool isOneToOne, CustomBasicList<SortInfo>? sortList, EnumDatabaseCategory category, int howMany = 0) where E : class, IJoinedEntity where D1 : class
        {
            StartList<E>(out CustomBasicList<DatabaseMapping> thisList, out CustomBasicList<string> joinList, out string tableName);
            AppendList<E, D1>(thisList, joinList, "b", isOneToOne);
            string sqls = GetSimpleSelectStatement(thisList, joinList, tableName, category, howMany);
            if (sortList == null)
                return sqls;
            StringBuilder thisStr = new StringBuilder(sqls);
            thisStr.Append(GetSortStatement(thisList, sortList, true));
            thisStr.Append(GetLimitSQLite(category, howMany));
            return thisStr.ToString();
        }
        private static string GetSimpleSelectStatement(CustomBasicList<DatabaseMapping> thisList, CustomBasicList<string> joinList, string tableName, EnumDatabaseCategory database, int howMany = 0) //sqlite requires limit at the end
        {
            if (joinList.Count == 0)
                throw new BasicBlankException("Needs at least one other table.  Otherwise, no join");
            StringBuilder thisStr = new StringBuilder("select ");
            StrCat cats = new StrCat();
            if (howMany > 0 && database == EnumDatabaseCategory.SQLServer) //sqlite requires it at the end.
                thisStr.Append($"top {howMany} ");
            thisList.ForEach(items =>
            {
                if (items.HasMatch == false)
                    cats.AddToString($"{items.Prefix}.{items.DatabaseName} as {items.ObjectName}", ", ");
                else
                    cats.AddToString($"{items.Prefix}.{items.DatabaseName}", ", ");
            });
            thisStr.Append(cats.GetInfo());
            thisStr.Append(" from ");
            thisStr.Append(tableName);
            thisStr.Append(" a left join ");
            cats = new StrCat();
            joinList.ForEach(Items => cats.AddToString(Items, " left join "));
            thisStr.Append(cats.GetInfo());
            return thisStr.ToString();
        }
        #endregion
        #region With Conditions
        //i am guessing that more gets added when it has conditions. so should be no need for repeating.
        private static (string sqls, CustomBasicList<DatabaseMapping> ParameterMappings) FinishConditionStatement(CustomBasicList<DatabaseMapping> mapList, CustomBasicList<ICondition> conditionList, CustomBasicList<SortInfo>? sortList, StringBuilder thisStr, EnumDatabaseCategory category, int howMany = 0)
        {
            var paramList = new CustomBasicList<DatabaseMapping>();
            thisStr.Append(" Where ");
            CustomBasicList<AndCondition> andList = conditionList.Where(items => items.ConditionCategory == EnumConditionCategory.And).ToCastedList<AndCondition>();
            Dictionary<string, int> thisDict = new Dictionary<string, int>();
            bool needsAppend;
            if (andList.Count > 0)
                needsAppend = true;
            else
                needsAppend = false;
            thisStr.Append(PopulatAnds(andList, mapList, " and ", paramList, thisDict));
            CustomBasicList<OrCondition> orList = conditionList.Where(items => items.ConditionCategory == EnumConditionCategory.Or).ToCastedList<OrCondition>();
            StrCat cats = new StrCat();
            if (orList.Count > 0)
            {
                if (needsAppend == true)
                    thisStr.Append(" and ");
                needsAppend = true;
                thisStr.Append("(");
                orList.ForEach(items => cats.AddToString(PopulatAnds(items.ConditionList, mapList, " or ", paramList, thisDict), ") and ("));
                thisStr.Append(cats.GetInfo());
                thisStr.Append(")");
            }
            CustomBasicList<SpecificListCondition> includeList = conditionList.Where(items => items.ConditionCategory == EnumConditionCategory.ListInclude).ToCastedList<SpecificListCondition>();
            if (includeList.Count > 1)
            {
                CustomBasicList<int> newList = new CustomBasicList<int>();
                includeList.ForEach(items =>
                {
                    newList.AddRange(items.ItemList);
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
                thisStr.Append("a.ID in (");
                thisStr.Append(PopulateListInfo(includeList.Single().ItemList));
            }
            CustomBasicList<NotListCondition> notList = conditionList.Where(items => items.ConditionCategory == EnumConditionCategory.ListNot).ToCastedList<NotListCondition>();
            if (notList.Count == 1)
            {
                if (needsAppend == true)
                    thisStr.Append(" and ");
                thisStr.Append("a.ID not in (");
                thisStr.Append(PopulateListInfo(notList.Single().ItemList));
            }
            if (sortList != null)
                thisStr.Append(GetSortStatement(mapList, sortList, true));
            thisStr.Append(GetLimitSQLite(category, howMany));
            return (thisStr.ToString(), paramList);
        }
        public static (string sqls, CustomBasicList<DatabaseMapping> ParameterMappings) GetConditionalStatement<E, D1>(CustomBasicList<ICondition> conditionList, CustomBasicList<SortInfo>? sortList, bool isOneToOne, EnumDatabaseCategory category, int howMany = 0) where E : class, IJoinedEntity where D1 : class
        {
            StringBuilder thisStr = new StringBuilder();
            StartList<E>(out CustomBasicList<DatabaseMapping> mapList, out CustomBasicList<string> joinList, out string tableName);
            AppendList<E, D1>(mapList, joinList, "b", isOneToOne);
            thisStr.Append(GetSimpleSelectStatement(mapList, joinList, tableName, category, howMany));
            return FinishConditionStatement(mapList, conditionList, sortList, thisStr, category, howMany);
        }
        public static (string sqls, CustomBasicList<DatabaseMapping> ParameterMappings) GetConditionalStatement<E, D1, D2>(CustomBasicList<ICondition> conditionList, CustomBasicList<SortInfo>? sortList, EnumDatabaseCategory category, int howMany = 0, bool isOneToOne = true) where E : class, ISimpleDapperEntity where D1 : class where D2 : class
        {
            StringBuilder thisStr = new StringBuilder();
            StartList<E>(out CustomBasicList<DatabaseMapping> mapList, out CustomBasicList<string> joinList, out string tableName);
            AppendList<E, D1>(mapList, joinList, "b", isOneToOne); //this is always a given.
            bool rets;
            string thisName = typeof(D2).Name;
            rets = HasJoiner<E>(thisName);
            if (rets == true)
            {
                AppendList<E, D2>(mapList, joinList, "c", true); //try this way (?)
            }
            else
            {
                //this means that b is linking to c.
                AppendList<D1, D2>(mapList, joinList, "c", true, "b"); //if that changes, rethink
            }
            thisStr.Append(GetSimpleSelectStatement(mapList, joinList, tableName, category, howMany));
            return FinishConditionStatement(mapList, conditionList, sortList, thisStr, category, howMany);
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
                    cats.AddToString($"{thisMap.Prefix}.{thisMap.DatabaseName} {items.Operator}", seperator); //i am guessing that with parameters no need to worry about the null parts
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
                    if (items.Property != "ID")
                        cats.AddToString($"{items.Code}{thisMap.DatabaseName} {items.Operator} @{thisMap.ObjectName}", seperator);
                    else
                        cats.AddToString($"{thisMap.Prefix}.{thisMap.DatabaseName} {items.Operator} @{thisMap.ObjectName}", seperator);
                }
            });
            return cats.GetInfo();
        }
        #endregion
    }
}