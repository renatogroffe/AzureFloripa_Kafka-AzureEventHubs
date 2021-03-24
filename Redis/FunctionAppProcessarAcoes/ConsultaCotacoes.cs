using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using FunctionAppProcessarAcoes.Data;
using FunctionAppProcessarAcoes.Models;

namespace FunctionAppProcessarAcoes
{
    public class ConsultaCotacoes
    {
        private readonly AcoesRepository _repository;

        public ConsultaCotacoes(AcoesRepository repository)
        {
            _repository = repository;
        }

        [FunctionName("ValorAtual")]
        public IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "acoes/{codigo}")] HttpRequest req,
            string codigo,
            ILogger log)
        {

            if (String.IsNullOrWhiteSpace(codigo))
            {
                log.LogError(
                    $"ValorAtual HTTP trigger - Codigo da Acao nao informado");
                return new BadRequestObjectResult(new
                {
                    Sucesso = false,
                    Mensagem = "Codigo da Acao nao informado"
                });
            }

            log.LogInformation($"ValorAtual HTTP trigger - codigo da Acao: {codigo}");
            Acao cotacaoAcao = null;
            if (!String.IsNullOrWhiteSpace(codigo))
                cotacaoAcao = _repository.Get(codigo.ToUpper());

            if (cotacaoAcao != null)
            {
                log.LogInformation(
                    $"ValorAtual HTTP trigger - Acao: {codigo} | Valor atual: {cotacaoAcao.Valor} | Ultima atualizacao: {cotacaoAcao.Data}");
                return new OkObjectResult(cotacaoAcao);
            }
            else
            {
                log.LogError(
                    $"ValorAtual HTTP trigger - Codigo da Acao nao encontrado: {codigo}");
                return new NotFoundObjectResult(new
                {
                    Sucesso = false,
                    Mensagem = $"Codigo da Acao nao encontrado: {codigo}"
                });
            }

        }
    }
}