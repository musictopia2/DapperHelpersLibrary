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
using System.Reflection;
//i think this is the most common things i like to do
namespace DapperHelpersLibrary.MapHelpers
{
    public class DatabaseMapping //will risk making this internal since i should not need it public for this
    {
        //since i need to test the statement, i have to put as public for now.

        public string ObjectName { get; set; } //its possible i have to change it.  its used for dynamic parameters for conditions.

        public object Value { get; set; }
        //since passing value worked, may be no need to even pass in the property.

        //public PropertyInfo ThisProperty { get; set; } //well see.

        public DatabaseMapping Clone()
        {
            return (DatabaseMapping) MemberwiseClone();
        }
        public string Prefix { get; set; } //sometimes a prefix is needed (especially for joined tables).
        public string DatabaseName { get; private set; }
        public bool HasMatch { get; private set; }
        public string InterfaceName { get; set; }
        public string TableName { get; private set; } //try this way.
        public bool IsBoolProperty { get; set; } //needs to know if its boolean. because even creating table needs to know.
        public bool CommonForUpdating { get; set; }
        public bool Like { get; set; } //if you use the statement like, then needs to know so something else can happen when it comes to the parameters.  hopefully it works
        public PropertyInfo PropertyDetails { get; set; }
        public string GetDataType()
        {
            //there are other times i have to make it integer.

            if (IsBoolProperty == true || PropertyDetails.IsInt() == true)
                return "integer";
            return "string";
        }
        public DatabaseMapping(string ObjectName, string DatabaseName, string TableName, PropertyInfo ThisProp)
        {
            this.ObjectName = ObjectName;
            this.DatabaseName = DatabaseName;
            this.TableName = TableName;
            this.PropertyDetails = ThisProp;
            if (DatabaseName == ObjectName)
                HasMatch = true;
            else
                HasMatch = false;
        }
    }
}
