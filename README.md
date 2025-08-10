# PCMedic (.NET 8, Windows-only) – MVP

This repo contains a Windows-only .NET 8 solution with three projects:
- PCMedic.Agent: Windows Service hosting a local API (Kestrel on http://localhost:7766)
- PCMedic.UI: WPF desktop app that displays status/findings and triggers fixes
- PCMedic.Shared: Shared DTOs and enums

## Prerequisites
- Windows 10/11
- .NET SDK 8.0+
- Administrator privileges to install/start the Windows Service

## Build & Restore
```powershell
# From repository root
 dotnet restore
```

## Install and start the Agent as Windows Service
Run as Administrator:
```powershell
 .\scripts\install-service.ps1
```
This publishes PCMedic.Agent (single-file) and installs/starts the service "PCMedicAgent".

To uninstall:
```powershell
 .\scripts\uninstall-service.ps1
```

## Run the UI
- Open PCMedic.UI in your IDE and Start (Debug) OR
- From CLI: `dotnet run --project .\PCMedic.UI\PCMedic.UI.csproj`

The UI expects the Agent API at http://localhost:7766.

## API Endpoints (local only)
- GET /health/latest → latest snapshot JSON
- GET /findings → evaluated findings
- POST /fix/{action} → triggers a repair action
  - actions: `sfc`, `dism`, `schedule-chkdsk`, `defrag-hdd`

Notes:
- Actions write logs to %ProgramData%\PCMedic\logs\repair.log
- API is bound to localhost only; no auth (MVP)

## Important MVP Constraints
- Do not defragment SSDs. Use the `defrag-hdd` action only for HDD volumes.
- All actions require elevation when executed by the service; results are logged.
- SMART via WMI (MSStorageDriver_*). If raw data unavailable, mark values as Unknown and continue.

---

# Welcome to your Lovable project

## Project info

**URL**: https://lovable.dev/projects/595019c7-b820-4802-b826-2e896c57ef7c

## How can I edit this code?

There are several ways of editing your application.

**Use Lovable**

Simply visit the [Lovable Project](https://lovable.dev/projects/595019c7-b820-4802-b826-2e896c57ef7c) and start prompting.

Changes made via Lovable will be committed automatically to this repo.

**Use your preferred IDE**

If you want to work locally using your own IDE, you can clone this repo and push changes. Pushed changes will also be reflected in Lovable.

The only requirement is having Node.js & npm installed - [install with nvm](https://github.com/nvm-sh/nvm#installing-and-updating)

Follow these steps:

```sh
# Step 1: Clone the repository using the project's Git URL.
 git clone <YOUR_GIT_URL>

# Step 2: Navigate to the project directory.
 cd <YOUR_PROJECT_NAME>

# Step 3: Install the necessary dependencies.
 npm i

# Step 4: Start the development server with auto-reloading and an instant preview.
 npm run dev
```

**Edit a file directly in GitHub**

- Navigate to the desired file(s).
- Click the "Edit" button (pencil icon) at the top right of the file view.
- Make your changes and commit the changes.

**Use GitHub Codespaces**

- Navigate to the main page of your repository.
- Click on the "Code" button (green button) near the top right.
- Select the "Codespaces" tab.
- Click on "New codespace" to launch a new Codespace environment.
- Edit files directly within the Codespace and commit and push your changes once you're done.

## What technologies are used for this project?

This project is built with:

- Vite
- TypeScript
- React
- shadcn-ui
- Tailwind CSS

## How can I deploy this project?

Simply open [Lovable](https://lovable.dev/projects/595019c7-b820-4802-b826-2e896c57ef7c) and click on Share -> Publish.

## Can I connect a custom domain to my Lovable project?

Yes, you can!

To connect a domain, navigate to Project > Settings > Domains and click Connect Domain.

Read more here: [Setting up a custom domain](https://docs.lovable.dev/tips-tricks/custom-domain#step-by-step-guide)
