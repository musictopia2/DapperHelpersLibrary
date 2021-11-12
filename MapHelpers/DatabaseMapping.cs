using System.Reflection; //eventually use source generators.
namespace DapperHelpersLibrary.MapHelpers;
public class DatabaseMapping 
{
    public string ObjectName { get; set; }
    public object? Value { get; set; }
    public DatabaseMapping Clone()
    {
        return (DatabaseMapping)MemberwiseClone();
    }
    public string Prefix { get; set; } = "";
    public string DatabaseName { get; private set; }
    public bool HasMatch { get; private set; }
    public string InterfaceName { get; set; } = "";
    public string TableName { get; private set; }
    public bool IsBoolProperty { get; set; }
    public bool CommonForUpdating { get; set; }
    public bool Like { get; set; }
    public PropertyInfo PropertyDetails { get; set; }
    public string GetDataType()
    {
        if (IsBoolProperty == true || PropertyDetails.IsIntOrEnum() == true)
        {
            return "integer";
        }
        return "string";
    }
    public DatabaseMapping(string objectName, string databaseName, string tableName, PropertyInfo thisProp)
    {
        ObjectName = objectName;
        DatabaseName = databaseName;
        TableName = tableName;
        PropertyDetails = thisProp;
        if (databaseName == objectName)
        {
            HasMatch = true;
        }
        else
        {
            HasMatch = false;
        }
    }
}