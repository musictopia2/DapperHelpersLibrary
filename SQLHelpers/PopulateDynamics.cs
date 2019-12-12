using CommonBasicStandardLibraries.CollectionClasses;
using Dapper;
using DapperHelpersLibrary.MapHelpers;
using System.Text;
namespace DapperHelpersLibrary.SQLHelpers
{
    internal static class PopulateDynamics //so i can test.
    {
        public enum EnumCategory
        {
            UseDatabaseMapping,
            Conditional
        }
        public static void PopulateSimple(CustomBasicList<DatabaseMapping> thisList, DapperSQLData output, EnumCategory category)
        {
            thisList.ForEach(items =>
            {
                if (category == EnumCategory.UseDatabaseMapping)
                {
                    output.Parameters.Add(items.DatabaseName, items.Value);
                }
                else
                {
                    if (items.Like)
                        output.Parameters.Add($"@{items.ObjectName}", "%" + items.Value + "%");
                    else
                        output.Parameters.Add($"@{items.ObjectName}", items.Value);
                }
            });
        }
        public static DynamicParameters GetDynamicIDData(ref StringBuilder builder, int ID, bool isJoined = false)
        {
            DynamicParameters output = new DynamicParameters();
            output.Add("ID", ID);
            if (isJoined == false)
                builder.Append(" where ID = @ID");
            else
                builder.Append(" where a.ID = @ID");
            return output;
        }
    }
}