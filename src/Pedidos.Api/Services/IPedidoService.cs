using Pedidos.Api.Contracts;
using Pedidos.Api.Domain;

namespace Pedidos.Api.Services;

public interface IPedidoService
{
    ResultadoCriacao Criar(CriarPedidoRequest request);

    Pedido? ObterPorId(Guid id);
}

/// <summary>
/// Resultado da criação: ou o pedido criado, ou os erros de validação por campo.
/// </summary>
public sealed record ResultadoCriacao(Pedido? Pedido, IDictionary<string, string[]> Erros)
{
    public bool Sucesso => Pedido is not null;

    public static ResultadoCriacao Ok(Pedido pedido) => new(pedido, new Dictionary<string, string[]>());

    public static ResultadoCriacao Invalido(IDictionary<string, string[]> erros) => new(null, erros);
}
