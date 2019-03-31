using System;
using System.Collections.Generic;
using System.Text;

namespace DapperHelpersLibrary.Attributes
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    public class ColumnAttribute : Attribute
    {
        public ColumnAttribute(string ColumnName)
        {
            //TableName = TSQLTableName;
            this.ColumnName = ColumnName;
        }
        public string ColumnName { get; private set; }

    }
}
