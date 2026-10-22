namespace Pedidos.Api.Domain;

/// <summary>
/// Entidade de domínio que representa um pedido de comércio eletrônico.
/// </summary>
public sealed class Pedido
{
    public required Guid Id { get; init; }
    public required string Cliente { get; init; }
    public required string Descricao { get; init; }
    public required decimal Valor { get; init; }
    public required StatusPedido Status { get; init; }
    public required DateTimeOffset CriadoEm { get; init; }
}
