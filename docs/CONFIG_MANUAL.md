# Universal Installer — Configuration Manual

This document describes the JSON configuration schema used to drive the installer engine.

> **Quick start:** Put your file next to the EXE (default: `config.min.json`). The UI loads it on startup.

---

## Top-level Structure

```json
{
  "meta": { ... },
  "ui": { ... },
  "artifacts": [ ... ],
  "preChecks": [ ... ],
  "tasks": [ ... ],
  "postTasks": [ ... ],
  "uninstall": { ... },
  "logging": { ... },
  "telemetry": { ... }
}
```

### 1) `meta` (required)
- **name** *(string, required)*: Product name shown in UI/logs.
- **version** *(string)*: Semantic or plain version.
- **publisher** *(string)*: Company/author.
- **supportUrl** *(string URL)*: Shown as “Support” link in UI.

### 2) `ui`
- **logoPath** *(string)*: Local relative/absolute path (PNG/SVG). Copied file is not required; a relative path is resolved against the app folder.
- **title** *(string)*: Window/app title.
- **subtitle** *(string)*: Secondary caption.
- **theme**:
  - **mode** *(string)*: `"dark"` or `"light"` (visual hint).
  - **primaryColor** *(string)*: Hex like `#3B82F6` used for primary buttons.
  - **fontSize** *(int)*: Base font size (UI uses Segoe UI; WinForms renders system font).
- **locale** *(string)*: e.g., `"en-US"` — reserved for future localization.
- **window**:
  - **width** *(int, default 900)*
  - **height** *(int, default 600)*
  - **topMost** *(bool, default false)*

### 3) `artifacts` (array)
Downloadable or local payloads to install.
Each artifact:
- **id** *(string, required)*: Unique reference used by tasks.
- **type** *(string, required)*: `"msi"|"exe"|"msix"|"zip"|"copy"`
- **source** *(string, required)*: HTTP(S) URL or local path (env vars supported, e.g., `%LOCALAPPDATA%\...`).
- **sha256** *(string)*: Optional integrity check (empty to skip).
- **targetDir** *(string)*: Required for `zip` & `copy`.
- **silentArgs** *(string)*: Installer switches for `msi`/`exe` (`msi` defaults to `/qn /norestart`, `exe` defaults to `/S`).

### 4) `preChecks` (array)
Supported types:
- `{ "type": "adminRights", "failMessage": "Run as Administrator." }`
- `{ "type": "osVersion", "min": "10.0.19041" }`
- `{ "type": "dotnetDesktopRuntime", "min": "8.0.0" }`
- `{ "type": "diskSpace", "drive": "C:", "minMB": 2048 }`
- `{ "type": "ports", "ports": [80, 443] }` — fails if any port is currently listening.

### 5) `tasks` (array)
Run in order. All tasks are **idempotent** where possible.

**Common field**: `continueOnError` *(bool)* honored by `powershell`, ignored by most.

Supported tasks:

- **runArtifact**
  ```json
  { "type": "runArtifact", "ref": "appMsi" }
  ```
  - Resolves artifact by `ref` and executes based on `artifact.type`.

- **unzip**
  ```json
  { "type": "unzip", "ref": "svcZip", "targetDir": "C:\\Path\\Override" }
  ```
  - Extracts zip to `artifact.targetDir` or override `targetDir`.

- **copy**
  ```json
  { "type": "copy", "source": "assets\\*", "targetDir": "%ProgramData%\\MyApp", "recursive": true }
  ```

- **serviceInstall**
  ```json
  {
    "type": "serviceInstall",
    "name": "My.Service",
    "displayName": "My Windows Service",
    "binaryPath": "C:\\Program Files\\MySvc\\Svc.exe",
    "startMode": "Automatic",
    "account": "LocalSystem"
  }
  ```

- **serviceStart / serviceStop / serviceDelete**
  ```json
  { "type": "serviceStart", "name": "My.Service" }
  { "type": "serviceStop", "name": "My.Service" }
  { "type": "serviceDelete", "name": "My.Service" }
  ```

- **registrySet**
  ```json
  { "type": "registrySet", "path": "HKLM\\Software\\MyApp", "name": "ApiUrl", "value": "https://api.local", "kind": "String" }
  ```
  - `path` supports roots: `HKLM|HKEY_LOCAL_MACHINE`, `HKCU`, `HKCR`, `HKU`.

- **envSet**
  ```json
  { "type": "envSet", "name": "MYAPP_HOME", "value": "C:\\Program Files\\MyApp" }
  ```
  - Sets **machine** environment variable. Requires admin.

- **shortcut**
  ```json
  { "type": "shortcut", "target": "C:\\Program Files\\MyApp\\MyApp.exe", "location": "Desktop", "name": "MyApp", "iconPath": "C:\\icons\\my.ico" }
  ```
  - `location`: `"Desktop"` or `"StartMenu"` (Programs). Creates **common** shortcuts.

- **powershell**
  ```json
  { "type": "powershell", "script": "scripts\\post.ps1", "args": "-RepairFalse", "timeoutSec": 300, "continueOnError": false }
  ```

- **msiUninstall**
  ```json
  { "type": "msiUninstall", "productCode": "{GUID-HERE}" }
  ```

- **removeDir**
  ```json
  { "type": "removeDir", "path": "%LOCALAPPDATA%\\MyApp", "recursive": true }
  ```

### 6) `postTasks` (array)
Runs after `tasks` succeed; same schema as `tasks`.

### 7) `uninstall`
- **preTasks** *(array)*: Tasks executed before uninstall (e.g., stop services).
- **tasks** *(array)*: Removal logic — delete service, uninstall MSI, delete folders.

### 8) `logging`
- **level** *(string)*: `"Info"|"Warn"|"Error"` (MVP uses Info/Error).
- **file** *(string)*: Path to rolling log file (env vars supported).
- **maxSizeMB** *(int, default 10)*
- **maxFiles** *(int, default 5)*

### 9) `telemetry`
- **enabled** *(bool)*: Default `false`.
- **endpoint** *(string URL)*: If set, sends minimal events `{start|end}` with timestamps.
  - Only mode, app, version, success, and error message (if any). No PII.

---

## Environment Variables in Paths
All tasks that accept paths support environment variable expansion:
- `%LOCALAPPDATA%`, `%ProgramData%`, `%TEMP%`, etc.

---

## Example: Minimal
```json
{
  "meta": { "name": "CF.TotalCare", "version": "1.2.3", "publisher": "UNIXFOR", "supportUrl": "https://example.com/support" },
  "ui": {
    "logoPath": "Assets\\brand.png",
    "title": "CF Suite Installer",
    "subtitle": "Setup Wizard",
    "theme": { "mode": "dark", "primaryColor": "#3B82F6", "fontSize": 10 },
    "locale": "en-US",
    "window": { "width": 900, "height": 600, "topMost": false }
  },
  "artifacts": [
    { "id": "appMsi", "type": "msi", "source": "C:\\drop\\myapp.msi", "sha256": "", "silentArgs": "/qn /norestart" }
  ],
  "preChecks": [
    { "type": "adminRights", "failMessage": "Run as Administrator." }
  ],
  "tasks": [
    { "type": "runArtifact", "ref": "appMsi" }
  ],
  "postTasks": [],
  "uninstall": { "preTasks": [], "tasks": [] },
  "logging": { "level": "Info", "file": "%ProgramData%\\MyApp\\logs\\install.log", "maxSizeMB": 10, "maxFiles": 5 },
  "telemetry": { "enabled": false, "endpoint": "" }
}
```

## Example: Full (App + Service)
```json
{
  "meta": { "name": "CF.TotalCare", "version": "1.2.3", "publisher": "UNIXFOR", "supportUrl": "https://support.example.com" },
  "ui": {
    "logoPath": "assets\\brand.png",
    "title": "CF Suite Installer",
    "subtitle": "Setup Wizard",
    "theme": { "mode": "dark", "primaryColor": "#22C55E", "fontSize": 10 },
    "locale": "en-US",
    "window": { "width": 1000, "height": 650, "topMost": false }
  },
  "artifacts": [
    { "id": "appMsi", "type": "msi", "source": "https://cdn.example.com/builds/app.msi", "sha256": "<sha256>", "silentArgs": "/qn /norestart" },
    { "id": "svcZip", "type": "zip", "source": "files\\service.zip", "sha256": "<sha256>", "targetDir": "C:\\Program Files\\MyService" }
  ],
  "preChecks": [
    { "type": "adminRights", "failMessage": "Run as Administrator." },
    { "type": "osVersion", "min": "10.0.19041" },
    { "type": "dotnetDesktopRuntime", "min": "8.0.0" },
    { "type": "diskSpace", "drive": "C:", "minMB": 2048 }
  ],
  "tasks": [
    { "type": "runArtifact", "ref": "appMsi" },
    { "type": "unzip", "ref": "svcZip" },
    { "type": "serviceInstall", "name": "CF.TotalCare", "displayName": "CF TotalCare Service", "binaryPath": "C:\\Program Files\\MyService\\CF.TotalCare.Service.exe", "startMode": "Automatic", "account": "LocalSystem" },
    { "type": "serviceStart", "name": "CF.TotalCare" },
    { "type": "registrySet", "path": "HKLM\\Software\\MyApp", "name": "ApiUrl", "value": "https://api.local", "kind": "String" },
    { "type": "envSet", "name": "MYAPP_HOME", "value": "C:\\Program Files\\MyApp" },
    { "type": "shortcut", "target": "C:\\Program Files\\MyApp\\MyApp.exe", "location": "Desktop", "name": "MyApp" }
  ],
  "postTasks": [
    { "type": "powershell", "script": "scripts\\post.ps1", "args": "-RepairFalse", "timeoutSec": 300 }
  ],
  "uninstall": {
    "preTasks": [{ "type": "serviceStop", "name": "CF.TotalCare", "continueOnError": true }],
    "tasks": [
      { "type": "serviceDelete", "name": "CF.TotalCare", "continueOnError": true },
      { "type": "msiUninstall", "productCode": "{GUID-HERE}" },
      { "type": "removeDir", "path": "C:\\Program Files\\MyService", "recursive": true }
    ]
  },
  "logging": { "level": "Info", "file": "C:\\Logs\\Installer\\install.log", "maxSizeMB": 10, "maxFiles": 5 },
  "telemetry": { "enabled": false, "endpoint": "" }
}
```

---

## Tips
- Run **as Administrator** for service/registry/HKLM operations.
- MSI/EXE silent arguments vary per vendor. Typical: `/quiet /norestart`, `/S`, `/VERYSILENT`.
- Use `postTasks` for configuration scripts that should only run on successful installs.
- For **rollback** safety, keep destructive actions (e.g., deletes) in uninstall or at the end.
