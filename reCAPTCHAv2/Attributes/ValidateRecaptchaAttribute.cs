using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using reCAPTCHAv2.Services;

namespace reCAPTCHAv2.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ValidateRecaptchaAttribute : ActionFilterAttribute
    {
        public string ErrorMessage { get; set; } = "Please verify that you are not a robot.";

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var recaptchaService = context.HttpContext.RequestServices.GetRequiredService<IRecaptchaService>();

            if (context.HttpContext.Request.HasFormContentType)
            {
                var recaptchaResponse = context.HttpContext.Request.Form["g-recaptcha-response"].ToString();
                var isValid = await recaptchaService.ValidateRecaptchaAsync(recaptchaResponse);

                if (!isValid)
                {
                    context.ModelState.AddModelError(string.Empty, ErrorMessage);

                    // Check if it's an API request
                    bool isApiRequest = context.HttpContext.Request.Headers
                        .TryGetValue("Accept", out var acceptValues) &&
                        acceptValues.Any(x => x?.Contains("application/json") == true);

                    if (isApiRequest)
                    {
                        // API response with BadRequest status
                        context.Result = new BadRequestObjectResult(context.ModelState);
                    }
                    else
                    {
                        // For MVC and Razor Pages, extract the model and return it with errors
                        // The key is to preserve model state by returning the same action
                        if (context.ActionArguments.Count > 0)
                        {
                            var model = context.ActionArguments.Values.FirstOrDefault();
                            string actionName = context.ActionDescriptor.RouteValues["action"];

                            // Use PageResult for Razor Pages or try ViewResult for MVC
                            // This approach avoids referencing specific types that might not be available
                            try
                            {
                                // Try MVC approach
                                var controllerType = Type.GetType("Microsoft.AspNetCore.Mvc.Controller, Microsoft.AspNetCore.Mvc.ViewFeatures");
                                if (controllerType != null && controllerType.IsInstanceOfType(context.Controller))
                                {
                                    // MVC approach: Use Controller.View() via reflection
                                    var viewMethod = controllerType.GetMethod("View", new[] { typeof(string), typeof(object) });
                                    if (viewMethod != null)
                                    {
                                        var result = viewMethod.Invoke(context.Controller, new[] { actionName, model });
                                        if (result != null)
                                        {
                                            context.Result = (IActionResult)result;
                                            return;
                                        }
                                    }
                                }
                            }
                            catch
                            {
                                // Fall back to simple redirect if reflection fails
                            }

                            // If we get here, just redirect back to the same URL
                            // Use HttpContext.Items for request-specific data instead of TempData
                            context.HttpContext.Items["RecaptchaValidationFailed"] = "true";

                            // We can't use TempData directly, so just redirect with the model state errors
                            context.Result = new RedirectResult(context.HttpContext.Request.Path.ToString());
                        }
                        else
                        {
                            // Fallback for when no model is available
                            context.Result = new BadRequestObjectResult(context.ModelState);
                        }
                    }
                    return;
                }
            }

            await next();
        }
    }
}