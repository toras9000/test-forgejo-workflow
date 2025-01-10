#!/usr/bin/env dotnet-script
#r "nuget: AngleSharp, 1.2.0"
#r "nuget: Lestaly, 0.69.0"
#r "nuget: Kokuban, 0.2.0"
#nullable enable
using System.Net;
using System.Threading;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Io;
using Kokuban;
using Lestaly;
using Lestaly.Cx;

var settings = new
{
    // Service URL
    ServiceURL = "https://forgejo.myserver.home",

    // Initial Startup Setup Configuration
    Setup = new
    {
        // Admin user name at setup
        AdminUser = "forgejo-admin",
        // Admin password at setup
        AdminPass = "forgejo-admin-pass",
        // Admin email address at setup
        AdminMail = "forgejo-admin@example.com",
    },
};

await Paved.RunAsync(config: c => c.AnyPause(), action: async () =>
{
    // Show service link
    WriteLine();
    WriteLine("Service URL");
    WriteLine($"  {Poster.Link[settings.ServiceURL]}");
    WriteLine();

    // Determine initial startup (whether the setup form is displayed)
    WriteLine();
    WriteLine("Check initialization status ...");
    var requester = new DefaultHttpRequester();
    requester.Timeout = TimeSpan.FromSeconds(3 * 60);
    var config = Configuration.Default.With(requester).WithDefaultLoader();
    var context = BrowsingContext.New(config);
    var document = default(IDocument);
    using (var breaker = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
    {
        while (document == null || document.Source.Length <= 0)
        {
            document = await context.OpenAsync(settings.ServiceURL, breaker.Token);
            if (document.StatusCode != HttpStatusCode.OK) throw new PavedMessageException("Cannot load page", PavedMessageKind.Warning);
        }
    }
    var container = document.QuerySelector<IHtmlDivElement>(".install-config-container");
    if (container == null)
    {
        // If the initial startup screen is not detected, it is assumed that the setup has already been done, so the process is completed.
        WriteLine($"  {Chalk.Green["The instance has already been initialized."]}");
        return;
    }

    // If you get a page that looks like the first startup, the setup process continues.
    WriteLine("  Detected that initial setup is required.");

    // Attempt to retrieve setup form
    WriteLine("Perform initial setup ...");
    var forms = container.Descendants<IHtmlFormElement>().ToArray();
    if (forms.Length != 1) throw new PavedMessageException("Unexpected setup form");
    var serupForm = forms[0];

    // Change settings
    var inputUpdateChecker = serupForm.QuerySelectorAll<IHtmlInputElement>("*").FirstOrDefault(e => e.Type == "checkbox" && e.Name == "enable_update_checker");
    if (inputUpdateChecker == null) throw new PavedMessageException("Unexpected setup form");
    inputUpdateChecker.IsChecked = false;

    // Send with setup parameters
    var setupResult = await forms[0].SubmitAsync(new
    {
        admin_name = settings.Setup.AdminUser,
        admin_email = settings.Setup.AdminMail,
        admin_passwd = settings.Setup.AdminPass,
        admin_confirm_passwd = settings.Setup.AdminPass,
    });
    var loadingElement = setupResult.GetElementById("goto-user-login");
    if (loadingElement == null) throw new PavedMessageException("Unexpected setup result");
    WriteLine(Chalk.Green[$"  Setup completed."]);
});
