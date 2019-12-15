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
using CommonBasicStandardLibraries.DatabaseHelpers.MiscClasses;
using CommonBasicStandardLibraries.DatabaseHelpers.MiscInterfaces;
using CommonBasicStandardLibraries.AdvancedGeneralFunctionsAndProcesses.ConfigProcesses;
//i think this is the most common things i like to do
namespace DapperHelpersLibrary
{
    /// <summary>
    /// this is a good class to inherit from if you need the same class to support either sql server or sqlite.
    /// if no need to support both sqlite and sql server, then probably no need to use this class but the other old fashioned class.
    /// </summary>
    public abstract class BasicMultiDatabaseClass
    {
        /// <summary>
        /// this is intended to be async void.  its useful so you can do async on new.  otherwise, can't do async on new.
        /// </summary>
        //protected abstract void SetUp();
        public BasicMultiDatabaseClass(ISimpleConfig config)
        {
            Helps = new ConnectionHelper(config, CalculateKey, GetDatabaseCategoryAsync);
            Connector = Helps.GetConnector;
        }
        //since i already have the helps, just use that one.
        //protected EnumDatabaseCategory DatabaseCategory => Helps!.Category; //so i have the option to make decisions based on database used.
        protected IDbConnector Connector { get; } //not sure (?)
        //protected abstract string Key { get; }
        protected abstract Task<EnumDatabaseCategory> GetDatabaseCategoryAsync();
        protected ConnectionHelper? Helps;
        protected abstract string CalculateKey(EnumDatabaseCategory category);
    }
}
