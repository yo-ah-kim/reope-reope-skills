# Revit Screenshot Emailer

A Revit 2025 addin that exports the current view as a PNG and emails it to
`joachim@reope.com` when you click it. Targets .NET 8, per the Reope Revit
Addin Setup Guide v2.

## Project layout

```
RevitScreenshotEmailer/
├── RevitScreenshotEmailer.csproj   # .NET 8 class library, references Revit 2025 DLLs
├── RevitScreenshotEmailer.addin    # Revit manifest (install to ProgramData)
├── EmailSettings.example.json      # SMTP config template — copy & rename
├── ScreenshotEmailCommand.cs       # IExternalCommand: thin orchestrator
├── Capture/
│   ├── IViewScreenshotter.cs       # capture abstraction
│   ├── RevitImageScreenshotter.cs  # Revit ImageExportOptions implementation
│   └── Screenshot.cs               # value + temp-file cleanup
└── Destinations/
    ├── IScreenshotDestination.cs   # swap point: where the screenshot goes
    ├── SmtpEmailDestination.cs     # current implementation: SMTP email
    └── EmailSettings.cs            # JSON config + validation
```

### Swapping destinations

The destination is the seam. To send to Slack, Teams, a shared drive, or
anywhere else, write a new `IScreenshotDestination`, then change one line
in `ScreenshotEmailCommand`'s default constructor:

```csharp
public ScreenshotEmailCommand()
    : this(new RevitImageScreenshotter(), () => new SlackDestination(...)) { }
```

The orchestrator, capture pipeline, and temp-file lifecycle don't need to
change.

## Prerequisites

Per the setup guide:

| Requirement       | Details                                                   |
| ----------------- | --------------------------------------------------------- |
| Visual Studio     | Latest (only needed if you prefer the IDE workflow)       |
| .NET 8 SDK        | https://dotnet.microsoft.com/download/dotnet/8            |
| Revit 2025        | Installed at `C:\Program Files\Autodesk\Revit 2025\`      |
| Git + GitHub      | Standard setup                                            |

## Build

From an x64 Developer Prompt or any shell with the .NET 8 SDK on PATH:

```powershell
cd RevitScreenshotEmailer
dotnet build -c Debug
```

Output DLL lands in `bin\Debug\net8.0-windows\RevitScreenshotEmailer.dll`.

> If Revit is installed somewhere other than `C:\Program Files\Autodesk\Revit 2025\`,
> override the path: `dotnet build -c Debug -p:RevitInstallDir="D:\Revit2025\"`.

## Install the addin

1. **Update the assembly path** in `RevitScreenshotEmailer.addin`. Replace
   `C:\Path\To\RevitScreenshotEmailer\bin\Debug\net8.0-windows\RevitScreenshotEmailer.dll`
   with the absolute path to the DLL you just built.
2. **Copy the addin file** to
   `C:\ProgramData\Autodesk\Revit\Addins\2025\RevitScreenshotEmailer.addin`.
3. **Configure SMTP credentials**: copy `EmailSettings.example.json` next to
   the DLL (same folder), rename to `EmailSettings.json`, and fill in:
   - `smtpHost` / `smtpPort` / `enableSsl` — defaults to `smtp.gmail.com:587`
     with STARTTLS, which works for Gmail and Google Workspace
   - `username` — your full Google Workspace address (e.g. `joachim@reope.com`)
   - `password` — a **Google App Password**, not your account password
     (see "Getting a Google App Password" below)
   - `fromAddress` / `fromName` — the sender shown in the email
   - `recipient` — who receives the screenshot (defaults to `joachim@reope.com`)

   `EmailSettings.json` is gitignored. Never commit real credentials.

### Getting a Google App Password

Google blocks plain-password SMTP for Workspace accounts. You need a 16-character
App Password instead:

1. Sign in at <https://myaccount.google.com>.
2. Enable **2-Step Verification** if it isn't on already
   (Security → 2-Step Verification).
3. Open **App passwords**: <https://myaccount.google.com/apppasswords>.
4. Name it something like "Revit Screenshot Emailer" and create.
5. Copy the 16-character password (shown once, no spaces needed) into the
   `password` field of `EmailSettings.json`.

> **Workspace admins:** if `App passwords` isn't visible, your admin has
> disabled it. Ask them to allow it for your account, or have them enable
> SMTP relay and use that host instead.

## Run

1. Launch Revit 2025 and open any project.
2. Open the view you want to capture (3D view, plan, section — anything that
   `View.CanBePrinted` accepts).
3. **Add-Ins tab → External Tools → Screenshot & Email**.
4. A dialog confirms the screenshot was sent to `joachim@reope.com`.

The PNG is exported at 1920 px wide, fit to page horizontally. The
temp file is cleaned up after the email is sent.

## Common errors

| Error                                         | Fix                                                                 |
| --------------------------------------------- | ------------------------------------------------------------------- |
| `Email settings not found.`                   | Place `EmailSettings.json` in the same folder as the built DLL.     |
| `Active view cannot be exported as an image.` | Switch to a view where `CanBePrinted` is true (most views qualify). |
| SMTP authentication errors                    | Use an app password (Office 365 / Gmail require it with MFA on).    |
| Revit doesn't show the addin                  | Verify the .addin file is in `C:\ProgramData\Autodesk\Revit\Addins\2025\` and that `<Assembly>` points to the real DLL. |
| Revit crashes on load                         | Confirm target is `net8.0-windows` and Revit refs have `<Private>false</Private>` (Copy Local = False). |

## How it relates to the PDF checklist

Steps 1–2 (project + rename), 10–12 (Revit refs + IExternalCommand), and 14
(.addin file) are pre-done in this repo. You still do the Git steps (3–9, 16)
and the build/install/test steps (13, 15) on your Windows machine.
