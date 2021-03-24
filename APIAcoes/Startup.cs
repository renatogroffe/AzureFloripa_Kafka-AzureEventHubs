using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Confluent.Kafka;
using FluentValidation;
using FluentValidation.AspNetCore;
using HealthChecks.UI.Client;
using APIAcoes.Models;
using APIAcoes.Validators;

namespace APIAcoes
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
                .AddFluentValidation();

            services.AddTransient<IValidator<Acao>, AcaoValidator>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "APIAcoes", Version = "v1" });
            });

            // Configurando a verificação de disponibilidade de diferentes
            // serviços através de Health Checks
            services.AddHealthChecks()
                .AddKafka(new ProducerConfig()
                {
                    BootstrapServers = Configuration["ApacheKafka:Broker"],
                    SecurityProtocol = SecurityProtocol.SaslSsl,
                    SaslMechanism = SaslMechanism.Plain,
                    SaslUsername = Configuration["ApacheKafka:Username"],
                    SaslPassword = Configuration["ApacheKafka:Password"]
                }, name: "kafka-azure", tags: new string[] { "messaging", "azure", "event hub" })
                .AddRedis(Configuration.GetConnectionString("Redis"),
                    name: "redis", tags: new string[] { "db", "cache", "nosql", "data" })
                .AddSqlServer(Configuration.GetConnectionString("SqlServer"),
                    name: "sqlserver", tags: new string[] { "db", "data", "sql" });

            services.AddHealthChecksUI()
                .AddInMemoryStorage();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "APIAcoes v1"));

            // Gera o endpoint que retornará os dados utilizados no dashboard
            app.UseHealthChecks("/healthchecks-data-ui", new HealthCheckOptions()
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            // Ativa o dashboard para a visualização da situação de cada Health Check
            app.UseHealthChecksUI(options =>
            {
                options.UIPath = "/monitor";
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}