using Pedidos.Api.Domain;

namespace Pedidos.Api.Contracts;

/// <summary>
/// Representação do pedido devolvida pela API.
/// </summary>
public sealed record PedidoResponse(
    Guid Id,
    string Cliente,
    string Descricao,
    decimal Valor,
    StatusPedido Status,
    DateTimeOffset CriadoEm)
{
    public static PedidoResponse From(Pedido pedido) =>
        new(pedido.Id, pedido.Cliente, pedido.Descricao, pedido.Valor, pedido.Status, pedido.CriadoEm);
}
