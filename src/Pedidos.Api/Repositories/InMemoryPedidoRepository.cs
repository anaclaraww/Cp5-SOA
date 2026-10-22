using System.Collections.Concurrent;
using Pedidos.Api.Domain;

namespace Pedidos.Api.Repositories;

/// <summary>
/// Armazenamento em memória (thread-safe). Os dados existem enquanto a aplicação estiver rodando.
/// </summary>
public sealed class InMemoryPedidoRepository : IPedidoRepository
{
    private readonly ConcurrentDictionary<Guid, Pedido> _pedidos = new();

    public void Adicionar(Pedido pedido)
    {
        if (!_pedidos.TryAdd(pedido.Id, pedido))
        {
            throw new InvalidOperationException($"Já existe um pedido com o id {pedido.Id}.");
        }
    }

    public Pedido? ObterPorId(Guid id) =>
        _pedidos.TryGetValue(id, out var pedido) ? pedido : null;
}
