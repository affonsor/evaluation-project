namespace Ambev.DeveloperEvaluation.WebApi.Common;

/// <summary>
/// Standard error payload, following the general API error contract:
/// <c>{ "type", "error", "detail" }</c>.
/// </summary>
public class ErrorResponse
{
    /// <summary>Machine-readable error type identifier.</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>Short, human-readable summary of the problem.</summary>
    public string Error { get; set; } = string.Empty;

    /// <summary>Human-readable explanation specific to this occurrence.</summary>
    public string Detail { get; set; } = string.Empty;
}
