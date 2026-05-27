using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitScreenshotEmailer.Capture;
using RevitScreenshotEmailer.Destinations;

namespace RevitScreenshotEmailer;

[Transaction(TransactionMode.Manual)]
public class ScreenshotEmailCommand : IExternalCommand
{
    private const string DialogTitle = "Screenshot Emailer";

    private readonly IViewScreenshotter _screenshotter;
    private readonly Func<IScreenshotDestination> _destinationFactory;

    public ScreenshotEmailCommand()
        : this(new RevitImageScreenshotter(), () => new SmtpEmailDestination(EmailSettings.LoadFromAddinFolder()))
    {
    }

    public ScreenshotEmailCommand(
        IViewScreenshotter screenshotter,
        Func<IScreenshotDestination> destinationFactory)
    {
        _screenshotter = screenshotter ?? throw new ArgumentNullException(nameof(screenshotter));
        _destinationFactory = destinationFactory ?? throw new ArgumentNullException(nameof(destinationFactory));
    }

    public Result Execute(
        ExternalCommandData commandData,
        ref string message,
        ElementSet elements)
    {
        try
        {
            var view = ResolveExportableView(commandData);
            var destination = _destinationFactory();

            using var screenshot = _screenshotter.Capture(view.Document, view);
            destination.Send(screenshot);

            TaskDialog.Show(
                DialogTitle,
                $"Sent screenshot of \"{view.Name}\" to {destination.DisplayName}.");
            return Result.Succeeded;
        }
        catch (Exception ex)
        {
            TaskDialog.Show($"{DialogTitle} — Error", ex.Message);
            return Result.Failed;
        }
    }

    private static View ResolveExportableView(ExternalCommandData commandData)
    {
        var document = commandData.Application.ActiveUIDocument?.Document
            ?? throw new InvalidOperationException("No active Revit document.");

        var view = document.ActiveView
            ?? throw new InvalidOperationException("No active view to capture.");

        if (!view.CanBePrinted)
        {
            throw new InvalidOperationException("The active view cannot be exported as an image.");
        }

        return view;
    }
}
