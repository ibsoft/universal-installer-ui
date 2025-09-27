# Universal Installer — CONFIG MANUAL (Μόνο υποστηριζόμενα)

Η παρούσα τεκμηρίωση περιγράφει **αποκλειστικά** όσα υποστηρίζονται από τον τρέχοντα κώδικα (models/factories/handlers).
Οτιδήποτε **δεν** αναφέρεται εδώ, **δεν υποστηρίζεται** και δεν πρέπει να εμφανίζεται σε config.

---

## Γενική Δομή

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

---

## 1) `meta` (υποστηρίζεται)
- `name` (string, **required**): Εμπορική ονομασία προϊόντος.
- `version` (string): Έκδοση.
- `publisher` (string): Εκδότης/εταιρεία.
- `supportUrl` (string URL): URL υποστήριξης.

**Μη υποστηριζόμενα**: `eulaPath`, `productCode`.

---

## 2) `ui` (υποστηρίζεται)
- `logoPath` (string): Μονοπάτι σε εικόνα (PNG/JPG).
- `title` (string)
- `subtitle` (string)
- `theme`:
  - `mode` (`"dark"` | `"light"`)
  - `primaryColor` (hex π.χ. `#22C55E`)
  - `fontSize` (int, προεπιλογή 10)
- `locale` (string, π.χ. `en-US`)
- `window`:
  - `width` (int, default 900)
  - `height` (int, default 600)
  - `topMost` (bool, default false)

**Μη υποστηριζόμενα**: `iconPath`, `showTaskProgress`.

---

## 3) `artifacts` (υποστηρίζεται)
Λίστα αντικειμένων προς εγκατάσταση/χρήση.

Κάθε artifact:
- `id` (string, **required**): Αναγνωριστικό.
- `type` (string, **υποστηριζόμενα**: `"msi"`, `"exe"`, `"zip"`)
- `source` (string): Τοπικό path ή HTTP(S) URL.
- `sha256` (string, optional): Προαιρετικός έλεγχος hash.
- `targetDir` (string): **Απαιτείται μόνο** όταν `type: "zip"`.
- `silentArgs` (string): Επιπλέον args για `msi`/`exe`.

> Η απλή αντιγραφή αρχείων **δεν** είναι `artifact type`; γίνεται με `task: "copy"`.

**Μη υποστηριζόμενα**: `type: "msix"`, `type: "copy"`.

---

## 4) `preChecks` (υποστηρίζεται)
Τρέχουν πριν από οποιαδήποτε εγκατάσταση. Αν αποτύχει προέλεγχος, η διαδικασία σταματά.

Τύποι & πεδία:
- `adminRights` — `{ "type": "adminRights", "failMessage": "Run as Administrator." }`
- `osVersion` — `{ "type": "osVersion", "min": "10.0.19041" }`
- `dotnetDesktopRuntime` — `{ "type": "dotnetDesktopRuntime", "min": "8.0.0" }`
- `diskSpace` — `{ "type": "diskSpace", "drive": "C:", "minMB": 2048 }`
- `ports` — `{ "type": "ports", "ports": [80,443] }` (αποτυχία αν κάποια είναι **ήδη σε χρήση**)

---

## 5) `tasks` / `postTasks` (υποστηρίζονται)
Εκτελούνται **σειριακά**. Τα `postTasks` τρέχουν μετά από επιτυχή εγκατάσταση.

Υποστηριζόμενοι τύποι:
- `runArtifact` — `{ "type": "runArtifact", "ref": "<artifactId>" }`
- `unzip` — `{ "type": "unzip", "ref": "<zipArtifactId>", "targetDir?": "<dir>" }`
- `copy` — `{ "type": "copy", "source": "<path|glob>", "targetDir": "<dir>", "recursive": true|false }`
- `serviceInstall` — `{ "type": "serviceInstall", "name", "displayName", "binaryPath", "startMode", "account" }`
- `serviceStart` — `{ "type": "serviceStart", "name" }`
- `serviceStop` — `{ "type": "serviceStop", "name" }`
- `serviceDelete` — `{ "type": "serviceDelete", "name" }`
- `registrySet` — `{ "type": "registrySet", "path", "name", "value", "kind" }` (HKLM/HKCU/HKCR/HKU)
- `envSet` — `{ "type": "envSet", "name": "VAR" , "value": "..." }` (ορισμός **machine** env var)
- `shortcut` — `{ "type": "shortcut", "target", "location": "Desktop"|"StartMenu", "name", "iconPath?" }`
- `powershell` — `{ "type": "powershell", "script", "args?", "timeoutSec?", "continueOnError": true|false }`
- `msiUninstall` — `{ "type": "msiUninstall", "productCode" }`
- `removeDir` — `{ "type": "removeDir", "path", "recursive": true|false }`

> Κοινό πεδίο (όπου αναφέρεται): `continueOnError` (bool), όταν υπάρχει — προχωρά παρά το σφάλμα του συγκεκριμένου task.

---

## 6) `uninstall`
Υποστηρίζεται η εξής δομή:
```json
{
  "preTasks": [ /* TaskSpec... */ ],
  "tasks": [ /* TaskSpec... */ ]
}
```
Συνήθης ροή: `serviceStop` → `serviceDelete` → `msiUninstall` → `removeDir`.

---

## 7) `logging`
- `level`: `"Info" | "Warn" | "Error"`
- `file`: Μονοπάτι σε αρχείο log (π.χ. `%ProgramData%\App\logs\install.log`)
- `maxSizeMB`: int (default 10)
- `maxFiles`: int (default 5)

---

## 8) `telemetry`
- `enabled` (bool)
- `endpoint` (string URL)
Στέλνονται μόνο βασικά γεγονότα (έναρξη/λήξη και έκβαση).

---

## Κανόνες Εγκυρότητας (validation)
- Κάθε `artifact.id` πρέπει να είναι **μοναδικό**.
- `unzip`/`runArtifact` απαιτούν έγκυρο `ref` σε υπάρχον `artifact`.
- Για `zip` απαιτείται `targetDir` είτε στο `artifact` είτε στο `task`.
- `serviceInstall.binaryPath` πρέπει να δείχνει σε υπαρκτό `.exe`.
- Το `registrySet.kind` πρέπει να είναι συμβατό με `value` (π.χ. `String`, `DWord`, `QWord`).

---

## Παράδειγμα Πλήρους & Έγκυρου Config
Δείτε το `config.sample.json` στο ίδιο φάκελο.
