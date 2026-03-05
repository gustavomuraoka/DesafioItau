using CompraAutomatizada.Domain.Common;
using FluentValidation;
using System.Text.Json;

namespace CompraAutomatizada.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            await EscreverRespostaAsync(context, 400, ex.Errors.First().ErrorCode, ex.Errors.First().ErrorMessage);
        }
        catch (DomainException ex)
        {
            var (status, codigo) = ResolverCodigoErro(ex.Message);
            await EscreverRespostaAsync(context, status, codigo, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado.");
            await EscreverRespostaAsync(context, 500, "ERRO_INTERNO", "Ocorreu um erro interno. Tente novamente.");
        }
    }

    private static (int status, string codigo) ResolverCodigoErro(string mensagem) => mensagem switch
    {
        var m when m.Contains("CPF já cadastrado") => (400, "CLIENTE_CPF_DUPLICADO"),
        var m when m.Contains("valor mensal mínimo") => (400, "VALOR_MENSAL_INVALIDO"),
        var m when m.Contains("soma dos percentuais") => (400, "PERCENTUAIS_INVALIDOS"),
        var m when m.Contains("exatamente 5 ativos") => (400, "QUANTIDADE_ATIVOS_INVALIDA"),
        var m when m.Contains("já havia saído") => (400, "CLIENTE_JA_INATIVO"),
        var m when m.Contains("já foi executada") => (409, "COMPRA_JA_EXECUTADA"),
        var m when m.Contains("não encontrado") => (404, "CLIENTE_NAO_ENCONTRADO"),
        var m when m.Contains("cesta ativa") => (404, "CESTA_NAO_ENCONTRADA"),
        var m when m.Contains("COTAHIST não encontrado") => (404, "COTACAO_NAO_ENCONTRADA"),
        _ => (400, "ERRO_NEGOCIO")
    };

    private static async Task EscreverRespostaAsync(HttpContext context, int status, string codigo, string mensagem)
    {
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json; charset=utf-8";

        var corpo = JsonSerializer.Serialize(
            new { erro = mensagem, codigo },
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
        );

        await context.Response.WriteAsync(corpo);
    }
}
