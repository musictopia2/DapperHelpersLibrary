namespace DapperHelpersLibrary.ConnectionHelpers;
internal interface IConnector
{
    bool IsTesting { get; }
    IDbConnector GetConnector { get; internal set; }
    EnumDatabaseCategory Category { get; internal set; } //had to be public so the document database library can access it.
    string ConnectionString { get; internal set; }
}