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
using DapperHelpersLibrary.EntityInterfaces;
using static DapperHelpersLibrary.Extensions.ReflectionDatabase;
using DapperHelpersLibrary.ConditionClasses;
using System.Collections.Generic;
using cs = DapperHelpersLibrary.ConditionClasses.ConditionOperators;
using DapperHelpersLibrary.Extensions;
using static DapperHelpersLibrary.SQLHelpers.SimpleStatementHelpers;
//i think this is the most common things i like to do
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
        //can take a risk.  i may need to test deeper (well see).


        #region No Conditions
        public static string GetSimpleSelectStatement<E, D1, D2>(CustomBasicList<SortInfo> SortList, EnumDatabaseCategory category, int HowMany = 0) where E : class, ISimpleDapperEntity where D1 : class where D2: class
        {
            StartList<E>(out CustomBasicList<DatabaseMapping> ThisList, out CustomBasicList<string> JoinList, out string TableName);
            //hopefully we don't need it with no conditions.  because that part only seems to be put in with conditions.
            //i may have to rethink some things too.
            AppendList<E, D1>(ThisList, JoinList, "b");
            AppendList<E, D2>(ThisList, JoinList, "c");
            string sqls = GetSimpleSelectStatement(ThisList, JoinList, TableName, category, HowMany);
            if (SortList == null)
                return sqls;
            StringBuilder ThisStr = new StringBuilder(sqls);
            ThisStr.Append(GetSortStatement(ThisList, SortList, false));
            ThisStr.Append(GetLimitSQLite(category, HowMany));
            return ThisStr.ToString();
            
        }
        private static void StartList<E>(out CustomBasicList<DatabaseMapping> ThisList, out CustomBasicList<string> JoinList, out string TableName) where E:class
        {
            ThisList = GetMappingList<E>(out TableName);
            JoinList = new CustomBasicList<string>();
            ThisList.ForEach(Items => Items.Prefix = "a");
        }
        private static void AppendList<E, D>(CustomBasicList<DatabaseMapping> ThisList, CustomBasicList<string> JoinList, string Prefix, bool IsOneToOne = true, string Firsts = "a", string NewTable = "") where D:class where E:class
        {
            CustomBasicList<DatabaseMapping> NewList = GetMappingList<D>(out string TableName, IsOneToOne); //unfortunately, it needs them all in that case since queries can be used for it.
            //CustomBasicList<DatabaseMapping> NewList = GetMappingList<D>(out string TableName, true); //try this.  has to rerun the tests.
            NewList.ForEach(Items => Items.Prefix = Prefix);
            ThisList.AddRange(NewList);
            string Foreign;
            string OtherTable;
            string ThisStr;
            if (NewTable != "")
                OtherTable = NewTable;
            else    
                OtherTable = typeof(D).GetTableName();
            if (IsOneToOne == true)
            {
                Foreign = GetJoiner<E, D>();
                ThisStr = $"{OtherTable} {Prefix} on {Firsts}.{Foreign}={Prefix}.ID";
            }
            else
            {
                Foreign = GetJoiner<D, E>();
                ThisStr = $"{OtherTable} {Prefix} on {Firsts}.ID={Prefix}.{Foreign}";
            }
            JoinList.Add(ThisStr);
        }

        public static string GetSimpleSelectStatement<E, D1>(bool IsOneToOne, CustomBasicList<SortInfo> SortList, EnumDatabaseCategory category, int HowMany = 0) where E : class, IJoinedEntity where D1:class
        {
            StartList<E>(out CustomBasicList<DatabaseMapping> ThisList, out CustomBasicList<string> JoinList, out string TableName);
            AppendList<E, D1>(ThisList, JoinList, "b", IsOneToOne);
            string sqls = GetSimpleSelectStatement(ThisList, JoinList, TableName, category, HowMany);
            if (SortList == null)
                return sqls;
            StringBuilder ThisStr = new StringBuilder(sqls);
            ThisStr.Append(GetSortStatement(ThisList, SortList, true));
            ThisStr.Append(GetLimitSQLite(category, HowMany));
            return ThisStr.ToString();
            //return GetSimpleSelectStatement(ThisList, JoinList, TableName);
        }

        //has to get some hints.

        //public static string GetSimpleSelectStatement(CustomBasicList<DatabaseMapping> ThisList, string TableName, EnumDatabaseCategory Database, EnumSQLCategory Category = EnumSQLCategory.Normal, int HowMany = 0, string Property = "")
        //{
        //    StringBuilder ThisStr = new StringBuilder("select ");
        //    if (HowMany > 0 && Database == EnumDatabaseCategory.SQLServer) //sqlite requires it at the end.
        //        ThisStr.Append($"top {HowMany} ");
        //    if (Category == EnumSQLCategory.Normal && Property == "")
        //    {
        //        if (ThisList.TrueForAll(Items => Items.HasMatch == true))
        //        {
        //            ThisStr.Append(" * from ");
        //            ThisStr.Append(TableName);
        //            return ThisStr.ToString();
        //        }
        //        StrCat cats = new StrCat();
        //        ThisList.ForEach(Items =>
        //        {
        //            if (Items.HasMatch == false)
        //                cats.AddToString($"{Items.DatabaseName} as {Items.ObjectName}", ", ");
        //            else
        //                cats.AddToString(Items.DatabaseName, ", ");
        //        });
        //        ThisStr.Append(cats.GetInfo());
        //    }
        //    else if (Category == EnumSQLCategory.Normal)
        //    {
        //        DatabaseMapping ThisMap = ThisList.Where(Items => Items.ObjectName == Property).Single();
        //        if (ThisMap.HasMatch == false)
        //            ThisStr.Append($"{ThisMap.DatabaseName} as {ThisMap.ObjectName} ");
        //        else
        //            ThisStr.Append($"{ThisMap.DatabaseName} ");
        //    }
        //    else if (Category == EnumSQLCategory.Count)
        //        ThisStr.Append("count (*)");
        //    else if (Category == EnumSQLCategory.Bool)
        //        ThisStr.Append("1");
        //    else if (Category == EnumSQLCategory.Delete)
        //        throw new BasicBlankException("Deleting is not supposed to get a select statement.  Try delete statement instead");
        //    else
        //        throw new BasicBlankException("Not supported");
        //    ThisStr.Append(" from ");
        //    ThisStr.Append(TableName);
        //    return ThisStr.ToString();
        //}



        private static string GetSimpleSelectStatement(CustomBasicList<DatabaseMapping> ThisList, CustomBasicList<string> JoinList, string TableName, EnumDatabaseCategory Database, int HowMany = 0) //sqlite requires limit at the end
        {
            if (JoinList.Count == 0)
                throw new BasicBlankException("Needs at least one other table.  Otherwise, no join");
            StringBuilder ThisStr = new StringBuilder("select ");
            StrCat cats = new StrCat();
            if (HowMany > 0 && Database == EnumDatabaseCategory.SQLServer) //sqlite requires it at the end.
                ThisStr.Append($"top {HowMany} ");
            ThisList.ForEach(Items =>
            {
                if (Items.HasMatch == false)
                    cats.AddToString($"{Items.Prefix}.{Items.DatabaseName} as {Items.ObjectName}", ", ");
                else
                    cats.AddToString($"{Items.Prefix}.{Items.DatabaseName}", ", ");
            });
            ThisStr.Append(cats.GetInfo());
            ThisStr.Append(" from ");
            ThisStr.Append(TableName);
            ThisStr.Append(" a left join ");
            cats = new StrCat();
            JoinList.ForEach(Items => cats.AddToString(Items, " left join "));
            ThisStr.Append(cats.GetInfo());
            return ThisStr.ToString();
        }
        #endregion

        #region With Conditions
        //i am guessing that more gets added when it has conditions. so should be no need for repeating.

        //hard to figure out how to do it without repeating myself because there are a few differences.
        //we have 2 and 3.  this means for this part, has to see if i can rethink.

        private static (string sqls, CustomBasicList<DatabaseMapping> ParameterMappings) FinishConditionStatement (CustomBasicList<DatabaseMapping> MapList, CustomBasicList<ICondition> ConditionList, CustomBasicList<SortInfo> SortList, StringBuilder ThisStr, EnumDatabaseCategory category, int HowMany = 0)
        {

            var ParamList = new CustomBasicList<DatabaseMapping>();
            ThisStr.Append(" Where ");
            CustomBasicList<AndCondition> AndList = ConditionList.Where(Items => Items.ConditionCategory == EnumConditionCategory.And).ToCastedList<AndCondition>();
            Dictionary<string, int> ThisDict = new Dictionary<string, int>();
            //for now, has to see what i expect and do unit tests on it.
            //for now, use all.  can change later if necessary.
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
                OrList.ForEach(Items => cats.AddToString(PopulatAnds(Items.ConditionList, MapList, " or ", ParamList, ThisDict), ") and ("));
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
            if (IncludeList.Count == 1)
            {
                if (NeedsAppend == true)
                    ThisStr.Append(" and ");
                NeedsAppend = true;
                ThisStr.Append("a.ID in (");
                ThisStr.Append(PopulateListInfo(IncludeList.Single().ItemList));
                //cats = new StrCat();
                //IncludeList.Single().ItemList.ForEach(Items => cats.AddToString(Items.ToString(), ", "));
                //ThisStr.Append(cats.GetInfo());
                //ThisStr.Append(")");
            }
            CustomBasicList<NotListCondition> NotList = ConditionList.Where(Items => Items.ConditionCategory == EnumConditionCategory.ListNot).ToCastedList<NotListCondition>();
            
            if (NotList.Count == 1)
            {
                if (NeedsAppend == true)
                    ThisStr.Append(" and ");
                ThisStr.Append("a.ID not in (");
                ThisStr.Append(PopulateListInfo(NotList.Single().ItemList));
            }
            if (SortList != null)
                ThisStr.Append(GetSortStatement(MapList, SortList, true));
            ThisStr.Append(GetLimitSQLite(category, HowMany));
            return (ThisStr.ToString(), ParamList);

        }

        public static (string sqls, CustomBasicList<DatabaseMapping> ParameterMappings) GetConditionalStatement<E, D1>(CustomBasicList<ICondition> ConditionList, CustomBasicList<SortInfo> SortList, bool IsOneToOne, EnumDatabaseCategory category, int HowMany = 0) where E : class, IJoinedEntity where D1 : class
        {
            StringBuilder ThisStr = new StringBuilder();
            //ThisStr.Append(GetSimpleSelectStatement(MapList, TableName));

            StartList<E>(out CustomBasicList<DatabaseMapping> MapList, out CustomBasicList<string> JoinList, out string TableName);
            AppendList<E, D1>(MapList, JoinList, "b", IsOneToOne);
            ThisStr.Append(GetSimpleSelectStatement(MapList, JoinList, TableName, category, HowMany));
            return FinishConditionStatement(MapList, ConditionList, SortList, ThisStr, category, HowMany);
        }

        public static (string sqls, CustomBasicList<DatabaseMapping> ParameterMappings) GetConditionalStatement<E, D1, D2>(CustomBasicList<ICondition> ConditionList, CustomBasicList<SortInfo> SortList, EnumDatabaseCategory category, int HowMany = 0, bool IsOneToOne = true) where E : class, ISimpleDapperEntity where D1 : class where D2: class
        {
            StringBuilder ThisStr = new StringBuilder();
            //ThisStr.Append(GetSimpleSelectStatement(MapList, TableName));

            StartList<E>(out CustomBasicList<DatabaseMapping> MapList, out CustomBasicList<string> JoinList, out string TableName);
            //string OtherTable = GetTableName<D2>();
            AppendList<E, D1>(MapList, JoinList, "b", IsOneToOne); //this is always a given.

            bool rets;
            string ThisName = typeof(D2).Name;
            rets = HasJoiner<E>(ThisName); 

            //here needs rethinking.
            
            


            if (rets == true)
            
                AppendList<E, D2>(MapList, JoinList, "c", true); //try this way (?)
            //AppendList<E, D2>(MapList, JoinList, "c", IsOneToOne); //not sure.
            else
            {
                //this means that b is linking to c.
                AppendList<D1, D2>(MapList, JoinList, "c", true, "b"); //if that changes, rethink
            }
            


            ThisStr.Append(GetSimpleSelectStatement(MapList, JoinList, TableName, category, HowMany));
            return FinishConditionStatement(MapList, ConditionList, SortList, ThisStr, category, HowMany);
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
                    cats.AddToString($"{ThisMap.Prefix}.{ThisMap.DatabaseName} {Items.Operator}", Seperator); //i am guessing that with parameters no need to worry about the null parts
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
                    if (Items.Property!= "ID")
                        cats.AddToString($"{ThisMap.DatabaseName} {Items.Operator} @{ThisMap.ObjectName}", Seperator);
                    else
                        cats.AddToString($"{ThisMap.Prefix}.{ThisMap.DatabaseName} {Items.Operator} @{ThisMap.ObjectName}", Seperator);
                    //if (Items.Operator == cs.Like)
                    //    cats.AddToString($"{ThisMap.DatabaseName} {Items.Operator} @%{ThisMap.ObjectName}%", Seperator);
                    //else

                    //cats.AddToString($"{ThisMap.Prefix}.{ThisMap.DatabaseName} {Items.Operator} @{ThisMap.ObjectName}", Seperator);

                    //cats.AddToString($"{ThisMap.DatabaseName} {Items.Operator} @{ThisMap.DatabaseName}", Seperator);


                }
                //for now, has to be this way so i can test the statements.  later will do parameters
                //
            });
            return cats.GetInfo();
        }

        #endregion

    }
}
