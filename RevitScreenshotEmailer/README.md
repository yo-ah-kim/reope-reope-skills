# Revit Screenshot Emailer

A Revit 2025 addin that exports the current view as a PNG and emails it to
`joachim@reope.com` when you click it. Targets .NET 8, per the Reope Revit
Addin Setup Guide v2.

## Project layout

```
RevitScreenshotEmailer/
├── RevitScreenshotEmailer.csproj   # .NET 8 class library, references Revit 2025 DLLs
├── ScreenshotEmailCommand.cs       # IExternalCommand: screenshot + SMTP send
├── RevitScreenshotEmailer.addin    # Revit manifest (install to ProgramData)
├── EmailSettings.example.json      # SMTP config template — copy & rename
└── .gitignore                      # excludes bin/, obj/, real EmailSettings.json
```

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
   - `smtpHost` / `smtpPort` / `enableSsl` — your mail server
   - `username` / `password` — for Office 365 / Gmail use an **app password**,
     not your account password
   - `fromAddress` / `fromName` — the sender shown in the email

   `EmailSettings.json` is gitignored. Never commit real credentials.

## Run

1. Launch Revit 2025 and open any project.
2. Open the view you want to capture (3D view, plan, section — anything that
   `View.CanBePrinted` accepts).
3. **Add-Ins tab → External Tools → Screenshot & Email**.
4. A dialog confirms the screenshot was sent to `joachim@reope.com`.

The PNG is exported at 1920 px wide, 300 DPI, fit to page horizontally. The
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
