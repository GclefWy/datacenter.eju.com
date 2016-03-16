using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace MonQ.Data.SqlClient
{
	internal class SqlDataSchema : List<SqlColSchema>
	{
		public SqlColSchema this[string name]
		{
			get
			{
				foreach (SqlColSchema col in this)
				{
					if (string.Compare(col.Name, name, true) == 0) return col;
				}
				return null;
			}
		}

		public new void Add(SqlColSchema value)
		{
			if (value == null)
			{
				return;
			}
			int index = -1;
			for (int i = 0; i < this.Count; i++)
			{
				SqlColSchema col = this[i];
				if (string.Compare(col.Name, value.Name, true) == 0)
				{
					index = i;
					break;
				}
			}
			if (index > -1)
			{
				this[index] = value;
			}
			else
			{
				base.Add(value);
			}
		}

		internal SqlDataSchema()
		{
		}

		public static SqlDataSchema Create(DataTable schema)
		{
			SqlDataSchema result = new SqlDataSchema();
			foreach (DataRow row in schema.Rows)
			{
				SqlColSchema rowSchema = new SqlColSchema();
				rowSchema.Name = row["name"].ToString();
				rowSchema.IsDefault = Convert.ToInt32(row["IsDefault"]) == 1;
				rowSchema.IsNullable = Convert.ToInt32(row["IsNullable"]) == 1;
				rowSchema.IsIdentity = Convert.ToInt32(row["IsIdentity"]) == 1;
				rowSchema.IsRowGuidCol = Convert.ToInt32(row["IsRowGuidCol"]) == 1;
				rowSchema.IsRowNewID = Convert.ToInt32(row["IsRowNewID"]) == 1;
				rowSchema.IsPrimaryKey = Convert.ToInt32(row["IsPrimaryKey"]) == 1;
				rowSchema.IsClusteredKey = Convert.ToInt32(row["IsClusteredKey"]) == 1;
				rowSchema.IsUniqueKey = Convert.ToInt32(row["IsUniqueKey"]) == 1;
				rowSchema.TypeName = row["TypeName"].ToString();
				rowSchema.Size = Convert.ToInt32(row["Size"]);
				rowSchema.ColID = Convert.ToInt32(row["ColID"]);
				result.Add(rowSchema);

			}
			return result;
		}

		public SqlColSchema[] Any(Func<SqlColSchema, bool> func)
		{
			List<SqlColSchema> result = new List<SqlColSchema>(this.Count);

			foreach (SqlColSchema row in this)
			{
				if (func(row)) result.Add(row);
			}
			return result.ToArray();
		}

	}
}
