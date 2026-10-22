using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.Testing;
using Pedidos.Api.Contracts;
using Pedidos.Api.Domain;

namespace Pedidos.Api.Tests;

/// <summary>
/// Testes de integração: sobem a API em memória (WebApplicationFactory) e fazem
/// chamadas HTTP reais aos endpoints, verificando status, headers e corpo JSON.
/// </summary>
public sealed class PedidosApiTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task CriarPedido_ComDadosValidos_Retorna201ComPedidoCriado()
    {
        var request = new CriarPedidoRequest("Maria Silva", "1x Notebook Dell Inspiron 15", 4599.90m);

        var response = await _client.PostAsJsonAsync("/pedidos", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var pedido = await response.Content.ReadFromJsonAsync<PedidoResponse>(JsonOptions);
        Assert.NotNull(pedido);
        Assert.NotEqual(Guid.Empty, pedido.Id);
        Assert.Equal("Maria Silva", pedido.Cliente);
        Assert.Equal("1x Notebook Dell Inspiron 15", pedido.Descricao);
        Assert.Equal(4599.90m, pedido.Valor);
        Assert.Equal(StatusPedido.CRIADO, pedido.Status);
        Assert.Equal($"/pedidos/{pedido.Id}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task ConsultarPedido_Existente_Retorna200ComOsMesmosDados()
    {
        var criacao = await _client.PostAsJsonAsync(
            "/pedidos", new CriarPedidoRequest("João Souza", "2x Mouse sem fio", 159.80m));
        var criado = await criacao.Content.ReadFromJsonAsync<PedidoResponse>(JsonOptions);
        Assert.NotNull(criado);

        var response = await _client.GetAsync($"/pedidos/{criado.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var consultado = await response.Content.ReadFromJsonAsync<PedidoResponse>(JsonOptions);
        Assert.Equal(criado, consultado);
    }

    [Fact]
    public async Task ConsultarPedido_Inexistente_Retorna404()
    {
        var idInexistente = Guid.NewGuid();

        var response = await _client.GetAsync($"/pedidos/{idInexistente}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.True(response.Headers.Contains("X-Request-Id"));
    }

    [Fact]
    public async Task CriarPedido_ComDadosInvalidos_Retorna400ComErrosPorCampo()
    {
        var request = new CriarPedidoRequest("", null, -10m);

        var response = await _client.PostAsJsonAsync("/pedidos", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using var corpo = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var erros = corpo.RootElement.GetProperty("errors");
        Assert.True(erros.TryGetProperty("cliente", out _));
        Assert.True(erros.TryGetProperty("descricao", out _));
        Assert.True(erros.TryGetProperty("valor", out _));
    }
}
