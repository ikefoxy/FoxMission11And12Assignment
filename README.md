# FoxMission11And12Assignment

Bookstore app built with ASP.NET Core Web API + React + SQLite.

## What This Includes

- Book catalog page
- Admin page at `/adminbooks`
- Book CRUD API (`POST`, `PUT`, `DELETE`)
- SQLite database (`Bookstore.sqlite`)

## Live Deployment

- Site: `https://foxmission13-ikefox-21573.azurewebsites.net`
- Admin: `https://foxmission13-ikefox-21573.azurewebsites.net/adminbooks`

## Localhost Quick Start

1. Install required tools:
   - .NET 10 SDK
   - Node.js 20+ (includes npm)

macOS (Homebrew):

```bash
brew update
brew install --cask dotnet-sdk
brew install node@22
```

Windows (PowerShell as Admin):

```powershell
winget install Microsoft.DotNet.SDK.10
winget install OpenJS.NodeJS.LTS
```

2. Verify installs:

```bash
dotnet --version
node --version
npm --version
```

3. Open a terminal in the project folder:

```bash
cd FoxMission11And12Assignment
```

4. From the repo root, run:

```bash
npm run start:all
```

5. Open:
   - `http://localhost:5039/`
   - `http://localhost:5039/adminbooks`

6. Confirm API is running:

```bash
curl http://localhost:5039/api/categories
```

7. Stop the app with `Ctrl + C`.
