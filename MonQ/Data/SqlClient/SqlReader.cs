using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Collections;
using System.Data.SqlTypes;
using System.Data.Common;
using MonQ.Data.SqlClient.CommandAdapters;
using MonQ.Data.SqlClient.ConnectionProviders;

namespace MonQ.Data.SqlClient
{
	internal class SqlReader : DbDataReader, IDisposable
	{
		internal int RecordSetIndex;

		internal SqlDataReader reader;
		internal EventHandler closeHandler;
		internal SqlReader(SqlDataReader reader, EventHandler closeHandler = null)
		{
			this.reader = reader;
			this.closeHandler = closeHandler;
			this.RecordSetIndex = 0;
		}

		public override int Depth
		{
			get
			{
				return this.reader.Depth;
			}
		}
		public override int FieldCount
		{
			get
			{
				return this.reader.FieldCount;
			}
		}
		public override bool HasRows
		{
			get
			{
				return this.reader.HasRows;
			}
		}
		public override bool IsClosed
		{
			get
			{
				return this.reader.IsClosed;
			}
		}
		public override int RecordsAffected
		{
			get
			{
				return this.reader.RecordsAffected;
			}
		}
		public override int VisibleFieldCount
		{
			get
			{
				return this.reader.VisibleFieldCount;
			}
		}

		public override object this[int i]
		{
			get
			{
				return this.reader[i];
			}
		}
		public override object this[string name]
		{
			get
			{
				return this.reader[name];
			}
		}

		public override void Close()
		{
			this.reader.Close();
			if (closeHandler != null)
			{
				closeHandler(this, null);
			}
		}

		void IDisposable.Dispose()
		{
			this.Close();
			this.reader.Dispose();
		}

		protected override void Dispose(bool disposing)
		{
			this.Close();
			this.reader.Dispose();
		}

		public override bool GetBoolean(int i)
		{
			return this.reader.GetBoolean(i);
		}
		public override byte GetByte(int i)
		{
			return this.reader.GetByte(i);
		}
		public override long GetBytes(int i, long dataIndex, byte[] buffer, int bufferIndex, int length)
		{
			return this.reader.GetBytes(i, dataIndex, buffer, bufferIndex, length);
		}
		public override char GetChar(int i)
		{
			return this.reader.GetChar(i);
		}
		public override long GetChars(int i, long dataIndex, char[] buffer, int bufferIndex, int length)
		{
			return this.reader.GetChars(i, dataIndex, buffer, bufferIndex, length);
		}
		public override string GetDataTypeName(int i)
		{
			return this.reader.GetDataTypeName(i);
		}
		public override DateTime GetDateTime(int i)
		{
			return this.reader.GetDateTime(i);
		}
		public override decimal GetDecimal(int i)
		{
			return this.reader.GetDecimal(i);
		}
		public override double GetDouble(int i)
		{
			return this.reader.GetDouble(i);
		}
		public override IEnumerator GetEnumerator()
		{
			return this.reader.GetEnumerator();
		}
		public override Type GetFieldType(int i)
		{
			return this.reader.GetFieldType(i);
		}
		public override float GetFloat(int i)
		{
			return this.reader.GetFloat(i);
		}
		public override Guid GetGuid(int i)
		{
			return this.reader.GetGuid(i);
		}
		public override short GetInt16(int i)
		{
			return this.reader.GetInt16(i);
		}
		public override int GetInt32(int i)
		{
			return this.reader.GetInt32(i);
		}
		public override long GetInt64(int i)
		{
			return this.reader.GetInt64(i);
		}
		public override string GetName(int i)
		{
			return this.reader.GetName(i);
		}
		public override int GetOrdinal(string name)
		{
			return this.reader.GetOrdinal(name);
		}
		public override Type GetProviderSpecificFieldType(int i)
		{
			return this.reader.GetProviderSpecificFieldType(i);
		}
		public override object GetProviderSpecificValue(int i)
		{
			return this.reader.GetProviderSpecificValue(i);
		}
		public override int GetProviderSpecificValues(object[] values)
		{
			return this.reader.GetProviderSpecificValues(values);
		}
		public override DataTable GetSchemaTable()
		{
			return this.reader.GetSchemaTable();
		}
		public override string GetString(int i)
		{
			return this.reader.GetString(i);
		}
		public override object GetValue(int i)
		{
			return this.reader.GetValue(i);
		}
		public override int GetValues(object[] values)
		{
			return this.reader.GetValues(values);
		}
		public override bool IsDBNull(int i)
		{
			return this.reader.IsDBNull(i);
		}
		public override bool NextResult()
		{
			if (this.reader.NextResult())
			{
				this.RecordSetIndex++;
				return true;
			}
			return false;
		}
		public override bool Read()
		{
			return this.reader.Read();
		}

	}

}
