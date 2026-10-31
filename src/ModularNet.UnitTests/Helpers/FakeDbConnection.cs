using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace ModularNet.UnitTests.Helpers;

public class FakeDbConnection : DbConnection
{
    private ConnectionState _state = ConnectionState.Open;

    public override ConnectionState State => _state;
    public override string Database => "test_database";
    public override string DataSource => "localhost";
    public override string ServerVersion => "8.0";

    [AllowNull]
    public override string ConnectionString { get; set; } = string.Empty;

    public override void ChangeDatabase(string databaseName) { }
    public override void Close() { _state = ConnectionState.Closed; }
    public override void Open() { _state = ConnectionState.Open; }

    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
    {
        return new FakeDbTransaction(this);
    }

    protected override DbCommand CreateDbCommand()
    {
        return new FakeDbCommand(this);
    }
}

public class FakeDbTransaction : DbTransaction
{
    private readonly DbConnection _dbConnection;

    public FakeDbTransaction(DbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public override IsolationLevel IsolationLevel => IsolationLevel.ReadCommitted;
    protected override DbConnection DbConnection => _dbConnection;
    public override void Commit() { }
    public override void Rollback() { }
}

public class FakeDbCommand : DbCommand
{
    private readonly FakeDbParameterCollection _parameters = new();

    public FakeDbCommand(DbConnection connection)
    {
        DbConnection = connection;
    }

    [AllowNull] public override string CommandText { get; set; } = string.Empty;
    public override int CommandTimeout { get; set; }
    public override CommandType CommandType { get; set; }
    protected override DbConnection? DbConnection { get; set; }
    protected override DbParameterCollection DbParameterCollection => _parameters;
    protected override DbTransaction? DbTransaction { get; set; }
    public override bool DesignTimeVisible { get; set; }
    public override UpdateRowSource UpdatedRowSource { get; set; }

    public override void Cancel() { }

    protected override DbParameter CreateDbParameter() => new FakeDbParameter();

    protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior) => new FakeDbDataReader();

    protected override Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken) =>
        Task.FromResult<DbDataReader>(new FakeDbDataReader());

    public override int ExecuteNonQuery() => 1;

    public override Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken) => Task.FromResult(1);

    public override object? ExecuteScalar() => null;

    public override Task<object?> ExecuteScalarAsync(CancellationToken cancellationToken) => Task.FromResult<object?>(null);

    public override void Prepare() { }
}

public class FakeDbParameter : DbParameter
{
    public override DbType DbType { get; set; }
    public override ParameterDirection Direction { get; set; }
    public override bool IsNullable { get; set; }
    [AllowNull] public override string ParameterName { get; set; } = string.Empty;
    [AllowNull] public override string SourceColumn { get; set; } = string.Empty;
    public override object? Value { get; set; }
    public override bool SourceColumnNullMapping { get; set; }
    public override int Size { get; set; }
    public override void ResetDbType() { }
}

public class FakeDbParameterCollection : DbParameterCollection
{
    private readonly List<DbParameter> _parameters = new();

    public override int Count => _parameters.Count;
    public override object SyncRoot => ((System.Collections.ICollection)_parameters).SyncRoot;

    public override int Add(object? value)
    {
        if (value is DbParameter p)
            _parameters.Add(p);
        return _parameters.Count - 1;
    }

    public override void AddRange(Array values)
    {
        foreach (var val in values)
        {
            if (val is DbParameter p)
                _parameters.Add(p);
        }
    }

    public override void Clear() => _parameters.Clear();
    public override bool Contains(object? value) => value is DbParameter p && _parameters.Contains(p);
    public override bool Contains(string value) => _parameters.Any(p => p.ParameterName == value);
    public override void CopyTo(Array array, int index) => ((System.Collections.ICollection)_parameters).CopyTo(array, index);
    public override System.Collections.IEnumerator GetEnumerator() => _parameters.GetEnumerator();
    protected override DbParameter GetParameter(int index) => _parameters[index];
    protected override DbParameter GetParameter(string parameterName) => _parameters.First(p => p.ParameterName == parameterName);
    public override int IndexOf(object? value) => value is DbParameter p ? _parameters.IndexOf(p) : -1;
    public override int IndexOf(string parameterName) => _parameters.FindIndex(p => p.ParameterName == parameterName);
    public override void Insert(int index, object? value)
    {
        if (value is DbParameter p)
            _parameters.Insert(index, p);
    }
    public override void Remove(object? value)
    {
        if (value is DbParameter p)
            _parameters.Remove(p);
    }
    public override void RemoveAt(int index) => _parameters.RemoveAt(index);
    public override void RemoveAt(string parameterName) => RemoveAt(IndexOf(parameterName));
    protected override void SetParameter(int index, DbParameter value) => _parameters[index] = value;
    protected override void SetParameter(string parameterName, DbParameter value) => SetParameter(IndexOf(parameterName), value);
}

public class FakeDbDataReader : DbDataReader
{
    public override bool Read() => false;
    public override Task<bool> ReadAsync(CancellationToken cancellationToken) => Task.FromResult(false);
    public override int FieldCount => 0;
    public override int Depth => 0;
    public override bool IsClosed => false;
    public override int RecordsAffected => 0;
    public override bool HasRows => false;
    public override object this[int ordinal] => throw new IndexOutOfRangeException();
    public override object this[string name] => throw new IndexOutOfRangeException();
    public override bool GetBoolean(int ordinal) => false;
    public override byte GetByte(int ordinal) => 0;
    public override long GetBytes(int ordinal, long dataOffset, byte[]? buffer, int bufferOffset, int length) => 0;
    public override char GetChar(int ordinal) => '\0';
    public override long GetChars(int ordinal, long dataOffset, char[]? buffer, int bufferOffset, int length) => 0;
    public override string GetDataTypeName(int ordinal) => string.Empty;
    public override DateTime GetDateTime(int ordinal) => DateTime.MinValue;
    public override decimal GetDecimal(int ordinal) => 0;
    public override double GetDouble(int ordinal) => 0;
    public override System.Collections.IEnumerator GetEnumerator() => System.Linq.Enumerable.Empty<object>().GetEnumerator();
    public override Type GetFieldType(int ordinal) => typeof(object);
    public override float GetFloat(int ordinal) => 0;
    public override Guid GetGuid(int ordinal) => Guid.Empty;
    public override short GetInt16(int ordinal) => 0;
    public override int GetInt32(int ordinal) => 0;
    public override long GetInt64(int ordinal) => 0;
    public override string GetName(int ordinal) => string.Empty;
    public override int GetOrdinal(string name) => -1;
    public override string GetString(int ordinal) => string.Empty;
    public override object GetValue(int ordinal) => new object();
    public override int GetValues(object[] values) => 0;
    public override bool IsDBNull(int ordinal) => true;
    public override bool NextResult() => false;
}
