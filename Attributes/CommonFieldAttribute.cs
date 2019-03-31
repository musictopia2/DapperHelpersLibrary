using System;
using System.Collections.Generic;
using System.Text;

namespace DapperHelpersLibrary.Attributes
{
    /// <summary>
    /// this is used in cases where you only want to retrive the common fields.  
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class CommonFieldAttribute : Attribute
    {
    }
}
