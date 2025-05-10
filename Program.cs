
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Reflectly.Entity;
using Reflectly.Models;
using Reflectly.Service;
using Reflectly.Services;
using System.Text;
using Reflectly.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Cấu hình xác thực JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };

    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
          
            var randomClaim = context.Principal?.FindFirst("random")?.Value;

            
            if (string.IsNullOrEmpty(randomClaim))
            {
                context.Fail("Claim 'random' is missing or invalid.");
            }

            return Task.CompletedTask;
        }
    };
});


builder.Services.AddAuthorization(); // Kích hoạt phân quyền

// Cấu hình CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Đăng ký các dịch vụ & repository
builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection("ReflectlyDatabase"));
builder.Services.AddSingleton<CRUD_Service<Account>>();
builder.Services.AddSingleton<TokenService>();
builder.Services.AddSingleton<Account_Service>();
builder.Services.AddSingleton<CRUD_Service<MoodCheckin>>();
builder.Services.AddSingleton<CRUD_Service<Photo>>();
builder.Services.AddSingleton<CRUD_Service<VoiceNote>>();
builder.Services.AddTransient<IEmailService, EmailService>();
builder.Services.AddSingleton<CRUD_Service<Quote>>();
builder.Services.AddSingleton<CRUD_Service<UserHeart>>();
builder.Services.AddSingleton<CRUD_Service<Challenge>>();
builder.Services.AddSingleton<CRUD_Service<UserChallenge>>();
builder.Services.AddSingleton<CRUD_Service<ChallengeCategory>>();
builder.Services.AddSingleton<Reflection_Service>();
builder.Services.AddSingleton<UserReflection_Service>();
builder.Services.AddSingleton<Quote_Service>();
builder.Services.AddSingleton<CRUD_Service<Activity>>();
builder.Services.AddSingleton<CRUD_Service<Feeling>>();
builder.Services.AddSingleton<Media_Service>();
builder.Services.AddHostedService<CleanService>();



// Thêm dịch vụ API Controller
builder.Services.AddControllers();

// Cấu hình Swagger (OpenAPI)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseAuthentication(); // Sử dụng xác thực
app.UseAuthorization(); // Sử dụng phân quyền
app.UseMiddleware<DeviceValidationMiddleware>();
app.UseCors("AllowSpecificOrigins"); // Kích hoạt CORS

// Cấu hình pipeline xử lý yêu cầu HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); // Hiển thị Swagger UI khi phát triển
    app.UseSwaggerUI();
}

app.UseHttpsRedirection(); // Chuyển hướng HTTPS
app.UseAuthorization(); // Xác thực & phân quyền

app.MapControllers(); // Định tuyến cho các controller




app.Run(); // Chạy ứng dụng
