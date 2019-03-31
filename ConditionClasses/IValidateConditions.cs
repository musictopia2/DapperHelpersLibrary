using System;
using System.Collections.Generic;
using System.Text;
using CommonBasicStandardLibraries.CollectionClasses;
namespace DapperHelpersLibrary.ConditionClasses
{
    public interface IValidateConditions
    {
        bool IsValid(CustomBasicList<ICondition> ConditionList, out string Message);
    }
}