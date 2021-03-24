using System.Text.Json;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Kafka;
using Microsoft.Extensions.Logging;
using FunctionAppProcessarAcoes.Models;
using FunctionAppProcessarAcoes.Validators;
using FunctionAppProcessarAcoes.Data;

namespace FunctionAppProcessarAcoes
{
    public class ProcessarAcoes
    {
        private readonly AcoesRepository _repository;

        public ProcessarAcoes(AcoesRepository repository)
        {
            _repository = repository;
        }

        [FunctionName("ProcessarAcoesRedis")]
        public void Run([KafkaTrigger(
            "BrokerKafka", "topic-acoes",
            ConsumerGroup = "processar_acoes-redis",
            Protocol = BrokerProtocol.SaslSsl,
            AuthenticationMode = BrokerAuthenticationMode.Plain,
            Username = "UserKafka",
            Password = "PasswordKafka"
            )]KafkaEventData<string> kafkaEvent,
            ILogger log)
        {
            string dados = kafkaEvent.Value.ToString();
            log.LogInformation($"ProcessarAcoesRedis - Dados: {dados}");

            Acao acao = null;
            try
            {
                acao = JsonSerializer.Deserialize<Acao>(dados,
                    new JsonSerializerOptions()
                    {
                        PropertyNameCaseInsensitive = true
                    });
            }
            catch
            {
                log.LogError("ProcessarAcoesRedis - Erro durante a deserializacao!");
            }

            if (acao != null)
            {
                var validationResult = new AcaoValidator().Validate(acao);
                if (validationResult.IsValid)
                {
                    log.LogInformation($"ProcessarAcoesRedis - Dados pos formatacao: {JsonSerializer.Serialize(acao)}");
                    _repository.Save(acao);
                    log.LogInformation("ProcessarAcoesRedis - Acao registrada com sucesso!");
                }
                else
                {
                    log.LogError("ProcessarAcoesRedis - Dados invalidos para a Acao");
                    foreach (var error in validationResult.Errors)
                        log.LogError($"ProcessarAcoesRedis - {error.ErrorMessage}");
                }
            }
        }

    }
}