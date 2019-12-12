using CommonBasicStandardLibraries.AdvancedGeneralFunctionsAndProcesses.BasicExtensions;
using System.Reflection;
namespace DapperHelpersLibrary.MapHelpers
{
    public class DatabaseMapping //will risk making this internal since i should not need it public for this
    {
        //since i need to test the statement, i have to put as public for now.

        public string ObjectName { get; set; } //its possible i have to change it.  its used for dynamic parameters for conditions.
        public object? Value { get; set; }
        public DatabaseMapping Clone()
        {
            return (DatabaseMapping)MemberwiseClone();
        }
        public string Prefix { get; set; } = ""; //sometimes a prefix is needed (especially for joined tables).  hopefully setting to blank to begin will not cause breaking change (?)
        public string DatabaseName { get; private set; }
        public bool HasMatch { get; private set; }
        public string InterfaceName { get; set; } = "";
        public string TableName { get; private set; } //try this way.
        public bool IsBoolProperty { get; set; } //needs to know if its boolean. because even creating table needs to know.
        public bool CommonForUpdating { get; set; }
        public bool Like { get; set; } //if you use the statement like, then needs to know so something else can happen when it comes to the parameters.  hopefully it works
        public PropertyInfo PropertyDetails { get; set; }
        public string GetDataType()
        {
            if (IsBoolProperty == true || PropertyDetails.IsInt() == true)
                return "integer";
            return "string";
        }
        public DatabaseMapping(string objectName, string databaseName, string tableName, PropertyInfo thisProp)
        {
            ObjectName = objectName;
            DatabaseName = databaseName;
            TableName = tableName;
            PropertyDetails = thisProp;
            if (databaseName == objectName)
                HasMatch = true;
            else
                HasMatch = false;
        }
    }
}