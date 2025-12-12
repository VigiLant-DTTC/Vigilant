using System.Globalization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using MQTTnet;
using MQTTnet.Client;
using VigiLant.Config;
using VigiLant.Contratos;
using VigiLant.Data;
using VigiLant.Hubs;
using VigiLant.Repository;
using VigiLant.Repository;
using VigiLant.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddControllers();
string mySqlConnection = builder.Configuration.GetConnectionString("DefaultDatabase");
builder.Services.AddDbContext<BancoCtx>(opt =>
{
    opt.UseMySql(mySqlConnection, ServerVersion.AutoDetect(mySqlConnection));
});


//Config
builder.Services.AddScoped<IAppConfigRepository, AppConfigRepository>();

// Repositorios e contratos
builder.Services.AddScoped<IRiscoRepository, RiscoRepository>();
builder.Services.AddScoped<IEquipamentoRepository, EquipamentoRepository>();
builder.Services.AddScoped<IColaboradorRepository, ColaboradorRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IRelatorioRepository, RelatorioRepository>();

// 4. Adicionar o serviço SignalR e o Hub
builder.Services.AddSignalR();

// Servicos
builder.Services.AddSingleton<IHashService, HashService>();
builder.Services.AddSingleton<MqttClientService>();
builder.Services.AddSingleton<IHostedService>(provider => provider.GetRequiredService<MqttClientService>());
builder.Services.AddSingleton<IMqttService>(provider => provider.GetRequiredService<MqttClientService>()); 


// Registra a Factory e o Cliente MQTT como Singleton (devem ser persistentes)
builder.Services.AddSingleton<MqttFactory>();
builder.Services.AddSingleton<IMqttClient>(sp => 
{
    var factory = sp.GetRequiredService<MqttFactory>();
    return factory.CreateMqttClient();
});


//Cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Conta/Login"; // Redireciona usuários não autenticados
        options.AccessDeniedPath = "/Conta/AcessoNegado"; // Redireciona usuários sem permissão (Role)
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await SeedData.Initialize(services); 
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocorreu um erro durante o seeding do DB.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication(); 
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    // Mapeamento do SignalR Hub
    endpoints.MapHub<MedicaoHub>("/medicaoHub");

    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});

app.Run();
