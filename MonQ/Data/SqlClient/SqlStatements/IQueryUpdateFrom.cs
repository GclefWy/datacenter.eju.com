using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonQ.Data.SqlClient.SqlStatements
{
	public interface IQueryUpdateFrom
	{
		IQueryUpdateSet SET(string column, string value);
	}
}
