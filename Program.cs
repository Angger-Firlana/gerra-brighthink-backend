using backend.Data;
using Microsoft.EntityFrameworkCore;
using backend.Features.User;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using backend.Middleware;
using backend.Features.Auth;
using backend.Helpers;
using backend.Features.Task;
using backend.Features.TaskCategory;
using backend.Features.Goal;
using backend.Features.Habit;
using backend.Features.Role;
using backend.Features.ActivityLog;

var builder = WebApplication.CreateBuilder(args);

//Add Auth
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "https://brighthink.my.id", "http://brighthink.my.id", "http://43.157.241.50", "https://43.157.241.50")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c=>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Gerra ToDo List API",
        Version = "v1"
    });
    c.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = JwtBearerDefaults.AuthenticationScheme
                    }
                },
                Array.Empty<string>()
            }
        }
    );
});


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), new MySqlServerVersion(new Version(8, 0, 33)))
);
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

//Register Helpers
builder.Services.AddScoped<GenerateToken>();

// Register Services
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITaskCategoryService, TaskCategoryService>();
builder.Services.AddScoped<IGoalService, GoalService>();
builder.Services.AddScoped<IHabitService, HabitService>();
builder.Services.AddScoped<IActivityLogService, ActivityLogService>();
builder.Services.AddScoped<IRoleService, RoleService>();

var app = builder.Build();

// 1. Aktifkan Routing duluan biar .NET tahu arah endpoint-nya
app.UseRouting();

// 2. CORS WAJIB setelah UseRouting dan sebelum Auth/Middleware lain
app.UseCors("AllowSpecificOrigins");

// 3. Jalankan Custom Middleware lu
app.UseMiddleware<JwtMiddleware>();

// 4. Aktifkan Authentication & Authorization bawaan .NET (Wajib kalau pake JWT)
app.UseAuthentication();
app.UseAuthorization();

// 5. Swagger dipaksa aktif di Production biar lu bisa test
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Gerra ToDo List API V1");
    c.RoutePrefix = "swagger"; 
});

app.MapControllers();

app.Run();
