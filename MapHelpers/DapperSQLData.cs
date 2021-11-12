namespace DapperHelpersLibrary.MapHelpers;
internal class DapperSQLData
{
    public string SQLStatement { get; set; } = "";
    public DynamicParameters Parameters { get; set; } = new DynamicParameters(); //i think should be done here. worked before so should work again.
}