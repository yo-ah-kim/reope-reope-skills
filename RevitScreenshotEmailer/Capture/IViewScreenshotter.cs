using Autodesk.Revit.DB;

namespace RevitScreenshotEmailer.Capture;

public interface IViewScreenshotter
{
    Screenshot Capture(Document document, View view);
}
