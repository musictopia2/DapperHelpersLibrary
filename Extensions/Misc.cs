using CommonBasicStandardLibraries.AdvancedGeneralFunctionsAndProcesses.Misc;
using CommonBasicStandardLibraries.DatabaseHelpers.MiscClasses;
using CommonBasicStandardLibraries.DatabaseHelpers.MiscInterfaces;
using CommonBasicStandardLibraries.Exceptions;
using Dapper;
using System.Data;
using System.Text;
using static CommonBasicStandardLibraries.DatabaseHelpers.Extensions.ReflectionDatabase;
using static DapperHelpersLibrary.MapHelpers.MapBaseHelperClass;
namespace DapperHelpersLibrary.Extensions
{
    //this is for cases where we don't know what category to put under but are still basic extensions  an example is creating sqlite databases.
    public static class Misc
    {
        //internal static EnumDatabaseCategory GetDatabaseCategory(this IDbConnection db) //maybe need this (?)
        //{
        //    IDbConnector conn = cons!.Resolve<IDbConnector>();
        //    return conn.GetCategory(db); //this way i can support either via unit testing or not.
        //    //if (db is SqlConnection)
        //    //    return EnumDatabaseCategory.SQLServer;
        //    //else if (db is SQLiteConnection)
        //    //    return EnumDatabaseCategory.SQLite;
        //    //throw new BasicBlankException("Only SQL Server And SQLite are supported currently");
        //}
        internal static EnumDatabaseCategory GetDatabaseCategory(this IDbConnection db, IDbConnector conn) //maybe need this (?)
        {
            return conn.GetCategory(db);
            //if (db is SqlConnection)
            //    return EnumDatabaseCategory.SQLServer;
            //else if (db is SQLiteConnection)
            //    return EnumDatabaseCategory.SQLite;
            //throw new BasicBlankException("Only SQL Server And SQLite are supported currently");
        }
        public static void CreateTableSQLite<E>(this IDbConnection db, IDbConnector conn) where E : class
        {
            EnumDatabaseCategory dcat = conn.GetCategory(db);
            if (dcat != EnumDatabaseCategory.SQLite)
                throw new BasicBlankException("Currently, Only SQLite can create tables since the variable types will be all strings");
            var thisList = GetMappingList<E>(out string TableName);
            //return "create table TestSong (ID integer primary key autoincrement, Name string, Value integer)";
            if (thisList.Exists(Items => Items.DatabaseName.ToUpper() == "ID") == false)
                throw new BasicBlankException("You must have ID in order to create table  Its needed for the primary key part");
            string sqls;
            StrCat cats = new StrCat();
            StringBuilder thisStr = new StringBuilder("create table ");
            thisStr.Append(TableName);
            bool autoIncrementID = IsAutoIncremented<E>();
            if (autoIncrementID == true)
                thisStr.Append(" (ID integer primary key autoincrement, ");
            else
                thisStr.Append(" (ID integer primary key, ");
            thisList.RemoveAllOnly(Items => Items.DatabaseName.ToLower() == "id"); //its okay to remove id because its already handled anyways.
            thisList.ForEach(Items => cats.AddToString($"{Items.DatabaseName} {Items.GetDataType()}", ", "));
            thisStr.Append(cats.GetInfo());
            thisStr.Append(")");
            sqls = thisStr.ToString();
            db.Execute(sqls);
        }
    }
}