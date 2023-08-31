using AuctionService;
using AuctionService.Data;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<AuctionDbContext>(e =>
{
  e.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddMassTransit(x =>
{
  x.AddEntityFrameworkOutbox<AuctionDbContext>(o =>
  {
    o.QueryDelay = TimeSpan.FromSeconds(10);
    o.UsePostgres();
    o.UseBusOutbox();
  });

  x.AddConsumersFromNamespaceContaining<AuctionCreatedFaultConsumer>();

  x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("auction", false));

  x.UsingRabbitMq((context, config) =>
  {
    config.ConfigureEndpoints(context);
  });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
  options.Authority = builder.Configuration["IdentityServiceUrl"]; //it tells our resource server who the token has issued by
  options.RequireHttpsMetadata = false;
  options.TokenValidationParameters.ValidateAudience = false;
  options.TokenValidationParameters.NameClaimType = "username";
});

var app = builder.Build();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

try
{
  DbInitializer.InitDb(app);
}
catch (Exception e)
{
  System.Console.WriteLine(e);
}

app.Run();