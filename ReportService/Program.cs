using ReportService.Data;
using Microsoft.EntityFrameworkCore;
using MassTransit;
using ReportService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()
                .WithExposedHeaders("Content-Disposition", "Content-Length")
                .SetIsOriginAllowed(origin => true);
        });
});
builder.Services.AddHostedService<BackgroundWorker>();
builder.Services.AddControllers();
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ReportRequestListenerService>();
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("user");
            h.Password("pass");
        });
        cfg.ReceiveEndpoint("report-queue", e =>
        {
            e.ConfigureConsumer<ReportRequestListenerService>(context);
        });
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHttpClient<ReportRequestListenerService>();

var app = builder.Build();
app.UseHttpsRedirection();
app.UseRouting();

app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();