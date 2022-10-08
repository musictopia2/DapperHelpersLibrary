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
        //InitAsync();   
    }
    //you have to do this later unfortunately.  otherwise, has major problems.
    public async Task InitAsync()
    {
        if (Helps is not null)
        {
            return;
        }
        var cat = await GetDatabaseCategoryAsync();
        Helps = new(cat, CalculateKey(cat), Config);
        //Helps = new ConnectionHelper(Config, CalculateKey, cat);
        Connector = Helps.GetConnector;
    }
    protected IDbConnector? Connector { get; private set; } //not sure (?)
    protected abstract Task<EnumDatabaseCategory> GetDatabaseCategoryAsync();
    protected ConnectionHelper? Helps;
    protected abstract string CalculateKey(EnumDatabaseCategory category);
}