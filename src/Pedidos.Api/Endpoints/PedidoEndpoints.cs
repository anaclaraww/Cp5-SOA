using Microsoft.AspNetCore.Http.HttpResults;
using Pedidos.Api.Contracts;
using Pedidos.Api.Services;

namespace Pedidos.Api.Endpoints;

/// <summary>
/// Camada de entrada HTTP: recebe/devolve JSON e traduz o resultado do serviço em códigos HTTP.
/// Não contém regra de negócio.
/// </summary>
public static class PedidoEndpoints
{
    public static IEndpointRouteBuilder MapPedidoEndpoints(this IEndpointRouteBuilder app)
    {
        var pedidos = app.MapGroup("/pedidos").WithTags("Pedidos");

        pedidos.MapPost("/", CriarPedido).WithName("CriarPedido");
        pedidos.MapGet("/{id:guid}", ObterPedidoPorId).WithName("ObterPedidoPorId");

        return app;
    }

    private static Results<Created<PedidoResponse>, ValidationProblem> CriarPedido(
        CriarPedidoRequest request,
        IPedidoService service)
    {
        var resultado = service.Criar(request);

        if (!resultado.Sucesso)
        {
            return TypedResults.ValidationProblem(resultado.Erros, title: "Dados do pedido inválidos.");
        }

        var response = PedidoResponse.From(resultado.Pedido!);
        return TypedResults.Created($"/pedidos/{response.Id}", response);
    }

    private static Results<Ok<PedidoResponse>, ProblemHttpResult> ObterPedidoPorId(
        Guid id,
        IPedidoService service)
    {
        var pedido = service.ObterPorId(id);

        if (pedido is null)
        {
            return TypedResults.Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Pedido não encontrado.",
                detail: $"Não existe pedido com o id {id}.");
        }

        return TypedResults.Ok(PedidoResponse.From(pedido));
    }
}
