using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using reCAPTCHAv2.Attributes;
using reCAPTCHAv2.Services;
using reCAPTCHAv2.Test.Models;

namespace reCAPTCHAv2.Test.Controllers
{
    /// <summary>
    /// Sample controller demonstrating reCAPTCHA integration
    /// </summary>
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IRecaptchaService _recaptchaService;

        /// <summary>
        /// Initializes a new instance of the HomeController
        /// </summary>
        /// <param name="logger">Logger for diagnostic information</param>
        /// <param name="recaptchaService">reCAPTCHA service for validation</param>
        public HomeController(ILogger<HomeController> logger, IRecaptchaService recaptchaService)
        {
            _logger = logger;
            _recaptchaService = recaptchaService;
        }

        /// <summary>
        /// Displays the home page with a contact form that includes reCAPTCHA
        /// </summary>
        /// <returns>The view with an empty contact form</returns>
        public IActionResult Index()
        {
            return View(new ContactFormModel());
        }

        /// <summary>
        /// Processes the contact form submission with reCAPTCHA validation
        /// </summary>
        /// <param name="model">The submitted form data</param>
        /// <returns>Redirects to Index on success or returns the view with errors</returns>
        /// <remarks>
        /// The [ValidateRecaptcha] attribute automatically validates the reCAPTCHA response.
        /// If validation fails, the attribute adds an error to ModelState and returns the view.
        /// </remarks>
        [HttpPost]
        [ValidateRecaptcha(ErrorMessage = "Please confirm that you are not a robot.")]
        public async Task<IActionResult> Index(ContactFormModel model)
        {
            // reCAPTCHA validation is now handled by the ValidateRecaptcha attribute
            // which automatically adds errors to ModelState if validation fails

            if (ModelState.IsValid)
            {
                // Process the form (e.g. send email, save to database)
                // Here we just show a success message
                TempData["SuccessMessage"] = "Your message has been sent successfully!";
                return RedirectToAction(nameof(Index));
            }

            // If ModelState is invalid (including reCAPTCHA failures), return to the form
            return View(model);
        }

        /// <summary>
        /// Displays the privacy page
        /// </summary>
        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// Handles errors
        /// </summary>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}