namespace Redm_backend
{
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

	public static class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Database Connection
			var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
			builder.Services.AddDbContext<DataContext>(options => options.UseNpgsql(connectionString));

			// Add services to the container.
			builder.Services.AddControllers()
				.AddJsonOptions(options =>
				{
					options.JsonSerializerOptions.Converters.Add(new CalendarColorConverter());
					options.JsonSerializerOptions.Converters.Add(new ActionConverter());
				});

			// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen(c =>
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
			builder.Services.AddAutoMapper(typeof(Program).Assembly);
			builder.Services.AddScoped<IAuthRepository, AuthRepository>();
			builder.Services.AddScoped<ISymptomService, SymptomService>();
			builder.Services.AddScoped<IUserService, UserService>();
			builder.Services.AddScoped<IPostCategoryService, PostCategoryService>();
			builder.Services.AddScoped<IPostService, PostService>();
			builder.Services.AddScoped<IStoryService, StoryService>();
			builder.Services.AddScoped<IPeriodHistoryService, PeriodHistoryService>();
			builder.Services.AddScoped<IHomePageService, HomePageService>();
			builder.Services.AddScoped<IProductService, ProductService>();
			builder.Services.AddScoped<ICronService, CronService>();
			builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
				.AddJwtBearer(options =>
				{
					options.TokenValidationParameters = new TokenValidationParameters
					{
						ValidateIssuerSigningKey = true,
						IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(builder.Configuration.GetSection("AppSettings:Token").Value!)),
						ValidateIssuer = false,
						ValidateAudience = false,
						ValidateLifetime = true,
						ClockSkew = TimeSpan.Zero,
					};
				});
			builder.Services.AddAuthorization(options =>
			{
				options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
			});
			builder.Services.AddHttpContextAccessor();

			builder.Services.AddCors(options =>
			{
				options.AddPolicy("AllowAll", policy =>
				{
					policy.AllowAnyOrigin()
						  .AllowAnyHeader()
						  .AllowAnyMethod();
				});
			});

			var app = builder.Build();

			app.UseSwagger();
			app.UseSwaggerUI();

			app.MapGet("/current-time", () =>
			{
				return Results.Ok($"Current server time: {Convert.ToString(DateTime.UtcNow)}");
			});

			app.UseSwagger();
			app.UseSwaggerUI();

			app.UseCors("AllowAll");

			app.UseAuthorization();

			app.MapControllers();

			app.Run();
		}
	}
}