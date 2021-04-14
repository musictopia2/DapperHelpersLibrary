using CommonBasicLibraries.AdvancedGeneralFunctionsAndProcesses.Misc;
using CommonBasicLibraries.BasicDataSettingsAndProcesses;
using CommonBasicLibraries.DatabaseHelpers.MiscClasses;
using CommonBasicLibraries.DatabaseHelpers.MiscInterfaces;
using Dapper;
using System.Data;
using System.Text;
using static CommonBasicLibraries.DatabaseHelpers.Extensions.ReflectionDatabase;
using static DapperHelpersLibrary.MapHelpers.MapBaseHelperClass;
namespace DapperHelpersLibrary.Extensions
{
    //this is for cases where we don't know what category to put under but are still basic extensions  an example is creating sqlite databases.
    public static class Misc
    {
        internal static EnumDatabaseCategory GetDatabaseCategory(this IDbConnection db, IDbConnector conn) //maybe need this (?)
        {
            return conn.GetCategory(db);
        }
        public static void CreateTableSQLite<E>(this IDbConnection db, IDbConnector conn) where E : class
        {
            EnumDatabaseCategory dcat = conn.GetCategory(db);
            if (dcat != EnumDatabaseCategory.SQLite)
            {
                throw new CustomBasicException("Currently, Only SQLite can create tables since the variable types will be all strings");
            }
            var thisList = GetMappingList<E>(out string TableName);
            //return "create table TestSong (ID integer primary key autoincrement, Name string, Value integer)";
            if (thisList.Exists(xx => xx.DatabaseName.ToUpper() == "ID") == false)
            {
                throw new CustomBasicException("You must have ID in order to create table  Its needed for the primary key part");
            }
            string sqls;
            StrCat cats = new ();
            StringBuilder thisStr = new ("create table ");
            thisStr.Append(TableName);
            bool autoIncrementID = IsAutoIncremented<E>();
            if (autoIncrementID == true)
            {
                thisStr.Append(" (ID integer primary key autoincrement, ");
            }
            else
            {
                thisStr.Append(" (ID integer primary key, ");
            }
            thisList.RemoveAllOnly(xx => xx.DatabaseName.ToLower() == "id"); //its okay to remove id because its already handled anyways.
            thisList.ForEach(xx => cats.AddToString($"{xx.DatabaseName} {xx.GetDataType()}", ", "));
            thisStr.Append(cats.GetInfo());
            thisStr.Append(')');
            sqls = thisStr.ToString();
            db.Execute(sqls);
        }
    }
}