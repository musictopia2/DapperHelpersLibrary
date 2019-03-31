using System;
using System.Collections.Generic;
using System.Text;

namespace DapperHelpersLibrary.Attributes
{
    /// <summary>
    /// The purpose of this would be to map to an interface.
    /// useful for cases where we are doing the where clause.
    /// there are cases where the interface classes are putting in the conditions
    /// since the interface does not know the real mapping, then this would work.
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true)]

    public class InterfaceAttribute : Attribute
    {
        public InterfaceAttribute(string InterfaceName)
        {
            this.InterfaceName = InterfaceName;
        }

        public string InterfaceName { get; set; }
    }
}
