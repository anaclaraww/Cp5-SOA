namespace Pedidos.Api.Observability;

/// <summary>
/// Escopo de log da aplicação. O formatter JSON só inclui escopos deste tipo,
/// ignorando os escopos internos do ASP.NET Core para manter o log enxuto.
/// </summary>
public sealed class LogScope : Dictionary<string, object?>
{
}
