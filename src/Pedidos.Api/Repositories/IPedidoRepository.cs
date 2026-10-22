using Pedidos.Api.Domain;

namespace Pedidos.Api.Repositories;

/// <summary>
/// Contrato de armazenamento de pedidos. A camada de negócio depende só desta interface,
/// então trocar a memória por um banco real não afeta o restante da aplicação.
/// </summary>
public interface IPedidoRepository
{
    void Adicionar(Pedido pedido);

    Pedido? ObterPorId(Guid id);
}
