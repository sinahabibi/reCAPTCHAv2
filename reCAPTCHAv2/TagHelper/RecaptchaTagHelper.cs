using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Configuration;

namespace reCAPTCHAv2.TagHelper
{
    [HtmlTargetElement("recaptcha")]
    public class RecaptchaTagHelper : Microsoft.AspNetCore.Razor.TagHelpers.TagHelper
    {
        private readonly IConfiguration _configuration;

        public RecaptchaTagHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Customization options
        public string Theme { get; set; } = "light"; // light or dark
        public string Size { get; set; } = "normal"; // normal or compact
        public string TabIndex { get; set; } = "0";
        public string Callback { get; set; } = "";
        public string ExpiredCallback { get; set; } = "";
        public string ErrorCallback { get; set; } = "";
        public bool Invisible { get; set; } = false;
        public string Language { get; set; } = "";
        public string Class { get; set; } = ""; // Additional CSS classes

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var siteKey = _configuration["RecaptchaSettings:SiteKey"];

            output.TagName = "div";

            // Add base class and any custom classes
            var cssClass = "g-recaptcha";
            if (!string.IsNullOrEmpty(Class))
                cssClass += $" {Class}";

            output.Attributes.Add("class", cssClass);
            output.Attributes.Add("data-sitekey", siteKey);

            // Add custom attributes if set
            if (!string.IsNullOrEmpty(Theme))
                output.Attributes.Add("data-theme", Theme);

            // Handle size attribute (normal, compact, or invisible)
            if (Invisible)
                output.Attributes.Add("data-size", "invisible");
            else if (!string.IsNullOrEmpty(Size))
                output.Attributes.Add("data-size", Size);

            if (!string.IsNullOrEmpty(TabIndex))
                output.Attributes.Add("data-tabindex", TabIndex);

            if (!string.IsNullOrEmpty(Callback))
                output.Attributes.Add("data-callback", Callback);

            if (!string.IsNullOrEmpty(ExpiredCallback))
                output.Attributes.Add("data-expired-callback", ExpiredCallback);

            if (!string.IsNullOrEmpty(ErrorCallback))
                output.Attributes.Add("data-error-callback", ErrorCallback);

            // Set script with preferred language
            var scriptUrl = "https://www.google.com/recaptcha/api.js";
            if (!string.IsNullOrEmpty(Language))
                scriptUrl += $"?hl={Language}";

            output.PostElement.AppendHtml($"<script src='{scriptUrl}' async defer></script>");
        }
    }
}