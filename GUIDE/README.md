# Ez3D-i.Installer — Developer & Testing Guide

## 1. Prerequisites & System Requirements
* **Operating System:** Windows 10/11 x64.
* **Elevation Requirement:** **Elevated Administrator privileges** are strictly required:
  * For InstallShield setup and uninstaller interaction (bypassing Windows UIPI UAC barriers).
  * For administratively purging the Quick Setting configuration file from the protected system directory:
    `C:\ProgramData\ewoosoft\common\config\VTServerConfig.ini`.
* **Runtimes:** .NET SDK 10.0 (`net10.0-windows`).
* **Package Storage:**
  * Target release installer placed in `Installation Package/` (e.g. `Ez3D-i V5.5.16.0 Setup(x64).exe`).
  * Baseline older installer placed in `Installation Package/Old Version/` (e.g. `Ez3D-i V5.5.15.1 Setup(x64).exe`) for upgrade testing.

---

## 2. Running Tests (VS Code / Terminal)

Per **Rule 7**, UI automation tests must always be executed directly from your elevated **Administrator VS Code Terminal** or **Windows Terminal**:

```powershell
cd d:\Ez3D-i.Installer

# Build Solution (TreatWarningsAsErrors=true, zero warnings enforced)
dotnet build
```

---

### ⚡ How to Run All 6 Install Cases (TC01–TC06)

You can run all 6 installation cases together using either a single batch command or a sequential PowerShell loop:

#### Method A: Single Batch Command (Fastest)
Discovers and executes all 6 test fixtures belonging to `[Category("Installation")]`:
```powershell
dotnet test "d:\Ez3D-i.Installer\Ez3D-i.Installer.csproj" --filter "Category=Installation" --logger "console;verbosity=normal"
```
*(Alternatively, filter by namespace: `dotnet test --filter "FullyQualifiedName~Tests.Install"`)*

#### Method B: Sequential Loop with Live Headers (Recommended for Debugging)
Executes each test case independently with dedicated status banners, clean console logs, and isolated process lifecycles:
```powershell
@("TC01_Clean_Installation", "TC02_Post_Install_Verify", "TC03_Uninstall_Control_Panel", "TC04_Uninstall_Rerun_Package", "TC05_Full_Lifecycle", "TC06_Quick_Setting") | ForEach-Object {
    Write-Host "`n========================================================" -ForegroundColor Cyan
    Write-Host ">>> RUNNING: $_" -ForegroundColor Green
    Write-Host "========================================================`n" -ForegroundColor Cyan
    dotnet test "d:\Ez3D-i.Installer\Ez3D-i.Installer.csproj" --filter "FullyQualifiedName~$_" --logger "console;verbosity=normal"
}
```

#### 🔄 Execution Flow & Bilateral State Guards
Because of our bilateral state guards, all 6 test cases are 100% self-healing and can run repeatedly in sequence without manual workstation cleanup:

| Order | Test Fixture | Purpose & Precondition State Guard |
| :---: | :--- | :--- |
| **1** | [`TC01_Clean_Installation`](file:///d:/Ez3D-i.Installer/Tests/Install/TC01_Clean_Installation.cs) | Clean 5-step installation wizard. Guarded by `EnsureEz3DiUninstalledAction` (auto-purges lingering install first). |
| **2** | [`TC02_Post_Install_Verify`](file:///d:/Ez3D-i.Installer/Tests/Install/TC02_Post_Install_Verify.cs) | Verifies `Bin\Ez3D-i64.exe`, Desktop Shortcut, and Registry Uninstall keys. |
| **3** | [`TC03_Uninstall_Control_Panel`](file:///d:/Ez3D-i.Installer/Tests/Install/TC03_Uninstall_Control_Panel.cs) | Uninstalls via Registry `UninstallString`. Guarded by `EnsureEz3DiInstalledAction` (deploys Ez3D-i if missing). |
| **4** | [`TC04_Uninstall_Rerun_Package`](file:///d:/Ez3D-i.Installer/Tests/Install/TC04_Uninstall_Rerun_Package.cs) | Reruns setup package in Maintenance Mode and selects Remove. Guarded by `EnsureEz3DiInstalledAction`. |
| **5** | [`TC05_Full_Lifecycle`](file:///d:/Ez3D-i.Installer/Tests/Install/TC05_Full_Lifecycle.cs) | Full end-to-end composite: Clean Install $\rightarrow$ Verify $\rightarrow$ Uninstall $\rightarrow$ Cleanup Verify. |
| **6** | [`TC06_Quick_Setting`](file:///d:/Ez3D-i.Installer/Tests/Install/TC06_Quick_Setting.cs) | First-launch Quick Setting: purges `C:\ProgramData` config, brings dialog past VS Code, searches IP `127.0.0.1`, saves and asserts. |

---

### 📦 Running Individual Install Test Cases

```powershell
# TC01: Clean 5-Step Installation
dotnet test --filter "FullyQualifiedName~TC01_Clean_Installation" --logger "console;verbosity=normal"

# TC02: Post-Installation Artifact Verification (Binary, Shortcut, Registry)
dotnet test --filter "FullyQualifiedName~TC02_Post_Install_Verify" --logger "console;verbosity=normal"

# TC03: Uninstallation via Windows Control Panel (Registry UninstallString)
dotnet test --filter "FullyQualifiedName~TC03_Uninstall_Control_Panel" --logger "console;verbosity=normal"

# TC04: Uninstallation via Rerunning Setup Package (Maintenance Mode Remove)
dotnet test --filter "FullyQualifiedName~TC04_Uninstall_Rerun_Package" --logger "console;verbosity=normal"

# TC05: Full End-to-End Lifecycle (Clean Install -> Verify -> Uninstall -> Verify Cleanup)
dotnet test --filter "FullyQualifiedName~TC05_Full_Lifecycle" --logger "console;verbosity=normal"

# TC06: Quick Setting 1st Launch Configuration (Anti-Hide Taskbar, Search IP, 127.0.0.1)
dotnet test --filter "FullyQualifiedName~TC06_Quick_Setting" --logger "console;verbosity=normal"
```

---

### 🚀 Upgrade Suite (`Tests/Upgrade/`)

```powershell
# TC01: In-Place Application Upgrade (Baseline v5.5.15.1 -> Target v5.5.16.0)
dotnet test --filter "FullyQualifiedName~TC01_InPlace_Upgrade" --logger "console;verbosity=normal"

# Run Entire Upgrade Suite
dotnet test --filter "Category=Upgrade" --logger "console;verbosity=normal"
```

---

## 3. Results & Reports

Test runs automatically output consolidated execution logs, JSON test data, HTML dashboards, and milestone screenshots into `d:\Ez3D-i.Installer\Result\`:

* **Visual Dashboard Report:** [`Result/Report_Installation.html`](file:///d:/Ez3D-i.Installer/Result/Report_Installation.html)
* **Structured JSON Results:** [`Result/ResultsData.json`](file:///d:/Ez3D-i.Installer/Result/ResultsData.json)
* **Raw Execution Log:** [`Result/Results.txt`](file:///d:/Ez3D-i.Installer/Result/Results.txt)
* **Milestone Screenshots:** [`Result/<TestName>/TCxx_Step*.png`](file:///d:/Ez3D-i.Installer/Result/)

---

## 4. Key Architecture Documents

* **Review & Calibration Log:** [`GUIDE/REVIEW.md`](file:///d:/Ez3D-i.Installer/GUIDE/REVIEW.md) (Chronological log of all architectural milestones).
* **Project Structure:** [`GUIDE/ProjectStructure.md`](file:///d:/Ez3D-i.Installer/GUIDE/ProjectStructure.md) (Complete directory and file inventory).
* **Installation Test Specs:** [`DOCUMENT/Install/INSTALL_TEST_SPEC.md`](file:///d:/Ez3D-i.Installer/DOCUMENT/Install/INSTALL_TEST_SPEC.md).
* **Upgrade Test Specs:** [`DOCUMENT/Upgrade/UPGRADE_TEST_SPEC.md`](file:///d:/Ez3D-i.Installer/DOCUMENT/Upgrade/UPGRADE_TEST_SPEC.md).
* **Dependency & Blast Radius Graph:** [`Code Graph/Impact_Analysis_Matrix.md`](file:///d:/Ez3D-i.Installer/Code%20Graph/Impact_Analysis_Matrix.md).
