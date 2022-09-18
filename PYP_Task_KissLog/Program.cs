using KissLog;
using KissLog.AspNetCore;
using KissLog.CloudListeners.Auth;
using KissLog.CloudListeners.RequestLogsListener;
using KissLog.Formatters;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();


// Add HttpContextAccessor 
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IKLogger>((provider) => Logger.Factory.Get());

builder.Services.AddLogging(logging =>
{
    logging.AddKissLog(options =>
    {
        options.Formatter = (FormatterArgs args) =>
        {
            if (args.Exception == null)
                return args.DefaultValue;

            string exceptionStr = new ExceptionFormatter().Format(args.Exception, args.Logger);

            return string.Join(Environment.NewLine, new[] { args.DefaultValue, exceptionStr });
        };
    });
});


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseKissLogMiddleware(options => ConfigureKissLog(options));

void ConfigureKissLog(IOptionsBuilder options)
{
    KissLogConfiguration.Listeners.Add(new RequestLogsApiListener(new Application(
        builder.Configuration["KissLog.OrganizationId"],    //  "55fe432e-58f4-4910-beba-e1f5c78b45c2"
        builder.Configuration["KissLog.ApplicationId"])     //  "76c582e9-e1b6-4e8e-ade4-5c5af67d3c92"
    )
    {
        ApiUrl = builder.Configuration["KissLog.ApiUrl"]    //  "https://api.kisslog.net"
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
