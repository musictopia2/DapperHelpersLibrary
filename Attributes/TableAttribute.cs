using System;
using System.Collections.Generic;
using System.Text;

namespace DapperHelpersLibrary.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class TableAttribute : Attribute
    {
        public TableAttribute(string TableName)
        {
            this.TableName = TableName;
        }
        public string TableName { get; private set; }
    }
}
