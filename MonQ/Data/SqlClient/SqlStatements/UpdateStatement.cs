using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MonQ.Data.SqlClient.SqlStatements;

namespace MonQ.Data.SqlClient.SqlStatements
{
	internal class UpdateStatement : StatementBase, IQueryUpdate
	{
		public UpdateStatement(SqlHelper helper)
			: base(helper)
		{
		}

		public IQueryUpdateFrom FROM(string table)
		{
			throw new NotImplementedException();
		}

		public IQueryUpdateFrom TOP(int n)
		{
			throw new NotImplementedException();
		}

		public IQueryUpdateFrom TOP_PERCENT(int percent)
		{
			throw new NotImplementedException();
		}
	}
}
