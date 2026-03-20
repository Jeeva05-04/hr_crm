using hr_crm.Authorization;
using hr_crm.Service;
using hr_crm.BackgroundServices;
using hr_crm.Data;
using hr_crm.Hubs;
using hr_crm.Mappings;
using hr_crm.Repositories;
using hr_crm.Repositories.Interface;
using hr_crm.Service;
using hr_crm.Service.Interface;
using hr_crm.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();


// =======================
// Services
// =======================

builder.Services.AddScoped<IAttendanceRepository, AttendanceRepository>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddHttpClient<IIpGeolocationService, IpGeolocationService>();

builder.Services.AddScoped<IBranchRepository, BranchRepository>();
builder.Services.AddScoped<IBranchService, BranchService>();

builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();

builder.Services.AddScoped<IKnowledgeRepository, KnowledgeRepository>();
builder.Services.AddScoped<IKnowledgeService, KnowledgeService>();

builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IProjectService, ProjectService>();

builder.Services.AddScoped<IRecruitmentRepository, RecruitmentRepository>();
builder.Services.AddScoped<IRecruitmentService, RecruitmentService>();

builder.Services.AddScoped<IDepartmentBudgetRepository, DepartmentBudgetRepository>();
builder.Services.AddScoped<IDepartmentBudgetService, DepartmentBudgetService>();

builder.Services.AddScoped<IBudgetChangeRequestRepository, BudgetChangeRequestRepository>();
builder.Services.AddScoped<IBudgetChangeRequestService, BudgetChangeRequestService>();

builder.Services.AddScoped<ITodoRepository, TodoRepository>();
builder.Services.AddScoped<ITodoService, TodoService>();

builder.Services.AddScoped<IPayrollRepository, PayrollRepository>();
builder.Services.AddScoped<IPayrollService, PayrollService>();
builder.Services.AddHostedService<PayrollAutoGenerationService>();

builder.Services.AddScoped<ILeaveRepository, LeaveRepository>();
builder.Services.AddScoped<ILeaveService, LeaveService>();

builder.Services.AddScoped<IDigitalSignatureRepository, DigitalSignatureRespository>();
builder.Services.AddScoped<IDigitalSignatureService, DigitalSignatureService>();

builder.Services.AddScoped<ILeadRepository, LeadRepository>();
builder.Services.AddScoped<ILeadService, LeadService>();

builder.Services.AddScoped<IExitInterviewRepository, ExitInterviewRepository>();
builder.Services.AddScoped<IExitInterviewService, ExitInterviewService>();

builder.Services.AddScoped<IOffBoardingRespository, OffBoardingRepository>();
builder.Services.AddScoped<IOffBoardingService, OffBoardingService>();

builder.Services.AddScoped<IEmployeeTrainingRepository, EmployeeTrainingRepository>();
builder.Services.AddScoped<IEmployeeTrainingService, EmployeeTrainingService>();

builder.Services.AddScoped<ILearningRepository, LearningRespository>();
builder.Services.AddScoped<ILearningService, LearningService>();

builder.Services.AddScoped<IEmployeeOnboardingService, EmployeeOnboardingService>();

builder.Services.AddScoped<NotificationRepository>();
builder.Services.AddScoped<NotificationService>();

builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

builder.Services.AddAutoMapper(typeof(OnboardingMappingProfile));

builder.Services.AddHttpClient();


// =======================
// Controllers
// =======================

builder.Services.AddControllers(options =>
{
    options.Filters.Add<RolePermissionFilter>();
});

builder.Services.AddSignalR();


// =======================
// CORS
// =======================

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .WithExposedHeaders("WWW-Authenticate"); // required for SignalR + JWT challenge
    });
});


// =======================
// Database
// =======================

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("HrDb")));

builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("HrDb")), ServiceLifetime.Scoped);


// =======================
// JWT Authentication
// =======================

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],

        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),

        NameClaimType = ClaimTypes.NameIdentifier,
        RoleClaimType = ClaimTypes.Role,

        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        // Allow SignalR to pass token via query string
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine("JWT AUTH FAILED: " + context.Exception.Message);
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            Console.WriteLine("JWT CHALLENGE: " + context.Error + " | " + context.ErrorDescription);
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            var identity = context.Principal?.Identity as ClaimsIdentity;
            if (identity == null)
                return Task.CompletedTask;

            var userId =
                identity.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                identity.FindFirst("sub")?.Value;

            if (!string.IsNullOrEmpty(userId) &&
                identity.FindFirst(ClaimTypes.NameIdentifier) == null)
            {
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId));
            }

            var roles = identity.FindAll("role")
                                .Select(x => x.Value)
                                .Distinct()
                                .ToList();

            foreach (var role in roles)
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, role.ToUpper()));
            }

            return Task.CompletedTask;
        }
    };
});


// =======================
// Authorization
// =======================

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("UserAccess", policy =>
        policy.RequireRole("HR_USER", "HR_MANAGER"));

    options.AddPolicy("HrManagerOnly", policy =>
        policy.RequireRole("HR_MANAGER"));
});


// =======================
// Swagger
// =======================

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token (without Bearer prefix)"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});


// =======================
// App
// =======================

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseStaticFiles();

app.UseCors("AllowFrontend");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHub<LocationHub>("/hubs/location");
app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();
