using RevitScreenshotEmailer.Capture;

namespace RevitScreenshotEmailer.Destinations;

public interface IScreenshotDestination
{
    string DisplayName { get; }

    void Send(Screenshot screenshot);
}
