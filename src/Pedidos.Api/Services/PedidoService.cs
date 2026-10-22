using Pedidos.Api.Contracts;
using Pedidos.Api.Domain;
using Pedidos.Api.Repositories;

namespace Pedidos.Api.Services;

/// <summary>
/// Regras de negócio de pedidos: valida os dados, gera id/status/data e persiste via repositório.
/// </summary>
public sealed class PedidoService(
    IPedidoRepository repository,
    TimeProvider timeProvider,
    ILogger<PedidoService> logger) : IPedidoService
{
    public const int TamanhoMaximoCliente = 100;
    public const int TamanhoMaximoDescricao = 500;

    public ResultadoCriacao Criar(CriarPedidoRequest request)
    {
        var erros = Validar(request);
        if (erros.Count > 0)
        {
            logger.LogWarning(
                "Falha de validação ao criar pedido. camposInvalidos={camposInvalidos}",
                string.Join(",", erros.Keys));
            return ResultadoCriacao.Invalido(erros);
        }

        var pedido = new Pedido
        {
            Id = Guid.NewGuid(),
            Cliente = request.Cliente!.Trim(),
            Descricao = request.Descricao!.Trim(),
            Valor = request.Valor!.Value,
            Status = StatusPedido.CRIADO,
            CriadoEm = timeProvider.GetUtcNow()
        };

        repository.Adicionar(pedido);

        logger.LogInformation(
            "Pedido criado com sucesso. orderId={orderId} valor={valor} status={status}",
            pedido.Id, pedido.Valor, pedido.Status.ToString());

        return ResultadoCriacao.Ok(pedido);
    }

    public Pedido? ObterPorId(Guid id)
    {
        var pedido = repository.ObterPorId(id);

        if (pedido is null)
        {
            logger.LogWarning("Pedido não encontrado. orderId={orderId}", id);
        }
        else
        {
            logger.LogInformation("Pedido consultado. orderId={orderId}", id);
        }

        return pedido;
    }

    private static Dictionary<string, string[]> Validar(CriarPedidoRequest request)
    {
        var erros = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Cliente))
        {
            erros["cliente"] = ["O cliente é obrigatório."];
        }
        else if (request.Cliente.Trim().Length > TamanhoMaximoCliente)
        {
            erros["cliente"] = [$"O cliente deve ter no máximo {TamanhoMaximoCliente} caracteres."];
        }

        if (string.IsNullOrWhiteSpace(request.Descricao))
        {
            erros["descricao"] = ["A descrição é obrigatória."];
        }
        else if (request.Descricao.Trim().Length > TamanhoMaximoDescricao)
        {
            erros["descricao"] = [$"A descrição deve ter no máximo {TamanhoMaximoDescricao} caracteres."];
        }

        if (request.Valor is null)
        {
            erros["valor"] = ["O valor é obrigatório."];
        }
        else if (request.Valor <= 0)
        {
            erros["valor"] = ["O valor deve ser maior que zero."];
        }

        return erros;
    }
}
