using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RevitScreenshotEmailer.Destinations;

public sealed class EmailSettings
{
    public const string FileName = "EmailSettings.json";

    [JsonPropertyName("smtpHost")] public string? SmtpHost { get; set; }
    [JsonPropertyName("smtpPort")] public int SmtpPort { get; set; } = 587;
    [JsonPropertyName("enableSsl")] public bool EnableSsl { get; set; } = true;
    [JsonPropertyName("username")] public string? Username { get; set; }
    [JsonPropertyName("password")] public string? Password { get; set; }
    [JsonPropertyName("fromAddress")] public string? FromAddress { get; set; }
    [JsonPropertyName("fromName")] public string? FromName { get; set; }
    [JsonPropertyName("recipient")] public string? Recipient { get; set; }

    public static EmailSettings LoadFromAddinFolder()
    {
        var path = ResolvePath();
        if (!File.Exists(path))
        {
            throw new FileNotFoundException(
                $"Email settings not found. Copy EmailSettings.example.json to {path} and fill in your SMTP credentials.",
                path);
        }

        var settings = JsonSerializer.Deserialize<EmailSettings>(
            File.ReadAllText(path),
            new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
            });

        if (settings == null)
        {
            throw new InvalidDataException($"Could not parse {path}.");
        }

        settings.Validate();
        return settings;
    }

    public void Validate()
    {
        Require(SmtpHost, nameof(SmtpHost));
        Require(Username, nameof(Username));
        Require(Password, nameof(Password));
        Require(FromAddress, nameof(FromAddress));
        Require(Recipient, nameof(Recipient));

        if (SmtpPort <= 0)
        {
            throw new InvalidDataException($"{nameof(SmtpPort)} must be greater than zero.");
        }
    }

    private static void Require(string? value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidDataException($"{name} is required in {FileName}.");
        }
    }

    private static string ResolvePath()
    {
        var location = typeof(EmailSettings).Assembly.Location;
        var directory = string.IsNullOrEmpty(location) ? null : Path.GetDirectoryName(location);
        if (string.IsNullOrEmpty(directory))
        {
            throw new InvalidOperationException(
                "Could not resolve the addin folder to locate EmailSettings.json. " +
                "Confirm the addin DLL is loaded from disk (not shadow-copied or single-file packed).");
        }
        return Path.Combine(directory, FileName);
    }
}
