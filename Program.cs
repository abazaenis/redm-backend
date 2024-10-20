namespace Redm_backend
{
	using Microsoft.Extensions.Azure;
	using Microsoft.Extensions.DependencyInjection;
	using Redm_backend.Extensions.ServiceExtensions;

	public static class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Configure Services
			builder.Services.AddDatabase(builder.Configuration);
			builder.Services.AddBlobServiceClient(builder.Configuration);
			builder.Services.AddCustomServices();
			builder.Services.ConfigureJsonOptions();
			builder.Services.AddJwtAuthentication(builder.Configuration);
			builder.Services.AddAuthorizationPolicies();
			builder.Services.ConfigureSwagger();
			builder.Services.ConfigureCors();

			var app = builder.Build();

			// Configure Middleware Pipeline
			app.UseSwagger();
			app.UseSwaggerUI();
			app.UseCors("AllowAll");
			app.UseAuthentication();
			app.UseAuthorization();
			app.MapControllers();

			app.Run();
		}
	}
}