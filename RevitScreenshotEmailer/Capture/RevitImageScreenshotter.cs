using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autodesk.Revit.DB;

namespace RevitScreenshotEmailer.Capture;

public sealed class RevitImageScreenshotter : IViewScreenshotter
{
    private const int PixelWidth = 1920;
    private const string TempRoot = "RevitScreenshotEmailer";
    private const string FileStemName = "screenshot";

    public Screenshot Capture(Document document, View view)
    {
        var sessionDirectory = CreateSessionDirectory();
        var fileStem = Path.Combine(sessionDirectory, FileStemName);

        document.ExportImage(BuildOptions(view, fileStem));

        var pngPath = FindFirstPng(sessionDirectory)
            ?? throw new InvalidOperationException("Revit did not produce a screenshot file.");

        return new Screenshot(
            filePath: pngPath,
            sessionDirectory: sessionDirectory,
            viewName: view.Name,
            documentTitle: document.Title,
            capturedAt: DateTime.Now);
    }

    private static string CreateSessionDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), TempRoot, Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private static ImageExportOptions BuildOptions(View view, string fileStem)
    {
        var options = new ImageExportOptions
        {
            ExportRange = ExportRange.SetOfViews,
            HLRandWFViewsFileType = ImageFileType.PNG,
            ShadowViewsFileType = ImageFileType.PNG,
            ImageResolution = ImageResolution.DPI_300,
            ZoomType = ZoomFitType.FitToPage,
            FitDirection = FitDirectionType.Horizontal,
            PixelSize = PixelWidth,
            FilePath = fileStem,
        };
        options.SetViewsAndSheets(new List<ElementId> { view.Id });
        return options;
    }

    private static string? FindFirstPng(string directory) =>
        Directory
            .EnumerateFiles(directory, "*.png", SearchOption.TopDirectoryOnly)
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .FirstOrDefault();
}
