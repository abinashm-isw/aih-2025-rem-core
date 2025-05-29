using Microsoft.EntityFrameworkCore;
using Oracle.EntityFrameworkCore.Extensions;
using RemCoreApi.Data;
using RemCoreApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure Oracle Database
builder.Services.AddDbContext<OracleDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("OracleConnection") 
        ?? throw new InvalidOperationException("Oracle connection string not found in configuration.");
    
    // Debug logging
    Console.WriteLine($"Using connection string: {connectionString}");
    
    options.UseOracle(connectionString);
});

// Register services
builder.Services.AddScoped<IContractService, ContractService>();

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "REM Core API", 
        Version = "v1",
        Description = "Real Estate Management Core API for Contract Management"
    });
    
    // Enable XML comments for better documentation
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Configure CORS for future MCP server integration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMCP", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "REM Core API v1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at app's root
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowMCP");
app.UseAuthorization();
app.MapControllers();

app.Run();
