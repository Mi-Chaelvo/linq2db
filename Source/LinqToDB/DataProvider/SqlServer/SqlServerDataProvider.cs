﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;

namespace LinqToDB.DataProvider.SqlServer
{
	using System.Linq.Expressions;
	using Common;
	using Data;
	using LinqToDB.Extensions;
	using Mapping;
	using SchemaProvider;
	using SqlProvider;

	public class SqlServerDataProvider : DynamicDataProviderBase<SqlServerProviderAdapter>
	{
		#region Init

		public SqlServerDataProvider(string name, SqlServerVersion version)
			: this(name, version, SqlServerProvider.SystemDataSqlClient)
		{
		}

		public SqlServerDataProvider(string name, SqlServerVersion version, SqlServerProvider provider)
			: base(
				  name,
				  MappingSchemaInstance.Get(version),
				  SqlServerProviderAdapter.GetInstance(provider))
		{
			Version  = version;
			Provider = provider;

			SqlProviderFlags.IsDistinctOrderBySupported = false;
			SqlProviderFlags.IsSubQueryOrderBySupported = false;
			SqlProviderFlags.IsDistinctSetOperationsSupported = true;
			SqlProviderFlags.IsUpdateFromSupported = true;

			if (version == SqlServerVersion.v2000)
			{
				SqlProviderFlags.AcceptsTakeAsParameter = false;
				SqlProviderFlags.IsSkipSupported = false;
				SqlProviderFlags.IsCountSubQuerySupported = false;
			}
			else
			{
				SqlProviderFlags.IsApplyJoinSupported = true;
				SqlProviderFlags.TakeHintsSupported = TakeHints.Percent | TakeHints.WithTies;
				SqlProviderFlags.IsCommonTableExpressionsSupported = version >= SqlServerVersion.v2008;
			}

			SetCharField("char", (r, i) => r.GetString(i).TrimEnd(' '));
			SetCharField("nchar", (r, i) => r.GetString(i).TrimEnd(' '));
			SetCharFieldToType<char>("char", (r, i) => DataTools.GetChar(r, i));
			SetCharFieldToType<char>("nchar", (r, i) => DataTools.GetChar(r, i));

			switch (version)
			{
				case SqlServerVersion.v2000:
					_sqlOptimizer = new SqlServer2000SqlOptimizer(SqlProviderFlags);
					break;
				case SqlServerVersion.v2005:
					_sqlOptimizer = new SqlServer2005SqlOptimizer(SqlProviderFlags);
					break;
				default:
				case SqlServerVersion.v2008:
					_sqlOptimizer = new SqlServer2008SqlOptimizer(SqlProviderFlags);
					break;
				case SqlServerVersion.v2012:
					_sqlOptimizer = new SqlServer2012SqlOptimizer(SqlProviderFlags);
					break;
				case SqlServerVersion.v2017:
					_sqlOptimizer = new SqlServer2017SqlOptimizer(SqlProviderFlags);
					break;
			}

			SetField<IDataReader, decimal>((r, i) => r.GetDecimal(i));
			SetField<IDataReader, decimal>("money"     , (r, i) => SqlServerTools.DataReaderGetMoney(r, i));
			SetField<IDataReader, decimal>("smallmoney", (r, i) => SqlServerTools.DataReaderGetMoney(r, i));
			SetField<IDataReader, decimal>("decimal"   , (r, i) => SqlServerTools.DataReaderGetDecimal(r, i));

			// missing:
			// GetSqlBytes
			// GetSqlChars
			SetProviderField<SqlBinary  , SqlBinary  >("GetSqlBinary"  , dataReaderType: Adapter.DataReaderType);
			SetProviderField<SqlBoolean , SqlBoolean >("GetSqlBoolean" , dataReaderType: Adapter.DataReaderType);
			SetProviderField<SqlByte    , SqlByte    >("GetSqlByte"    , dataReaderType: Adapter.DataReaderType);
			SetProviderField<SqlDateTime, SqlDateTime>("GetSqlDateTime", dataReaderType: Adapter.DataReaderType);
			SetProviderField<SqlDecimal , SqlDecimal >("GetSqlDecimal" , dataReaderType: Adapter.DataReaderType);
			SetProviderField<SqlDouble  , SqlDouble  >("GetSqlDouble"  , dataReaderType: Adapter.DataReaderType);
			SetProviderField<SqlGuid    , SqlGuid    >("GetSqlGuid"    , dataReaderType: Adapter.DataReaderType);
			SetProviderField<SqlInt16   , SqlInt16   >("GetSqlInt16"   , dataReaderType: Adapter.DataReaderType);
			SetProviderField<SqlInt32   , SqlInt32   >("GetSqlInt32"   , dataReaderType: Adapter.DataReaderType);
			SetProviderField<SqlInt64   , SqlInt64   >("GetSqlInt64"   , dataReaderType: Adapter.DataReaderType);
			SetProviderField<SqlMoney   , SqlMoney   >("GetSqlMoney"   , dataReaderType: Adapter.DataReaderType);
			SetProviderField<SqlSingle  , SqlSingle  >("GetSqlSingle"  , dataReaderType: Adapter.DataReaderType);
			SetProviderField<SqlString  , SqlString  >("GetSqlString"  , dataReaderType: Adapter.DataReaderType);
			SetProviderField<SqlXml     , SqlXml     >("GetSqlXml"     , dataReaderType: Adapter.DataReaderType);

			SetProviderField<DateTimeOffset>("GetDateTimeOffset", dataReaderType: Adapter.DataReaderType);
			SetProviderField<TimeSpan>      ("GetTimeSpan"      , dataReaderType: Adapter.DataReaderType);

			// non-specific fallback
			SetProviderField<IDataReader, SqlString, SqlString>((r, i) => r.GetString(i));
		}

		#endregion

		#region Public Properties

		public SqlServerVersion Version { get; }

		public SqlServerProvider Provider { get; }

		#endregion

		#region Overrides

		static class MappingSchemaInstance
		{
			public static readonly MappingSchema SqlServer2000MappingSchema = new SqlServer2000MappingSchema();
			public static readonly MappingSchema SqlServer2005MappingSchema = new SqlServer2005MappingSchema();
			public static readonly MappingSchema SqlServer2008MappingSchema = new SqlServer2008MappingSchema();
			public static readonly MappingSchema SqlServer2012MappingSchema = new SqlServer2012MappingSchema();
			public static readonly MappingSchema SqlServer2017MappingSchema = new SqlServer2017MappingSchema();

			public static MappingSchema Get(SqlServerVersion version)
			{
				switch (version)
				{
					case SqlServerVersion.v2000: return SqlServer2000MappingSchema;
					case SqlServerVersion.v2005: return SqlServer2005MappingSchema;
					default:
					case SqlServerVersion.v2008: return SqlServer2008MappingSchema;
					case SqlServerVersion.v2012: return SqlServer2012MappingSchema;
					case SqlServerVersion.v2017: return SqlServer2017MappingSchema;
				}
			}
		}

		public override ISqlBuilder CreateSqlBuilder(MappingSchema mappingSchema)
		{
			switch (Version)
			{
				case SqlServerVersion.v2000 : return new SqlServer2000SqlBuilder(this, mappingSchema, GetSqlOptimizer(), SqlProviderFlags);
				case SqlServerVersion.v2005 : return new SqlServer2005SqlBuilder(this, mappingSchema, GetSqlOptimizer(), SqlProviderFlags);
				case SqlServerVersion.v2008 : return new SqlServer2008SqlBuilder(this, mappingSchema, GetSqlOptimizer(), SqlProviderFlags);
				case SqlServerVersion.v2012 : return new SqlServer2012SqlBuilder(this, mappingSchema, GetSqlOptimizer(), SqlProviderFlags);
				case SqlServerVersion.v2017 : return new SqlServer2017SqlBuilder(this, mappingSchema, GetSqlOptimizer(), SqlProviderFlags);
			}

			throw new InvalidOperationException();
		}

		readonly ISqlOptimizer _sqlOptimizer;

		public override ISqlOptimizer GetSqlOptimizer() => _sqlOptimizer;

		public override ISchemaProvider GetSchemaProvider()
		{
			return Version == SqlServerVersion.v2000 ? new SqlServer2000SchemaProvider(this) : new SqlServerSchemaProvider(this);
		}

		static readonly ConcurrentDictionary<string,bool> _marsFlags = new ConcurrentDictionary<string,bool>();

		public override object? GetConnectionInfo(DataConnection dataConnection, string parameterName)
		{
			// take it from real Connection object, as dataConnection.ConnectionString could be null
			// also it will not cache original connection string with credentials in _marsFlags
			var connectionString = dataConnection.Connection.ConnectionString;
			switch (parameterName)
			{
				case "IsMarsEnabled" :
					if (connectionString != null)
					{
						if (!_marsFlags.TryGetValue(connectionString, out var flag))
						{
							flag = Adapter.CreateConnectionStringBuilder(connectionString).MultipleActiveResultSets;
							_marsFlags[connectionString] = flag;
						}

						return flag;
					}

					return false;
			}

			return null;
		}

		public override void SetParameter(DataConnection dataConnection, IDbDataParameter parameter, string name, DbDataType dataType, object? value)
		{
			var param = TryGetProviderParameter(parameter, MappingSchema);

			switch (dataType.DataType)
			{
				case DataType.Udt        :
					{
						if (param    != null
							&& value != null
							&& _udtTypes.TryGetValue(value.GetType(), out var s))
							Adapter.SetUdtTypeName(param, s);
					}

					break;
				case DataType.NText:
					     if (value is DateTimeOffset dto) value = dto.ToString("yyyy-MM-ddTHH:mm:ss.ffffff zzz");
					else if (value is DateTime dt)
					{
						value = dt.ToString(
							dt.Millisecond == 0
								? "yyyy-MM-ddTHH:mm:ss"
								: "yyyy-MM-ddTHH:mm:ss.fff");
					}
					else if (value is TimeSpan ts)
					{
						value = ts.ToString(
							ts.Days > 0
								? ts.Milliseconds > 0
									? "d\\.hh\\:mm\\:ss\\.fff"
									: "d\\.hh\\:mm\\:ss"
								: ts.Milliseconds > 0
									? "hh\\:mm\\:ss\\.fff"
									: "hh\\:mm\\:ss");
					}
					break;

				case DataType.Undefined:
					if (value != null
						&& (value is DataTable
						|| value is DbDataReader
							|| value is IEnumerable<DbDataRecord>
							|| value.GetType().IsEnumerableTType(Adapter.SqlDataRecordType)))
					{
						dataType = dataType.WithDataType(DataType.Structured);
					}

					break;
			}

			base.SetParameter(dataConnection, parameter, name, dataType, value);

			if (param != null)
			{
				// Setting for NVarChar and VarChar constant size. It reduces count of cached plans.
				switch (Adapter.GetDbType(param))
				{
					case SqlDbType.Structured:
						{
							if (!dataType.DbType.IsNullOrEmpty())
								Adapter.SetTypeName(param, dataType.DbType);

							// TVP doesn't support DBNull
							if (parameter.Value is DBNull)
								parameter.Value = null;

							break;
						}
					case SqlDbType.VarChar:
						{
							var strValue = value as string;
							if ((strValue != null && strValue.Length > 8000) || (value != null && strValue == null))
								parameter.Size = -1;
							else if (dataType.Length != null && dataType.Length <= 8000 && (strValue == null || strValue.Length <= dataType.Length))
								parameter.Size = dataType.Length.Value;
							else
								parameter.Size = 8000;

							break;
						}
					case SqlDbType.NVarChar:
						{
							var strValue = value as string;
							if ((strValue != null && strValue.Length > 4000) || (value != null && strValue == null))
								parameter.Size = -1;
							else if (dataType.Length != null && dataType.Length <= 4000 && (strValue == null || strValue.Length <= dataType.Length))
								parameter.Size = dataType.Length.Value;
							else
								parameter.Size = 4000;

							break;
						}
					case SqlDbType.VarBinary:
						{
							var binaryValue = value as byte[];
							if ((binaryValue != null && binaryValue.Length > 8000) || (value != null && binaryValue == null))
								parameter.Size = -1;
							else if (dataType.Length != null && dataType.Length <= 8000 && (binaryValue == null || binaryValue.Length <= dataType.Length))
								parameter.Size = dataType.Length.Value;
							else
								parameter.Size = 8000;

							break;
						}
				}
			}
		}

		protected override void SetParameterType(DataConnection dataConnection, IDbDataParameter parameter, DbDataType dataType)
		{
			if (parameter is BulkCopyReader.Parameter)
				return;

			SqlDbType? type = null;

			switch (dataType.DataType)
			{
				case DataType.Text          : type = SqlDbType.Text;          break;
				case DataType.NText         : type = SqlDbType.NText;         break;
				case DataType.Binary        : type = SqlDbType.Binary;        break;
				case DataType.Image         : type = SqlDbType.Image;         break;
				case DataType.SmallMoney    : type = SqlDbType.SmallMoney;    break;
				case DataType.Date          : type = SqlDbType.Date;          break;
				case DataType.Time          : type = SqlDbType.Time;          break;
				case DataType.SmallDateTime : type = SqlDbType.SmallDateTime; break;
				case DataType.Timestamp     : type = SqlDbType.Timestamp;     break;
				case DataType.Structured    : type = SqlDbType.Structured;    break;
			}

			if (type != null)
			{
				var param = TryGetProviderParameter(parameter, dataConnection.MappingSchema);
				if (param != null)
				{
					Adapter.SetDbType(param, type.Value);
					return;
				}
			}

			switch (dataType.DataType)
			{
				// including provider-specic fallbacks
				case DataType.Text          : parameter.DbType = DbType.AnsiString; break;
				case DataType.NText         : parameter.DbType = DbType.String;     break;
				case DataType.Binary        :
				case DataType.Timestamp     :
				case DataType.Image         : parameter.DbType = DbType.Binary;     break;
				case DataType.SmallMoney    :
				case DataType.Money         : parameter.DbType = DbType.Currency;    break;
				case DataType.SmallDateTime : parameter.DbType = DbType.DateTime;    break;
				case DataType.Structured    : parameter.DbType = DbType.Object;      break;
				case DataType.Xml           : parameter.DbType = DbType.Xml;         break;
				case DataType.SByte         : parameter.DbType = DbType.Int16;       break;
				case DataType.UInt16        : parameter.DbType = DbType.Int32;       break;
				case DataType.UInt32        : parameter.DbType = DbType.Int64;       break;
				case DataType.UInt64        :
				case DataType.VarNumeric    : parameter.DbType = DbType.Decimal;     break;
				case DataType.DateTime      :
				case DataType.DateTime2     :
					parameter.DbType =
						Version == SqlServerVersion.v2000 || Version == SqlServerVersion.v2005 ?
							DbType.DateTime :
							DbType.DateTime2;
					break;
				default                     : base.SetParameterType(dataConnection, parameter, dataType); break;
			}
		}

		#endregion

		#region Udt support

		static readonly ConcurrentDictionary<Type,string> _udtTypes = new ConcurrentDictionary<Type,string>();

		internal static void SetUdtType(Type type, string udtName)
		{
			_udtTypes[type] = udtName;
		}

		internal static Type? GetUdtType(string udtName)
		{
			foreach (var udtType in _udtTypes)
				if (udtType.Value == udtName)
					return udtType.Key;

			return null;
		}

		public void AddUdtType(Type type, string udtName)
		{
			MappingSchema.SetScalarType(type);

			_udtTypes[type] = udtName;
		}

		public void AddUdtType<T>(string udtName, T defaultValue, DataType dataType = DataType.Undefined)
		{
			MappingSchema.AddScalarType(typeof(T), defaultValue, dataType);

			_udtTypes[typeof(T)] = udtName;
		}

		#endregion

		#region BulkCopy

		SqlServerBulkCopy? _bulkCopy;

		public override BulkCopyRowsCopied BulkCopy<T>(ITable<T> table, BulkCopyOptions options, IEnumerable<T> source)
		{
			if (_bulkCopy == null)
				_bulkCopy = new SqlServerBulkCopy(this);

			return _bulkCopy.BulkCopy(
				options.BulkCopyType == BulkCopyType.Default ? SqlServerTools.DefaultBulkCopyType : options.BulkCopyType,
				table,
				options,
				source);
		}

		#endregion
	}
}
