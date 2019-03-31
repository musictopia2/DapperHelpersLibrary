using System;
using System.Collections.Generic;
using System.Text;

namespace DapperHelpersLibrary.ConditionClasses
{
    public class NotListCondition : BaseListCondition, ICondition
    {
        EnumConditionCategory ICondition.ConditionCategory => EnumConditionCategory.ListNot ;
    }
}