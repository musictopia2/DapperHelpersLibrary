using System;
using System.Collections.Generic;
using System.Text;
using DapperHelpersLibrary.MapHelpers;

namespace DapperHelpersLibrary.SQLHelpers
{
    public class UpdateFieldInfo : IProperty
    {
        public string Property { get; set; }
        //try to not even worry about the field.
        //public object Value { get; set; }
        public UpdateFieldInfo()
        {

        }
        public UpdateFieldInfo(string Property)
        {
            this.Property = Property;
        }
    }

    public class UpdateEntity : UpdateFieldInfo
    {
        public object Value { get; set; } //this time i need the value.  no way around it.  has to update one a time when its doing it this way.

        public UpdateEntity(string Property, object Value)
        {
            this.Property = Property;
            this.Value = Value;
        }

    }
}