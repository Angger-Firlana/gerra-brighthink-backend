using System.Text;
using Dapper;

namespace backend.Helpers;

/// <summary>
/// Builds dynamic WHERE clauses and partial UPDATE SET clauses for Dapper queries.
/// </summary>
public static class SqlHelper
{
    /// <summary>
    /// Start building a dynamic WHERE clause. Call .Add() for each condition, then .Build().
    /// </summary>
    public static WhereBuilder Where(string initial = "") => new(initial);

    /// <summary>
    /// Start building a dynamic UPDATE SET. Call .Set() for each field, then .Build().
    /// </summary>
    public static UpdateBuilder Update(string table) => new(table);
}

public class WhereBuilder
{
    private readonly List<string> _clauses = [];
    private readonly DynamicParameters _params = new();

    public WhereBuilder(string initial)
    {
        if (!string.IsNullOrWhiteSpace(initial))
            _clauses.Add(initial);
    }

    /// <summary>Add a condition if the value is not null/empty.</summary>
    public WhereBuilder Add(string condition, string paramName, object? value)
    {
        if (value is null) return this;
        if (value is string s && string.IsNullOrWhiteSpace(s)) return this;

        _clauses.Add(condition);
        _params.Add(paramName, value);
        return this;
    }

    /// <summary>Add a LIKE condition if the value is not null/empty.</summary>
    public WhereBuilder AddLike(string field, string paramName, string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return this;
        _clauses.Add($"{field} LIKE @{paramName}");
        _params.Add(paramName, $"%{value}%");
        return this;
    }

    /// <summary>Add an int equality condition if the value is parseable.</summary>
    public WhereBuilder AddInt(string field, string paramName, string? rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue) || !int.TryParse(rawValue, out var parsed)) return this;
        _clauses.Add($"{field} = @{paramName}");
        _params.Add(paramName, parsed);
        return this;
    }

    public (string clause, DynamicParameters parameters) Build()
    {
        var sql = _clauses.Count > 0 ? string.Join(" AND ", _clauses) : "1=1";
        return (sql, _params);
    }
}

public class UpdateBuilder
{
    private readonly string _table;
    private readonly List<string> _sets = [];
    private readonly DynamicParameters _params = new();
    private string? _where;
    private DynamicParameters? _whereParams;

    public UpdateBuilder(string table) => _table = table;

    /// <summary>Add a SET field=@param if value is not null.</summary>
    public UpdateBuilder Set(string field, string paramName, object? value)
    {
        if (value is null) return this;
        // Skip empty strings for string-typed updates
        if (value is string s && string.IsNullOrWhiteSpace(s)) return this;

        _sets.Add($"{field} = @{paramName}");
        _params.Add(paramName, value);
        return this;
    }

    /// <summary>Set the WHERE clause with params (typically "Id = @Id").</summary>
    public UpdateBuilder Where(string clause, object? parameters = null)
    {
        _where = clause;
        if (parameters is not null)
        {
            _whereParams = new DynamicParameters();
            _whereParams.AddDynamicParams(parameters);
        }
        return this;
    }

    public (string sql, DynamicParameters parameters) Build()
    {
        if (_sets.Count == 0)
            throw new InvalidOperationException("No fields to update. Call Set() at least once.");

        var sb = new StringBuilder($"UPDATE {_table} SET ");
        sb.Append(string.Join(", ", _sets));
        sb.Append(", updated_at = NOW()");
        if (!string.IsNullOrWhiteSpace(_where))
            sb.Append($" WHERE {_where}");

        var combined = _params;
        if (_whereParams is not null)
        {
            foreach (var p in _whereParams.ParameterNames)
                combined.Add(p, _whereParams.Get<object>(p));
        }

        return (sb.ToString(), combined);
    }
}
