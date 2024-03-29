using EventBus.Messages.Common;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Ordering.API.EventBusConsumer;
using Ordering.Application;
using Ordering.Infrastructure;

namespace Ordering.API
{
  public class Startup
  {
    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddApplicationServices();
      services.AddInfrastructureServices(Configuration);

      // Mass Transit for RabbitMQ event queues
      services.AddMassTransit(config =>
      {
        config.AddConsumer<BasketCheckoutConsumer>();
        config.UsingRabbitMq((context, rabbitConfig) =>
        {
          rabbitConfig.Host(Configuration["EventBusSettings:HostAddress"]);
          rabbitConfig.ReceiveEndpoint(EventBusConstants.BasketCheckoutQueue, (c) => 
          {
            c.ConfigureConsumer<BasketCheckoutConsumer>(context);
          });
        });
      });
      // Deprecated in latest; I don't think it is needed.
      // services.AddMassTransitHostedService();

      // General services
      services.AddAutoMapper(typeof(Startup));
      services.AddScoped<BasketCheckoutConsumer>();

      services.AddControllers();
      services.AddSwaggerGen(c =>
      {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Ordering.API", Version = "v1" });
      });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ordering.API v1"));
      }

      app.UseRouting();

      app.UseAuthorization();

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllers();
      });
    }
  }
}
