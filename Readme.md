# reCAPTCHAv2 for .NET 8/9

*Read this in: [English](#english) | [فارسی](#persian)*

<a name="english"></a>
## English Documentation

A simple, lightweight library for integrating Google reCAPTCHA v2 into ASP.NET Core applications.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

### Features

- Easy integration with Razor Pages and MVC applications
- Advanced customization options for reCAPTCHA appearance
- Simple validation with ActionFilter attributes
- Comprehensive error reporting
- Test mode for development environments
- Extensive documentation and examples

### Installation

#### 1. Add the reCAPTCHAv2 package to your project

```sh
dotnet add package reCAPTCHAv2-Net8
```

#### 2. Register services in Program.cs

```csharp
// Add reCAPTCHA services
builder.Services.AddHttpClient<IRecaptchaService, RecaptchaService>();
builder.Services.AddScoped<IRecaptchaService, RecaptchaService>();
```

### Configuration

#### 1. Add reCAPTCHA settings to appsettings.json

```json
{
  "RecaptchaSettings": {
    "SiteKey": "YOUR_SITE_KEY",
    "SecretKey": "YOUR_SECRET_KEY"
  }
}
```

To obtain these keys:
1. Visit [Google reCAPTCHA Admin](https://www.google.com/recaptcha/admin)
2. Register a new site with reCAPTCHA v2 ("I'm not a robot" Checkbox)
3. Add your domains and get your Site Key and Secret Key

### Basic Usage

#### In Razor Pages

##### 1. Add Tag Helper in _ViewImports.cshtml

```razor
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
@addTagHelper *, reCAPTCHAv2
```

##### 2. Add reCAPTCHA to your form

```razor
<form method="post">
    <!-- Your form fields -->
    
    <div class="mb-3">
        <recaptcha />
        <span asp-validation-summary="All" class="text-danger"></span>
    </div>
    
    <button type="submit" class="btn btn-primary">Submit</button>
</form>
```

##### 3. Validate in PageModel handler

```csharp
using reCAPTCHAv2.Services;
using reCAPTCHAv2.Attributes;

public class ContactModel : PageModel
{
    [BindProperty]
    public ContactForm Contact { get; set; }
    
    [ValidateRecaptcha(ErrorMessage = "Please verify that you are not a robot.")]
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }
        
        // Process the form data...
        
        return RedirectToPage("ThankYou");
    }
}
```

#### In MVC

##### 1. Add reCAPTCHA to your view

```razor
@{
    ViewData["Title"] = "Contact";
}

<h1>Contact Us</h1>

<form asp-action="Contact" method="post">
    <!-- Form fields -->
    
    <div class="form-group">
        <recaptcha theme="light" size="normal" language="en" />
        @Html.ValidationMessage(string.Empty)
    </div>
    
    <button type="submit" class="btn btn-primary">Submit</button>
</form>
```

##### 2. Use the ValidateRecaptcha attribute in your controller

```csharp
using reCAPTCHAv2.Attributes;

[HttpPost]
[ValidateRecaptcha(ErrorMessage = "Please verify that you are not a robot.")]
public IActionResult Contact(ContactViewModel model)
{
    if (!ModelState.IsValid)
        return View(model);
        
    // Process form...
    return RedirectToAction("ThankYou");
}
```

### Advanced Configuration

#### Customizing reCAPTCHA Appearance

The `<recaptcha>` tag helper supports various customization options:

```razor
<recaptcha 
    theme="dark" 
    size="compact" 
    language="fr" 
    tab-index="5"
    callback="onRecaptchaSuccess"
    expired-callback="onRecaptchaExpired"
    error-callback="onRecaptchaError"
    class="my-custom-class" />
```

Available options:
- `theme`: "light" (default) or "dark"
- `size`: "normal" (default) or "compact"
- `language`: Two-letter language code (e.g., "en", "fr", "es")
- `tab-index`: Tab index for the widget
- `callback`: JavaScript function name to call on successful completion
- `expired-callback`: Function name to call when reCAPTCHA expires
- `error-callback`: Function name to call on error
- `invisible`: Set to "true" for invisible reCAPTCHA
- `class`: Additional CSS classes

#### Test Mode

For development and testing without making actual API calls:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Register services
    services.AddHttpClient<IRecaptchaService, RecaptchaService>();
    services.AddScoped<IRecaptchaService, RecaptchaService>(sp => {
        var service = new RecaptchaService(
            sp.GetRequiredService<IConfiguration>(),
            sp.GetRequiredService<HttpClient>(),
            sp.GetService<ILogger<RecaptchaService>>());
        
        // Enable test mode in development environment
        if (Environment.IsDevelopment())
        {
            service.TestMode = true;
            service.TestModeResult = true; // Set to false to simulate validation failures
        }
        
        return service;
    });
}
```

#### Manual Validation (Without Attribute)

If you need more control over the validation process:

```csharp
public class ContactModel : PageModel
{
    private readonly IRecaptchaService _recaptchaService;
    
    public ContactModel(IRecaptchaService recaptchaService)
    {
        _recaptchaService = recaptchaService;
    }
    
    public async Task<IActionResult> OnPostAsync()
    {
        // Get reCAPTCHA response from form
        var recaptchaResponse = Request.Form["g-recaptcha-response"].ToString();
        
        // Validate with detailed error information
        var (success, errorMessage) = await _recaptchaService.ValidateRecaptchaWithDetailsAsync(recaptchaResponse);
        
        if (!success)
        {
            ModelState.AddModelError(string.Empty, errorMessage);
            return Page();
        }
        
        // Process form...
        return RedirectToPage("ThankYou");
    }
}
```

### Troubleshooting

#### Common Issues

1. **reCAPTCHA validation always fails**
   - Check if your secret key is correctly configured in appsettings.json
   - Verify that your domain is registered in the Google reCAPTCHA Admin Console

2. **"Invalid domain for site key" error**
   - During development, make sure to add 'localhost' to your allowed domains in Google reCAPTCHA Admin Console

3. **reCAPTCHA is not displaying**
   - Verify that your site key is correct
   - Check for JavaScript errors in the browser console

4. **Error messages not displaying in form**
   - Ensure you have a validation summary or message element in your form

#### Debugging

For detailed validation information, use the `ValidateRecaptchaWithDetailsAsync` method which returns specific error messages from Google's API.

### License

This project is licensed under the MIT License - see the LICENSE file for details.

### Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

---

<a name="persian"></a>
## مستندات فارسی

کتابخانه‌ای ساده و سبک برای یکپارچه‌سازی Google reCAPTCHA v2 در برنامه‌های ASP.NET Core.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

### ویژگی‌ها

- یکپارچه‌سازی آسان با برنامه‌های Razor Pages و MVC
- گزینه‌های سفارشی‌سازی پیشرفته برای ظاهر reCAPTCHA
- اعتبارسنجی ساده با استفاده از ویژگی‌های ActionFilter
- گزارش خطای جامع
- حالت آزمایشی برای محیط‌های توسعه
- مستندات و نمونه‌های گسترده

### نصب

#### ۱. افزودن بسته reCAPTCHAv2 به پروژه شما

```sh
dotnet add package reCAPTCHAv2-Net8
```

#### ۲. ثبت سرویس‌ها در Program.cs

```csharp
// Add reCAPTCHA services
builder.Services.AddHttpClient<IRecaptchaService, RecaptchaService>();
builder.Services.AddScoped<IRecaptchaService, RecaptchaService>();
```

### پیکربندی

#### ۱. افزودن تنظیمات reCAPTCHA به appsettings.json

```json
{
  "RecaptchaSettings": {
    "SiteKey": "YOUR_SITE_KEY",
    "SecretKey": "YOUR_SECRET_KEY"
  }
}
```

برای دریافت این کلیدها:
1. به [پنل مدیریت Google reCAPTCHA](https://www.google.com/recaptcha/admin) مراجعه کنید
2. یک سایت جدید با reCAPTCHA v2 ("من ربات نیستم" چک باکس) ثبت کنید
3. دامنه‌های خود را اضافه کنید و Site Key و Secret Key خود را دریافت کنید

### استفاده پایه

#### در Razor Pages

##### ۱. افزودن Tag Helper در _ViewImports.cshtml

```razor
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
@addTagHelper *, reCAPTCHAv2
```

##### ۲. افزودن reCAPTCHA به فرم خود

```razor
<form method="post">
    <!-- فیلدهای فرم شما -->
    
    <div class="mb-3">
        <recaptcha />
        <span asp-validation-summary="All" class="text-danger"></span>
    </div>
    
    <button type="submit" class="btn btn-primary">ارسال</button>
</form>
```

##### ۳. اعتبارسنجی در PageModel handler

```csharp
using reCAPTCHAv2.Services;
using reCAPTCHAv2.Attributes;

public class ContactModel : PageModel
{
    [BindProperty]
    public ContactForm Contact { get; set; }
    
    [ValidateRecaptcha(ErrorMessage = "لطفاً تأیید کنید که ربات نیستید.")]
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }
        
        // پردازش داده‌های فرم...
        
        return RedirectToPage("ThankYou");
    }
}
```

#### در MVC

##### ۱. افزودن reCAPTCHA به view خود

```razor
@{
    ViewData["Title"] = "تماس با ما";
}

<h1>تماس با ما</h1>

<form asp-action="Contact" method="post">
    <!-- فیلدهای فرم -->
    
    <div class="form-group">
        <recaptcha theme="light" size="normal" language="fa" />
        @Html.ValidationMessage(string.Empty)
    </div>
    
    <button type="submit" class="btn btn-primary">ارسال</button>
</form>
```

##### ۲. استفاده از ویژگی ValidateRecaptcha در کنترلر خود

```csharp
using reCAPTCHAv2.Attributes;

[HttpPost]
[ValidateRecaptcha(ErrorMessage = "لطفاً تأیید کنید که ربات نیستید.")]
public IActionResult Contact(ContactViewModel model)
{
    if (!ModelState.IsValid)
        return View(model);
        
    // پردازش فرم...
    return RedirectToAction("ThankYou");
}
```

### پیکربندی پیشرفته

#### سفارشی‌سازی ظاهر reCAPTCHA

Tag helper ‎`<recaptcha>` از گزینه‌های سفارشی‌سازی مختلفی پشتیبانی می‌کند:

```razor
<recaptcha 
    theme="dark" 
    size="compact" 
    language="fa" 
    tab-index="5"
    callback="onRecaptchaSuccess"
    expired-callback="onRecaptchaExpired"
    error-callback="onRecaptchaError"
    class="my-custom-class" />
```

گزینه‌های موجود:
- `theme`: "light" (پیش‌فرض) یا "dark"
- `size`: "normal" (پیش‌فرض) یا "compact"
- `language`: کد زبان دو حرفی (مانند "fa"، "en"، "ar")
- `tab-index`: Tab index برای ویجت
- `callback`: نام تابع جاوااسکریپت برای فراخوانی در صورت تکمیل موفق
- `expired-callback`: نام تابع برای فراخوانی هنگامی که reCAPTCHA منقضی می‌شود
- `error-callback`: نام تابع برای فراخوانی در صورت خطا
- `invisible`: برای reCAPTCHA نامرئی به "true" تنظیم کنید
- `class`: کلاس‌های CSS اضافی

#### حالت آزمایشی

برای توسعه و آزمایش بدون انجام فراخوانی‌های واقعی API:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // ثبت سرویس‌ها
    services.AddHttpClient<IRecaptchaService, RecaptchaService>();
    services.AddScoped<IRecaptchaService, RecaptchaService>(sp => {
        var service = new RecaptchaService(
            sp.GetRequiredService<IConfiguration>(),
            sp.GetRequiredService<HttpClient>(),
            sp.GetService<ILogger<RecaptchaService>>());
        
        // فعال‌سازی حالت آزمایشی در محیط توسعه
        if (Environment.IsDevelopment())
        {
            service.TestMode = true;
            service.TestModeResult = true; // برای شبیه‌سازی شکست‌های اعتبارسنجی، به false تنظیم کنید
        }
        
        return service;
    });
}
```

#### اعتبارسنجی دستی (بدون ویژگی)

اگر به کنترل بیشتری بر فرآیند اعتبارسنجی نیاز دارید:

```csharp
public class ContactModel : PageModel
{
    private readonly IRecaptchaService _recaptchaService;
    
    public ContactModel(IRecaptchaService recaptchaService)
    {
        _recaptchaService = recaptchaService;
    }
    
    public async Task<IActionResult> OnPostAsync()
    {
        // دریافت پاسخ reCAPTCHA از فرم
        var recaptchaResponse = Request.Form["g-recaptcha-response"].ToString();
        
        // اعتبارسنجی با اطلاعات خطای دقیق
        var (success, errorMessage) = await _recaptchaService.ValidateRecaptchaWithDetailsAsync(recaptchaResponse);
        
        if (!success)
        {
            ModelState.AddModelError(string.Empty, errorMessage);
            return Page();
        }
        
        // پردازش فرم...
        return RedirectToPage("ThankYou");
    }
}
```

### عیب‌یابی

#### مشکلات رایج

1. **اعتبارسنجی reCAPTCHA همیشه با شکست مواجه می‌شود**
   - بررسی کنید که کلید مخفی شما به درستی در appsettings.json پیکربندی شده باشد
   - تأیید کنید که دامنه شما در کنسول مدیریت Google reCAPTCHA ثبت شده است

2. **خطای "دامنه نامعتبر برای کلید سایت"**
   - در حین توسعه، مطمئن شوید که 'localhost' را به دامنه‌های مجاز خود در کنسول مدیریت Google reCAPTCHA اضافه کرده‌اید

3. **reCAPTCHA نمایش داده نمی‌شود**
   - تأیید کنید که کلید سایت شما صحیح است
   - خطاهای جاوااسکریپت را در کنسول مرورگر بررسی کنید

4. **پیام‌های خطا در فرم نمایش داده نمی‌شوند**
   - اطمینان حاصل کنید که یک خلاصه اعتبارسنجی یا عنصر پیام در فرم خود دارید

#### اشکال‌زدایی

برای اطلاعات اعتبارسنجی دقیق، از متد `ValidateRecaptchaWithDetailsAsync` استفاده کنید که پیام‌های خطای خاصی را از API گوگل برمی‌گرداند.

### مجوز

این پروژه تحت مجوز MIT است - برای جزئیات به فایل LICENSE مراجعه کنید.

### مشارکت

مشارکت‌ها مورد استقبال قرار می‌گیرند! لطفاً برای ارسال Pull Request اقدام کنید.