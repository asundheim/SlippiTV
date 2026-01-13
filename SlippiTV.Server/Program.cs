using SlippiTV.Server.Streams;

namespace SlippiTV.Server;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddHttpLogging();
        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddApplicationInsightsTelemetry();
        builder.Services.AddSingleton<StreamManager, StreamManager>();
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpLogging();
        app.UseWebSockets();
        app.UseCors(cors => cors.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
        
        //app.UseHttpsRedirection();
        //app.UseAuthorization();

        app.MapControllers();

        // Separate thread for hosted streams
        _ = Task.Run(async () =>
        {
            HostedStreams hostedStreams = new HostedStreams(app.Services);
            await hostedStreams.BeginHostingAsync();
        });

        app.Run();
    }
}
