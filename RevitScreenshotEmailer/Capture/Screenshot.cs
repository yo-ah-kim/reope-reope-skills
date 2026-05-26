using System;
using System.IO;

namespace RevitScreenshotEmailer.Capture;

public sealed class Screenshot : IDisposable
{
    private readonly string _sessionDirectory;
    private bool _disposed;

    public Screenshot(
        string filePath,
        string sessionDirectory,
        string viewName,
        string documentTitle,
        DateTime capturedAt)
    {
        FilePath = filePath;
        ViewName = viewName;
        DocumentTitle = documentTitle;
        CapturedAt = capturedAt;
        _sessionDirectory = sessionDirectory;
    }

    public string FilePath { get; }
    public string ViewName { get; }
    public string DocumentTitle { get; }
    public DateTime CapturedAt { get; }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        try
        {
            if (Directory.Exists(_sessionDirectory))
            {
                Directory.Delete(_sessionDirectory, recursive: true);
            }
        }
        catch
        {
            // Leaving a temp file behind is preferable to crashing Revit during cleanup.
        }
    }
}
