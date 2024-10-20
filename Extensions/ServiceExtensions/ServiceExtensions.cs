namespace Redm_backend.Extensions.ServiceExtensions
{
    using System.Text;
    using Azure.Storage.Blobs;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.IdentityModel.Tokens;
    using Microsoft.OpenApi.Models;
    using Redm_backend.Data;
    using Redm_backend.Dtos.PeriodHistory;
    using Redm_backend.Services.AdminService;
    using Redm_backend.Services.CronService;
    using Redm_backend.Services.HomePageService;
    using Redm_backend.Services.PeriodService;
    using Redm_backend.Services.PostService;
    using Redm_backend.Services.ProductService;
    using Redm_backend.Services.StoryService;
    using Redm_backend.Services.SymptomService;
    using Redm_backend.Services.UserService;
    using Swashbuckle.AspNetCore.Filters;

    public static class ServiceExtensions
	{
		public static void ConfigureCors(this IServiceCollection services) => // TODO: restrict CORS only to EXPO requests
			services.AddCors(options =>
			{
				options.AddPolicy("AllowAll", policy =>
				{
					policy.AllowAnyOrigin()
						  .AllowAnyHeader()
						  .AllowAnyMethod();
				});
			});

		public static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
		{
			var connectionString = configuration.GetConnectionString("DefaultConnection");
			services.AddDbContext<DataContext>(options => options.UseNpgsql(connectionString));
		}

		public static void AddBlobServiceClient(this IServiceCollection services, IConfiguration configuration)
		{
			services.AddSingleton(_ => new BlobServiceClient(configuration.GetConnectionString("AzureStorage")));
		}

		public static void AddCustomServices(this IServiceCollection services)
		{
			services.AddScoped<IAuthRepository, AuthRepository>();
			services.AddScoped<ISymptomService, SymptomService>();
			services.AddScoped<IUserService, UserService>();
			services.AddScoped<IPostCategoryService, PostCategoryService>();
			services.AddScoped<IPostService, PostService>();
			services.AddScoped<IStoryService, StoryService>();
			services.AddScoped<IPeriodHistoryService, PeriodHistoryService>();
			services.AddScoped<IHomePageService, HomePageService>();
			services.AddScoped<IProductService, ProductService>();
			services.AddScoped<ICronService, CronService>();
			services.AddAutoMapper(typeof(Program).Assembly);
			services.AddHttpContextAccessor();
		}

		public static void AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
		{
			var key = Encoding.UTF8.GetBytes(configuration.GetSection("AppSettings:Token").Value!);
			services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
				.AddJwtBearer(options =>
				{
					options.TokenValidationParameters = new TokenValidationParameters
					{
						ValidateIssuerSigningKey = true,
						IssuerSigningKey = new SymmetricSecurityKey(key),
						ValidateIssuer = false,
						ValidateAudience = false,
						ValidateLifetime = true,
						ClockSkew = TimeSpan.Zero,
					};
				});
		}

		public static void AddAuthorizationPolicies(this IServiceCollection services)
		{
			services.AddAuthorizationBuilder()
				.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
		}

		public static void ConfigureJsonOptions(this IServiceCollection services)
		{
			services.AddControllers().AddJsonOptions(options =>
			{
				options.JsonSerializerOptions.Converters.Add(new CalendarColorConverter());
				options.JsonSerializerOptions.Converters.Add(new ActionConverter());
			});
		}

		public static void ConfigureSwagger(this IServiceCollection services)
		{
			services.AddEndpointsApiExplorer();
			services.AddSwaggerGen(c =>
			{
				c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
				{
					Description = """Standard Authorization header using the Bearer scheme. Example: "bearer {token}""",
					In = ParameterLocation.Header,
					Name = "Authorization",
					Type = SecuritySchemeType.ApiKey,
				});
				c.OperationFilter<SecurityRequirementsOperationFilter>();
				c.EnableAnnotations();
			});
		}
	}
}
