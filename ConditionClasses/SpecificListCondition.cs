//i think this is the most common things i like to do
namespace DapperHelpersLibrary.ConditionClasses
{
    public class SpecificListCondition : BaseListCondition, ICondition
    {
        EnumConditionCategory ICondition.ConditionCategory => EnumConditionCategory.ListInclude;
    }
}
