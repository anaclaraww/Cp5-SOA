using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging.Console;
using Pedidos.Api.Endpoints;
using Pedidos.Api.Observability;
using Pedidos.Api.Repositories;
using Pedidos.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Logs estruturados: uma linha JSON por evento, no console.
builder.Logging.ClearProviders();
builder.Logging
    .AddConsole(options => options.FormatterName = JsonLogFormatter.FormatterName)
    .AddConsoleFormatter<JsonLogFormatter, ConsoleFormatterOptions>();

// JSON: camelCase (padrão) e enums como texto ("CRIADO").
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Erros no formato padrão RFC 9457 (application/problem+json), com o requestId da requisição.
builder.Services.AddProblemDetails(options =>
    options.CustomizeProblemDetails = context =>
        context.ProblemDetails.Extensions["requestId"] = context.HttpContext.TraceIdentifier);

// JSON malformado ou corpo ausente vira 400 (e não exceção 500), em qualquer ambiente.
builder.Services.Configure<RouteHandlerOptions>(options => options.ThrowOnBadRequest = false);

// Injeção de dependência: endpoints -> serviço -> repositório em memória.
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IPedidoRepository, InMemoryPedidoRepository>();
builder.Services.AddScoped<IPedidoService, PedidoService>();

var app = builder.Build();

app.UseMiddleware<RequestIdMiddleware>();
app.UseExceptionHandler();
app.UseStatusCodePages();

app.MapPedidoEndpoints();

app.Run();

// Exposto para os testes de integração (WebApplicationFactory<Program>).
public partial class Program;
