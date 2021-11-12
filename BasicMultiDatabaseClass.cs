namespace DapperHelpersLibrary;
/// <summary>
/// this is a good class to inherit from if you need the same class to support either sql server or sqlite.
/// if no need to support both sqlite and sql server, then probably no need to use this class but the other old fashioned class.
/// </summary>
public abstract class BasicMultiDatabaseClass
{
    protected ISimpleConfig Config;
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