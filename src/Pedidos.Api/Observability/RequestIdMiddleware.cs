using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Pedidos.Api.Observability;

/// <summary>
/// Gera (ou reaproveita do header X-Request-Id) um identificador por requisição,
/// devolve esse id no header da resposta e o coloca no escopo de log, para que
/// todo log emitido durante a requisição carregue o campo "requestId".
/// </summary>
public sealed partial class RequestIdMiddleware(RequestDelegate next, ILogger<RequestIdMiddleware> logger)
{
    public const string HeaderName = "X-Request-Id";

    public async Task InvokeAsync(HttpContext context)
    {
        var requestId = ObterOuGerarRequestId(context);
        context.TraceIdentifier = requestId;

        // OnStarting garante o header mesmo quando o tratador de erros limpa a resposta.
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = requestId;
            return Task.CompletedTask;
        });

        using var scope = logger.BeginScope(new LogScope { ["requestId"] = requestId });
        var cronometro = Stopwatch.StartNew();

        try
        {
            await next(context);
        }
        finally
        {
            cronometro.Stop();
            var statusCode = context.Response.StatusCode;
            var nivel = statusCode >= 500 ? LogLevel.Error
                      : statusCode >= 400 ? LogLevel.Warning
                      : LogLevel.Information;

            logger.Log(
                nivel,
                "Requisição HTTP concluída. method={method} path={path} statusCode={statusCode} durationMs={durationMs}",
                context.Request.Method,
                context.Request.Path.Value,
                statusCode,
                Math.Round(cronometro.Elapsed.TotalMilliseconds, 2));
        }
    }

    private static string ObterOuGerarRequestId(HttpContext context)
    {
        var recebido = context.Request.Headers[HeaderName].ToString();
        return RequestIdValido().IsMatch(recebido) ? recebido : Guid.NewGuid().ToString();
    }

    // Aceita ids de até 64 caracteres seguros para evitar injeção de conteúdo nos logs.
    [GeneratedRegex("^[A-Za-z0-9._-]{1,64}$")]
    private static partial Regex RequestIdValido();
}
