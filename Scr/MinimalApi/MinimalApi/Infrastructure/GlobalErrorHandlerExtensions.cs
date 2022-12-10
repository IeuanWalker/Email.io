using Domain.Exceptions;
using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;

namespace Api.Infrastructure;

class ExceptionHandler { }


/// <summary>
/// extensions for global exception handling
/// </summary>
public static class GlobalErrorHandlerExtensions
{
	/// <summary>
	/// registers the default global exception handler which will log the exceptions on the server and return a user-friendly json response to the client when unhandled exceptions occur.
	/// TIP: when using this exception handler, you may want to turn off the asp.net core exception middleware logging to avoid duplication like so:
	/// <code>
	/// "Logging": { "LogLevel": { "Default": "Warning", "Microsoft.AspNetCore.Diagnostics.ExceptionHandlerMiddleware": "None" } }
	/// </code>
	/// </summary>
	/// <param name="app"></param>
	/// <param name="logger">an optional logger instance</param>
	public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app, ILogger? logger = null)
	{
		app.UseExceptionHandler(errApp =>
		{
			errApp.Run(async ctx =>
			{
				var exHandlerFeature = ctx.Features.Get<IExceptionHandlerFeature>();
				if(exHandlerFeature is null)
				{
					return;
				}
				
				logger ??= ctx.Resolve<ILogger<ExceptionHandler>>();
				logger.LogError(
					"{@http}{@type}{@reason}{@exception}",
					exHandlerFeature.Endpoint?.DisplayName?.Split(" => ")[0],
					exHandlerFeature.Error.GetType().Name,
					exHandlerFeature.Error.Message,
					exHandlerFeature.Error);

				ctx.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
				string status = "Error";
				string reason = "Unknown";
				string note = "See application log for stack trace.";

				// Custom internal exception to throw specific status codes
				if (exHandlerFeature.Error.GetType() == typeof(RequestHandleException))
				{
					var ex = (RequestHandleException)exHandlerFeature.Error;
					ctx.Response.StatusCode = (int)ex.HttpStatusCode;
					reason = ex.Reason;
					note = ex.Note;
				}

				try
				{
					status = ((HttpStatusCode)ctx.Response.StatusCode).ToString();
				}
				catch
				{
					// ignored
				}

				ctx.Response.ContentType = "application/problem+json";
				await ctx.Response.WriteAsJsonAsync(new InternalErrorResponse
				{
					Status = status,
					Code = ctx.Response.StatusCode,
					Reason = reason,
					Note = note
				});
			});
		});

		return app;
	}
}