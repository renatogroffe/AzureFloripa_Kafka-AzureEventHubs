using System;
using System.Text.Json;
using StackExchange.Redis;
using FunctionAppProcessarAcoes.Models;

namespace FunctionAppProcessarAcoes.Data
{
    public class AcoesRepository
    {
        private readonly ConnectionMultiplexer _conexaoRedis;
        private readonly string _prefixoChaveRedis;

        public AcoesRepository()
        {
            _conexaoRedis = ConnectionMultiplexer.Connect(
                Environment.GetEnvironmentVariable("Redis_Connection"));
            _prefixoChaveRedis =
                Environment.GetEnvironmentVariable("Redis_PrefixoChave"); 
        }

        public void Save(Acao acao)
        {
            acao.Data = DateTime.Now;
            _conexaoRedis.GetDatabase().StringSet(
                $"{_prefixoChaveRedis}-{acao.Codigo}",
                JsonSerializer.Serialize(acao),
                expiry: null);
        }

        public Acao Get(string codigo)
        {
            string strDadosAcao =
                _conexaoRedis.GetDatabase().StringGet(
                    $"{_prefixoChaveRedis}-{codigo}");
            if (!String.IsNullOrWhiteSpace(strDadosAcao))
                return JsonSerializer.Deserialize<Acao>(
                    strDadosAcao,
                    new JsonSerializerOptions()
                    {
                        PropertyNameCaseInsensitive = true
                    });
            else
                return null;
        }
    }
}