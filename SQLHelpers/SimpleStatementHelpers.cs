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


//i think this is the most common things i like to do
namespace DapperHelpersLibrary.SQLHelpers
{
    //this is for simple parts.  will probably have a more complex one to focus on complex statements
    //one part for the statements and one part for the dynamic parameters (probably another namespace)

    internal class SimpleStatementHelpers
    {
        public enum EnumDatabaseCategory
        {
            None,
            SQLServer,
            SQLite
        }
        public enum EnumSQLCategory
        {
            Normal,
            Count,
            Bool,
            Delete
        }
        public static string GetInsertStatement(EnumDatabaseCategory Category, CustomBasicList<DatabaseMapping> ThisList, string TableName, bool IsAutoIncremented)
        {
            if (Category == EnumDatabaseCategory.None)
                throw new BasicBlankException("Must choose what database to use");
            StringBuilder ThisStr = new StringBuilder("insert into ");
            ThisStr.Append(TableName);
            ThisStr.Append(" (");
            StrCat cat1 = new StrCat();
            StrCat cat2 = new StrCat();
            ThisList.ForEach(Items =>
            {
                cat1.AddToString(Items.DatabaseName, ", ");
                cat2.AddToString($"@{Items.DatabaseName}", ", ");
            });
            ThisStr.Append(cat1.GetInfo());
            ThisStr.Append(") values (");
            ThisStr.Append(cat2.GetInfo());
            ThisStr.Append(")");
            if (IsAutoIncremented == true)
            {
                if (Category == EnumDatabaseCategory.SQLite)
                    ThisStr.Append("; SELECT last_insert_rowid()"); //hopefully this works with sql server. if not, rethink
                else if (Category == EnumDatabaseCategory.SQLServer)
                    ThisStr.Append("; SELECT CAST(SCOPE_IDENTITY()  AS BIGINT) AS [id]");
                else
                    throw new BasicBlankException("Not Supported");
            }
            //here is the sql server statement
            //SELECT CAST(SCOPE_IDENTITY()  AS BIGINT) AS [id]
            //i can't just set the variable because if 2 databases are being used, will cause problems.
            //its possible to have processes that use both sql server and sqlite.
            return ThisStr.ToString();
        }
        public static string GetLimitSQLite(EnumDatabaseCategory Database, int HowMany)
        {
            if (Database == EnumDatabaseCategory.SQLServer)
                return "";
            if (HowMany <= 0)
                return "";
            return $"Limit {HowMany}";
        }

        public static string GetSortStatement(CustomBasicList<DatabaseMapping> MapList, CustomBasicList<SortInfo> SortList, bool IsJoined)
        {
            if (SortList == null)
                return ""; //nothing
            if (SortList.Count == 0)
                throw new BasicBlankException("If you are not sending nothing. you must have at least one condition");
            StringBuilder ThisStr = new StringBuilder();
            ThisStr.Append(" order by ");
            string Extras;
            StrCat cats = new StrCat();
            SortList.ForEach(Items =>
            {
                DatabaseMapping ThisMap = FindMappingForProperty(Items, MapList);
                if (Items.OrderBy == SortInfo.EnumOrderBy.Descending)
                    Extras = " desc";
                else
                    Extras = "";
                if (IsJoined == false)
                    cats.AddToString($"{ThisMap.DatabaseName}{Extras}", ", ");
                else
                    cats.AddToString($"{ThisMap.Prefix}.{ThisMap.DatabaseName}{Extras}", ", ");
            });
            ThisStr.Append(cats.GetInfo());
            return ThisStr.ToString();
        }

        public static string GetDeleteStatement(string TableName)
        {
            StringBuilder ThisStr = new StringBuilder("delete from ");
            ThisStr.Append(TableName);
            return ThisStr.ToString();
        }
        public static (string sqls, CustomBasicList<DatabaseMapping> MapList) GetSimpleSelectStatement<E>(EnumDatabaseCategory Database, int HowMany = 0) where E : class
        {
            
            CustomBasicList<DatabaseMapping> ThisList = GetMappingList<E>(out string TableName);
            string ThisStr = GetSimpleSelectStatement(ThisList, TableName, Database, HowMany: HowMany);
            return (ThisStr, ThisList);
            //return GetSimpleSelectStatement(ThisList, TableName);
            
        }
        public static string GetSimpleSelectStatement(CustomBasicList<DatabaseMapping> ThisList, string TableName, EnumDatabaseCategory Database, EnumSQLCategory Category = EnumSQLCategory.Normal, int HowMany = 0, string Property = "")
        {
            StringBuilder ThisStr = new StringBuilder("select ");
            if (HowMany > 0 && Database == EnumDatabaseCategory.SQLServer) //sqlite requires it at the end.
                ThisStr.Append($"top {HowMany} ");
            if (Category == EnumSQLCategory.Normal && Property == "")
            {
                if (ThisList.TrueForAll(Items => Items.HasMatch == true))
                {
                    ThisStr.Append(" * from ");
                    ThisStr.Append(TableName);
                    return ThisStr.ToString();
                }
                StrCat cats = new StrCat();
                ThisList.ForEach(Items =>
                {
                    if (Items.HasMatch == false)
                        cats.AddToString($"{Items.DatabaseName} as {Items.ObjectName}", ", ");
                    else
                        cats.AddToString(Items.DatabaseName, ", ");
                });
                ThisStr.Append(cats.GetInfo());
            }
            else if (Category == EnumSQLCategory.Normal)
            {
                DatabaseMapping ThisMap = ThisList.Where(Items => Items.ObjectName == Property).Single();
                if (ThisMap.HasMatch == false)
                    ThisStr.Append($"{ThisMap.DatabaseName} as {ThisMap.ObjectName} ");
                else
                    ThisStr.Append($"{ThisMap.DatabaseName} ");
            }
            else if (Category == EnumSQLCategory.Count)
                ThisStr.Append("count (*)");
            else if (Category == EnumSQLCategory.Bool)
                ThisStr.Append("1");
            else if (Category == EnumSQLCategory.Delete)
                throw new BasicBlankException("Deleting is not supposed to get a select statement.  Try delete statement instead");
            else
                throw new BasicBlankException("Not supported");
            ThisStr.Append(" from ");
            ThisStr.Append(TableName);
            return ThisStr.ToString();
        }

    }
}
