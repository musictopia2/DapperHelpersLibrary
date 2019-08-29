/// <summary>
/// 
/// </summary>
namespace DapperHelpersLibrary.Attributes
{
    using System;
    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    public class ColumnAttribute : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ColumnName"></param>
        public ColumnAttribute(string ColumnName)
        {
            //TableName = TSQLTableName;
            this.ColumnName = ColumnName;
        }
        /// <summary>
        /// 
        /// </summary>
        public string ColumnName { get; private set; }

    }
}
