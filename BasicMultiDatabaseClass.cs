using CommonBasicStandardLibraries.AdvancedGeneralFunctionsAndProcesses.ConfigProcesses;
using CommonBasicStandardLibraries.DatabaseHelpers.MiscClasses;
using CommonBasicStandardLibraries.DatabaseHelpers.MiscInterfaces;
using System.Threading.Tasks; //most of the time, i will be using asyncs.
namespace DapperHelpersLibrary
{
    /// <summary>
    /// this is a good class to inherit from if you need the same class to support either sql server or sqlite.
    /// if no need to support both sqlite and sql server, then probably no need to use this class but the other old fashioned class.
    /// </summary>
    public abstract class BasicMultiDatabaseClass
    {

        protected ISimpleConfig Config; //has to be here.  otherwise timing and by the time the base one is called, too late.

        public BasicMultiDatabaseClass(ISimpleConfig config)
        {
            Config = config;
            Helps = new ConnectionHelper(config, CalculateKey, GetDatabaseCategoryAsync);
            Connector = Helps.GetConnector;
        }
        protected IDbConnector Connector { get; } //not sure (?)
        protected abstract Task<EnumDatabaseCategory> GetDatabaseCategoryAsync();
        protected ConnectionHelper? Helps;
        protected abstract string CalculateKey(EnumDatabaseCategory category);
    }
}
