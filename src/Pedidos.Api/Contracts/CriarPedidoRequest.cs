namespace Pedidos.Api.Contracts;

/// <summary>
/// Corpo da requisição POST /pedidos. Id, status e data de criação são definidos pelo servidor.
/// </summary>
public sealed record CriarPedidoRequest(string? Cliente, string? Descricao, decimal? Valor);
