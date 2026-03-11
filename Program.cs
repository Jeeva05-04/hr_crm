using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using hr_crm.Authorization;
using hr_crm.Data;
using hr_crm.Repositories;
using hr_crm.Repositories.Interface;
using hr_crm.Service;
using hr_crm.Service.Interface;
using hr_crm.Services;

using hr_crm.Authorization;
using Microsoft.AspNetCore.Authorization;

using System.Text.Json.Serialization;
using hr_crm.Mappings;


using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Prevent automatic claim remapping
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

// Services
builder.Services.AddScoped<IAttendanceRepository, AttendanceRepository>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();

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


builder.Services.AddHttpClient();

builder.Services.AddScoped<IPayrollRepository, PayrollRepository>();
builder.Services.AddScoped<IPayrollService, PayrollService>();

builder.Services.AddScoped<ILeaveRepository, LeaveRepository>();
builder.Services.AddScoped<ILeaveService, LeaveService>();

builder.Services.AddScoped<IDigitalSignatureRepository, DigitalSignatureRespository>();
builder.Services.AddScoped<IDigitalSignatureService, DigitalSignatureService>();

builder.Services.AddScoped<IExitInterviewRepository, ExitInterviewRepository>();
builder.Services.AddScoped<IExitInterviewService, ExitInterviewService>();

builder.Services.AddScoped<IOffBoardingRespository, OffBoardingRepository>();
builder.Services.AddScoped<IOffBoardingService, OffBoardingService>();

builder.Services.AddScoped<IEmployeeTrainingRepository, EmployeeTrainingRepository>();
builder.Services.AddScoped<IEmployeeTrainingService, EmployeeTrainingService>();

builder.Services.AddScoped<ILearningRepository, LearningRespository>();
builder.Services.AddScoped<ILearningService, LearningService>();

builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

builder.Services.AddScoped<NotificationRepository>();


builder.Services.AddScoped<NotificationService>();


builder.Services.AddAuthorization();
builder.Services.AddScoped<IEmployeeOnboardingService, EmployeeOnboardingService>();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddAutoMapper(typeof(OnboardingMappingProfile));
builder.Services.AddControllers(options =>
{
    options.Filters.Add<RolePermissionFilter>();
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("HrDb")));

// JWT
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
            OnTokenValidated = context =>
            {
                var identity = context.Principal?.Identity as ClaimsIdentity;
                if (identity == null)
                    return Task.CompletedTask;

                // Map user id claim
                var userId =
                    identity.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                    identity.FindFirst("id")?.Value ??
                    identity.FindFirst("userId")?.Value ??
                    identity.FindFirst("userid")?.Value ??
                    identity.FindFirst("sub")?.Value;

                if (!string.IsNullOrEmpty(userId) &&
                    identity.FindFirst(ClaimTypes.NameIdentifier) == null)
                {
                    identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId));
                }

                // Map role claim
                var existingRoles = identity.FindAll(ClaimTypes.Role).Select(x => x.Value).ToList();

                var roles = identity.FindAll("role").Select(x => x.Value)
                    .Concat(identity.FindAll("roles").Select(x => x.Value))
                    .Distinct()
                    .ToList();

                foreach (var role in roles)
                {
                    if (!existingRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
                    {
                        identity.AddClaim(new Claim(ClaimTypes.Role, role.ToUpper()));
                    }
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("UserAccess", policy =>
        policy.RequireRole("USER", "HR_MANAGER"));

    options.AddPolicy("HrManagerOnly", policy =>
        policy.RequireRole("HR_MANAGER"));
});

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
        Description = "Enter JWT like: Bearer {your token}"
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

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "HR CRM API v1");
    c.RoutePrefix = "swagger";
});

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseStaticFiles();
app.UseCors("AllowFrontend");


 app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseHttpsRedirection();


app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();