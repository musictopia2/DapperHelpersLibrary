using System;
using System.Collections.Generic;
using System.Text;
using DapperHelpersLibrary.MapHelpers;
namespace DapperHelpersLibrary.ConditionClasses
{
    public class AndCondition : ICondition, IProperty
    {
        public string Property { get; set; }
        public string Operator { get; set; } = "="; //decided to do it this way so a person has a choice.  if they use the strongly typed, should work.  if not, will be runtime error.
        public object Value { get; set; }
        EnumConditionCategory ICondition.ConditionCategory { get => EnumConditionCategory.And; }
    }
}