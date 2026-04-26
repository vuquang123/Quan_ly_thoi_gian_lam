using FaceIDHRM.Server.Hubs;
using FaceIDHRM.Server.Repositories;
using FaceIDHRM.Server.Repositories.Workforce;
using FaceIDHRM.Server.Services;
using FaceIDHRM.Server.Services.Workforce;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllLan", policy =>
    {
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .SetIsOriginAllowed(_ => true)
            .AllowCredentials();
    });
});

builder.Services.AddSingleton<IEarlyCheckoutRequestRepository, JsonEarlyCheckoutRequestRepository>();
builder.Services.AddSingleton<IEarlyCheckoutApprovalService, EarlyCheckoutApprovalService>();
builder.Services.AddSingleton<IEmployeeRepository, JsonEmployeeRepository>();
builder.Services.AddSingleton<IAttendanceRepository, JsonAttendanceRepository>();
builder.Services.AddSingleton<IEmployeeService, EmployeeService>();
builder.Services.AddSingleton<IAttendanceService, AttendanceService>();

var app = builder.Build();

app.Urls.Add("http://0.0.0.0:5055");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowAllLan");
app.UseHttpsRedirection();

app.MapControllers();
app.MapHub<EarlyCheckoutHub>("/hubs/early-checkout");

app.Run();
