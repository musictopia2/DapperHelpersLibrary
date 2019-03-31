using System;
using System.Text;
using CommonBasicStandardLibraries.Exceptions;
using CommonBasicStandardLibraries.AdvancedGeneralFunctionsAndProcesses.BasicExtensions;
using System.Linq;
using CommonBasicStandardLibraries.BasicDataSettingsAndProcesses;
using static CommonBasicStandardLibraries.BasicDataSettingsAndProcesses.BasicDataFunctions;
using CommonBasicStandardLibraries.CollectionClasses;
using System.Threading.Tasks; //most of the time, i will be using asyncs.
using fs = CommonBasicStandardLibraries.AdvancedGeneralFunctionsAndProcesses.JsonSerializers.FileHelpers;
using js = CommonBasicStandardLibraries.AdvancedGeneralFunctionsAndProcesses.JsonSerializers.NewtonJsonStrings; //just in case i need those 2.
//i think this is the most common things i like to do
namespace DapperHelpersLibrary.ConditionClasses
{
    public class ValidationConditions : IValidateConditions
    {
        //this is default implementation.  anybody else can have a different way to validate if necessary
        public bool IsValid(CustomBasicList<ICondition> ConditionList, out string Message)
        {
            //should be only one of 2 kinds of lists.
            if (ConditionList.Count(Items => Items.ConditionCategory == EnumConditionCategory.ListInclude) > 1)
            {
                Message = "The Include Condition List Contains More Than One List";
                return false;
            }
            if (ConditionList.Count(Items => Items.ConditionCategory == EnumConditionCategory.ListNot) > 1)
            {
                Message = "The Exclude Condition List Contains More Than One List";
                return false;
            }
            Message = ""; //since i am forced to pass back the message
            return true;
        }
    }
}