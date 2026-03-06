using CompraAutomatizada.Infrastructure;
using CompraAutomatizada.Worker.Extensions;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddCompraQuartz();

var host = builder.Build();
host.Run();
