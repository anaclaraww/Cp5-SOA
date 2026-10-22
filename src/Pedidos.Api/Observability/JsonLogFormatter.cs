using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;

namespace Pedidos.Api.Observability;

/// <summary>
/// Formata cada log como uma linha JSON com campos fixos:
/// timestamp, level, category, message, requestId (do escopo) e as propriedades estruturadas
/// da mensagem (ex.: orderId, statusCode).
/// </summary>
public sealed class JsonLogFormatter : ConsoleFormatter
{
    public const string FormatterName = "pedidos-json";

    private static readonly JsonWriterOptions WriterOptions = new()
    {
        // Mantém acentos legíveis; aspas e caracteres de controle continuam escapados.
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public JsonLogFormatter() : base(FormatterName)
    {
    }

    public override void Write<TState>(
        in LogEntry<TState> logEntry,
        IExternalScopeProvider? scopeProvider,
        TextWriter textWriter)
    {
        var message = logEntry.Formatter?.Invoke(logEntry.State, logEntry.Exception);
        if (message is null && logEntry.Exception is null)
        {
            return;
        }

        using var buffer = new MemoryStream();
        using (var writer = new Utf8JsonWriter(buffer, WriterOptions))
        {
            var escritos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            writer.WriteStartObject();
            WriteProperty(writer, escritos, "timestamp",
                DateTimeOffset.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture));
            WriteProperty(writer, escritos, "level", ToLevel(logEntry.LogLevel));
            WriteProperty(writer, escritos, "category", logEntry.Category);
            WriteProperty(writer, escritos, "message", message);

            scopeProvider?.ForEachScope((scope, state) =>
            {
                if (scope is LogScope propriedades)
                {
                    foreach (var (chave, valor) in propriedades)
                    {
                        WriteProperty(state.writer, state.escritos, chave, valor);
                    }
                }
            }, (writer, escritos));

            if (logEntry.State is IReadOnlyList<KeyValuePair<string, object?>> propriedadesDaMensagem)
            {
                foreach (var (chave, valor) in propriedadesDaMensagem)
                {
                    if (chave != "{OriginalFormat}")
                    {
                        WriteProperty(writer, escritos, chave, valor);
                    }
                }
            }

            if (logEntry.Exception is not null)
            {
                WriteProperty(writer, escritos, "exception", logEntry.Exception.ToString());
            }

            writer.WriteEndObject();
        }

        textWriter.WriteLine(Encoding.UTF8.GetString(buffer.GetBuffer(), 0, (int)buffer.Length));
    }

    private static string ToLevel(LogLevel level) => level switch
    {
        LogLevel.Trace => "TRACE",
        LogLevel.Debug => "DEBUG",
        LogLevel.Information => "INFO",
        LogLevel.Warning => "WARN",
        LogLevel.Error => "ERROR",
        LogLevel.Critical => "FATAL",
        _ => level.ToString().ToUpperInvariant()
    };

    private static void WriteProperty(Utf8JsonWriter writer, HashSet<string> escritos, string nome, object? valor)
    {
        // Evita chaves duplicadas no mesmo objeto JSON.
        if (!escritos.Add(nome))
        {
            return;
        }

        switch (valor)
        {
            case null:
                writer.WriteNull(nome);
                break;
            case string texto:
                writer.WriteString(nome, texto);
                break;
            case bool booleano:
                writer.WriteBoolean(nome, booleano);
                break;
            case int inteiro:
                writer.WriteNumber(nome, inteiro);
                break;
            case long longo:
                writer.WriteNumber(nome, longo);
                break;
            case double real:
                writer.WriteNumber(nome, real);
                break;
            case decimal numero:
                writer.WriteNumber(nome, numero);
                break;
            case DateTimeOffset data:
                writer.WriteString(nome, data);
                break;
            case Guid guid:
                writer.WriteString(nome, guid);
                break;
            default:
                writer.WriteString(nome, Convert.ToString(valor, CultureInfo.InvariantCulture));
                break;
        }
    }
}
