using ApiApp;
using ApiApp.Data;
using EventTicketing.API;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Services.AddServices(config);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

}

app.UseStaticFiles();
app.UseAuthorization();
app.MapControllers();

app.Services.GetRequiredService<AutoMapper.IMapper>().ConfigurationProvider.AssertConfigurationIsValid(); app.Run();
