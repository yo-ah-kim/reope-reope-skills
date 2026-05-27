using System;
using System.Collections.Generic;
using System.IO;
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
        try
        {
            var fileStem = Path.Combine(sessionDirectory, FileStemName);
            document.ExportImage(BuildOptions(view, fileStem));

            var pngPath = ResolveSinglePng(sessionDirectory);
            return new Screenshot(
                filePath: pngPath,
                sessionDirectory: sessionDirectory,
                viewName: view.Name,
                documentTitle: document.Title,
                capturedAt: DateTime.Now);
        }
        catch
        {
            TryDelete(sessionDirectory);
            throw;
        }
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

    private static string ResolveSinglePng(string directory)
    {
        var pngs = Directory.GetFiles(directory, "*.png", SearchOption.TopDirectoryOnly);
        return pngs.Length switch
        {
            0 => throw new InvalidOperationException("Revit did not produce a screenshot file."),
            1 => pngs[0],
            _ => throw new InvalidOperationException(
                $"Revit produced {pngs.Length} screenshot files; expected exactly one."),
        };
    }

    private static void TryDelete(string directory)
    {
        try { Directory.Delete(directory, recursive: true); }
        catch { }
    }
}
