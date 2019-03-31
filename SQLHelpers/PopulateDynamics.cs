using CommonBasicStandardLibraries.CollectionClasses;
using Dapper;
using DapperHelpersLibrary.MapHelpers;
using System;
using System.Collections.Generic;
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
        public static void PopulateSimple(CustomBasicList<DatabaseMapping> ThisList, DapperSQLData output, EnumCategory Category )
        {
            ThisList.ForEach(Items =>
            {
                //output.Parameters.Add(Items.DatabaseName, Items.Value);
                if (Category == EnumCategory.UseDatabaseMapping)
                    output.Parameters.Add(Items.DatabaseName, Items.Value);
                else
                {
                    if (Items.Like)
                        output.Parameters.Add($"@{Items.ObjectName}", "%" + Items.Value + "%");
                    else
                        output.Parameters.Add($"@{Items.ObjectName}", Items.Value);
                }
                    
            });
        }
        //can see what can be done for cases not so simple.

        public static DynamicParameters GetDynamicIDData(ref StringBuilder builder, int ID, bool IsJoined = false)
        {
            DynamicParameters output = new DynamicParameters();
            output.Add("ID", ID);
            if (IsJoined == false)
                builder.Append(" where ID = @ID");
            else
                builder.Append(" where a.ID = @ID");

            return output;
        }

        
        

    }
}