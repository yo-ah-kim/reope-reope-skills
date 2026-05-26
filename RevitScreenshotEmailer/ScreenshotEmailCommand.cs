using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitScreenshotEmailer
{
    [Transaction(TransactionMode.Manual)]
    public class ScreenshotEmailCommand : IExternalCommand
    {
        private const string RecipientEmail = "joachim@reope.com";

        public Result Execute(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements)
        {
            string? sessionDir = null;
            try
            {
                UIDocument uiDoc = commandData.Application.ActiveUIDocument;
                Document? doc = uiDoc?.Document;
                if (doc == null)
                {
                    message = "No active Revit document.";
                    return Result.Failed;
                }

                View activeView = doc.ActiveView;
                if (activeView == null || !activeView.CanBePrinted)
                {
                    message = "Active view cannot be exported as an image.";
                    return Result.Failed;
                }

                sessionDir = Path.Combine(
                    Path.GetTempPath(),
                    "RevitScreenshotEmailer",
                    Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(sessionDir);

                string pathStem = Path.Combine(sessionDir, "screenshot");

                var options = new ImageExportOptions
                {
                    ExportRange = ExportRange.SetOfViews,
                    HLRandWFViewsFileType = ImageFileType.PNG,
                    ShadowViewsFileType = ImageFileType.PNG,
                    ImageResolution = ImageResolution.DPI_300,
                    ZoomType = ZoomFitType.FitToPage,
                    PixelSize = 1920,
                    FitDirection = FitDirectionType.Horizontal,
                    FilePath = pathStem,
                };
                options.SetViewsAndSheets(new List<ElementId> { activeView.Id });

                doc.ExportImage(options);

                string? pngPath = Directory
                    .EnumerateFiles(sessionDir, "*.png", SearchOption.TopDirectoryOnly)
                    .OrderByDescending(File.GetLastWriteTimeUtc)
                    .FirstOrDefault();

                if (pngPath == null || !File.Exists(pngPath))
                {
                    message = "Revit did not produce a screenshot file.";
                    return Result.Failed;
                }

                EmailSettings? settings = LoadEmailSettings(out string settingsPath);
                if (settings == null)
                {
                    message =
                        $"Email settings not found.\n\nExpected: {settingsPath}\n\n" +
                        "Copy EmailSettings.example.json next to the addin DLL, rename it to " +
                        "EmailSettings.json, and fill in your SMTP credentials.";
                    TaskDialog.Show("Screenshot Emailer", message);
                    return Result.Failed;
                }

                string? validationError = settings.Validate();
                if (validationError != null)
                {
                    message = $"EmailSettings.json is invalid: {validationError}";
                    TaskDialog.Show("Screenshot Emailer", message);
                    return Result.Failed;
                }

                SendEmail(settings, pngPath, doc.Title, activeView.Name);

                TaskDialog.Show(
                    "Screenshot Emailer",
                    $"Sent screenshot of view \"{activeView.Name}\" to {RecipientEmail}.");
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                TaskDialog.Show("Screenshot Emailer — Error", ex.ToString());
                return Result.Failed;
            }
            finally
            {
                TryCleanup(sessionDir);
            }
        }

        private static EmailSettings? LoadEmailSettings(out string path)
        {
            string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                         ?? AppContext.BaseDirectory;
            path = Path.Combine(dir, "EmailSettings.json");
            if (!File.Exists(path))
            {
                return null;
            }

            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<EmailSettings>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true,
                });
        }

        private static void SendEmail(
            EmailSettings settings,
            string attachmentPath,
            string documentTitle,
            string viewName)
        {
#pragma warning disable SYSLIB0014 // SmtpClient is obsolete but still the lightest dependency-free option.
            using var client = new SmtpClient(settings.SmtpHost, settings.SmtpPort)
            {
                EnableSsl = settings.EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new NetworkCredential(settings.Username, settings.Password),
                Timeout = 30_000,
            };
#pragma warning restore SYSLIB0014

            using var msg = new MailMessage
            {
                From = new MailAddress(settings.FromAddress!, settings.FromName ?? "Revit Screenshot Emailer"),
                Subject = $"Revit screenshot — {documentTitle} — {viewName}",
                Body =
                    $"Screenshot of view \"{viewName}\" from \"{documentTitle}\", " +
                    $"captured {DateTime.Now:yyyy-MM-dd HH:mm:ss}.",
                IsBodyHtml = false,
            };
            msg.To.Add(RecipientEmail);
            msg.Attachments.Add(new Attachment(attachmentPath));

            client.Send(msg);
        }

        private static void TryCleanup(string? dir)
        {
            if (dir == null || !Directory.Exists(dir)) return;
            try { Directory.Delete(dir, recursive: true); }
            catch { /* leave temp files behind rather than fail */ }
        }

        private sealed class EmailSettings
        {
            [JsonPropertyName("smtpHost")] public string? SmtpHost { get; set; }
            [JsonPropertyName("smtpPort")] public int SmtpPort { get; set; } = 587;
            [JsonPropertyName("enableSsl")] public bool EnableSsl { get; set; } = true;
            [JsonPropertyName("username")] public string? Username { get; set; }
            [JsonPropertyName("password")] public string? Password { get; set; }
            [JsonPropertyName("fromAddress")] public string? FromAddress { get; set; }
            [JsonPropertyName("fromName")] public string? FromName { get; set; }

            public string? Validate()
            {
                if (string.IsNullOrWhiteSpace(SmtpHost)) return "smtpHost is required";
                if (SmtpPort <= 0) return "smtpPort must be > 0";
                if (string.IsNullOrWhiteSpace(Username)) return "username is required";
                if (string.IsNullOrWhiteSpace(Password)) return "password is required";
                if (string.IsNullOrWhiteSpace(FromAddress)) return "fromAddress is required";
                return null;
            }
        }
    }
}
