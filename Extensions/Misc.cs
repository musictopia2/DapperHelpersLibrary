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
//i think this is the most common things i like to do
namespace DapperHelpersLibrary.Extensions
{
    //this is for cases where we don't know what category to put under but are still basic extensions  an example is creating sqlite databases.
    public static class Misc
    {

        internal static EnumDatabaseCategory GetDatabaseCategory(this IDbConnection db)
        {
            if (db is SqlConnection)
                return EnumDatabaseCategory.SQLServer;
            else if (db is SQLiteConnection)
                return EnumDatabaseCategory.SQLite;
            throw new BasicBlankException("Only SQL Server And SQLite are supported currently");
        }

        public static void CreateTableSQLite<E>(this IDbConnection db) where E : class
        {
            //this should not use any transactions.  should only be done for testing.
            if (db is SQLiteConnection == false)
                throw new BasicBlankException("Currently, Only SQLite can create tables since the variable types will be all strings");

            var ThisList = GetMappingList<E>(out string TableName);
            //return "create table TestSong (ID integer primary key autoincrement, Name string, Value integer)";
            if (ThisList.Exists(Items => Items.DatabaseName.ToUpper() == "ID") == false)
                throw new BasicBlankException("You must have ID in order to create table  Its needed for the primary key part");
            string sqls;
            StrCat cats = new StrCat();
            StringBuilder ThisStr = new StringBuilder("create table ");
            ThisStr.Append(TableName);
            bool AutoIncrementID = IsAutoIncremented<E>();
            if (AutoIncrementID == true)
                ThisStr.Append(" (ID integer primary key autoincrement, ");
            else
                ThisStr.Append(" (ID integer primary key, ");
            ThisList.RemoveAllOnly(Items => Items.DatabaseName.ToLower() == "id"); //its okay to remove id because its already handled anyways.
            ThisList.ForEach(Items => cats.AddToString($"{Items.DatabaseName} {Items.GetDataType()}", ", "));
            ThisStr.Append(cats.GetInfo());
            ThisStr.Append(")");
            sqls = ThisStr.ToString();
            db.Execute(sqls);
        }

        


    }
}
