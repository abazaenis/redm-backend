namespace Redm_backend.Services.ApiKeyService
{
	using Microsoft.AspNetCore.Mvc;
	using Microsoft.AspNetCore.Mvc.Filters;
	using Microsoft.Extensions.Configuration;

	public class ApiKeyAttribute : Attribute, IAsyncActionFilter
	{
		private const string ApiKeyHeaderName = "X-API-KEY";

		public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
		{
			if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out var potentialApiKey))
			{
				context.Result = new UnauthorizedResult();
				return;
			}

			var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
			var apiKey = configuration.GetSection("AppSettings:ApiKey").Value;

			if (!apiKey!.Equals(potentialApiKey))
			{
				context.Result = new UnauthorizedResult();
				return;
			}

			await next();
		}
	}
}
