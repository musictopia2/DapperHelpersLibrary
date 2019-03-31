using System;
using System.Collections.Generic;
using System.Text;

namespace DapperHelpersLibrary.Attributes
{
    public class ForeignKeyAttribute : Attribute
    {
        public ForeignKeyAttribute(string ClassName)
        {
            this.ClassName = ClassName;
        }
        public string ClassName { get; private set; }
    }
}