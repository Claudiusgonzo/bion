﻿using BSOA.Generator.Schema;
using System.Collections.Generic;

namespace BSOA.Generator.Generation
{
    /// <summary>
    ///  TemplateDefaults knows the values used in the Template code; these are used to string.Replace
    ///  the defaults from the template with the correct value for the desired column.
    /// </summary>
    /// <remarks>
    ///  Template Defaults were chosen carefully to avoid string.Replace over-replacing.
    ///  Avoid the type 'int', which will be replaced in 'internal'.
    ///  Avoid names which include the type name, causing incorrect replacements.
    /// </remarks>
    public static class TemplateDefaults
    {
        public static string TableName = "Team";
        public static string DatabaseName = "CompanyDatabase";
        public static string Namespace = "BSOA.Generator.Templates";

        public static Dictionary<ColumnTypeCategory, Schema.Column> Columns = new Dictionary<ColumnTypeCategory, Schema.Column>()
        {
            [ColumnTypeCategory.Simple]     = Schema.Column.Simple("EmployeeId", "long", "-1"),
            [ColumnTypeCategory.DateTime]   = Schema.Column.DateTime("WhenFormed", "DateTime.MinValue"),
            [ColumnTypeCategory.Enum]       = Schema.Column.Enum("JoinPolicy", "SecurityPolicy", "byte", "SecurityPolicy.Open"),
            [ColumnTypeCategory.FlagsEnum]  = Schema.Column.FlagsEnum("Attributes", "GroupAttributes", "long", "GroupAttributes.None"),
            [ColumnTypeCategory.Ref]        = Schema.Column.Ref("Manager", "Employee"),
            [ColumnTypeCategory.RefList]    = Schema.Column.RefList("Members", "Employee")
        };
    }
}
