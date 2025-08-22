#!/usr/bin/env dotnet-script
#r "nuget: AngleSharp, 1.3.0"
#r "nuget: Lestaly.General, 0.102.0"
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

return await Paved.ProceedAsync(noPause: Args.RoughContains("--nopause"), async () =>
{
    using var signal = new SignalCancellationPeriod();
    using var outenc = ConsoleWig.OutputEncodingPeriod(Encoding.UTF8);

    // Show service link
    WriteLine();
    WriteLine("Service URL");
    WriteLine($"  {Poster.Link[settings.ServiceURL]}");
    WriteLine();

    // Determine initial startup (whether the setup form is displayed)
    WriteLine();
    WriteLine("Check initialization status ...");
    var requester = new DefaultHttpRequester() { Timeout = TimeSpan.FromSeconds(3 * 60), };
    var config = Configuration.Default.With(requester).WithDefaultLoader();
    var context = BrowsingContext.New(config);
    var document = default(IDocument);
    using (var breaker = signal.Token.CreateLink(TimeSpan.FromSeconds(10)))
    {
        while (document == null || document.Source.Length <= 0)
        {
            if (document != null) await Task.Delay(TimeSpan.FromMilliseconds(200), breaker.Token);
            document = await context.OpenAsync(settings.ServiceURL, breaker.Token);
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
    var setupForm = forms[0];

    // Change settings
    var inputUpdateChecker = setupForm.QuerySelectorAll<IHtmlInputElement>("*").FirstOrDefault(e => e.Type == "checkbox" && e.Name == "enable_update_checker");
    if (inputUpdateChecker == null) throw new PavedMessageException("Unexpected setup form");
    inputUpdateChecker.IsChecked = false;

    // Send with setup parameters
    var setupResult = await setupForm.SubmitAsync(new
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
