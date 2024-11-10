namespace Redm_backend.Extensions.ExceptionExtensions
{
	using System.Net;

	using Microsoft.AspNetCore.Diagnostics;
	using Microsoft.AspNetCore.Http;

	using Redm_backend.Models;

	public static class ExceptionMiddlewareExtensions
	{
		public static void ConfigureExceptionHandler(this WebApplication app)
		{
			app.UseExceptionHandler(appError =>
			{
				appError.Run(async context =>
				{
					context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
					context.Response.ContentType = "application/json";

					var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
					if (contextFeature != null)
					{
						var errorDetails = new ErrorDetails()
						{
							StatusCode = context.Response.StatusCode,
							Message = "We are currently not available, please try again later.",
						};

						await context.Response.WriteAsync(errorDetails.ToString());
					}
				});
			});
		}
	}
}
