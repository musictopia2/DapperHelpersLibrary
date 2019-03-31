using System;
using System.Collections.Generic;
using System.Text;
using DapperHelpersLibrary.MapHelpers;
namespace DapperHelpersLibrary.SQLHelpers
{
    public class SortInfo : IProperty
    {
        public enum EnumOrderBy
        {
            Ascending,Descending
        }
        public string Property { get; set; }
        public EnumOrderBy OrderBy { get; set; } = EnumOrderBy.Ascending;
        //this time just a list of sortinfo.  no interfaces needed this time.
    }
}