# Ez3D-i Installation & Uninstallation Test Automation Specification

## 1. Overview
The `Ez3D-i.Installer` test automation suite automates end-to-end deployment, verification, and uninstallation testing for Vatech Ez3D-i imaging software.

* **Target OS:** Windows 10/11 x64 (Elevated Administrator)
* **Framework:** .NET 10.0-windows, FlaUI UIA3, NUnit 4.3, OpenCvSharp4
* **Target Installer:** InstallShield / InstallScript Setup Launcher (`*.exe`)

---

## 2. Directory Layout & Package Placement
* **Installer Directory:** `d:\Ez3D-i.Installer\Installation Package\`
* **Dynamic Scanner:** [`FindInstallerPackageAction`](file:///d:/Ez3D-i.Installer/Actions/Discovery/FindInstallerPackageAction.cs) scans for any `*.exe` located inside `Installation Package/`. You can drop any Ez3D-i setup executable into this directory without modifying code.
* **Fallback Scan:** If `Installation Package/` is empty, it automatically checks `D:\Test product\Ez3D-i\` for the latest setup package.

---

## 3. Test Cases Catalog

| Test Case | Method | Description |
| :--- | :--- | :--- |
| **TC01** | `Run_TC01_Clean_Installation()` | Launches setup wizard, handles language selection, accepts license agreement, triggers installation, and confirms completion. |
| **TC02** | `Run_TC02_Post_Install_Verify()` | Verifies binaries exist (`Ez3D-i64.exe`), desktop shortcut, version info, and Windows Registry keys. |
| **TC03** | `Run_TC03_Uninstallation()` | Launches uninstaller, confirms removal prompt, monitors uninstall progress, and validates system cleanup. |
| **TC04** | `Run_TC04_Full_Lifecycle()` | End-to-end composite: Clean Install $\rightarrow$ State Verification $\rightarrow$ Uninstallation $\rightarrow$ Final Cleanup Assert. |

---

## 4. Execution Commands (VSCode Terminal)

Per **Rule 7**, execute tests directly in your elevated VSCode / Windows Terminal:

```powershell
cd d:\Ez3D-i.Installer

# Run Clean Installation
dotnet test --filter "FullyQualifiedName~TC01_Clean_Installation" --logger "console;verbosity=normal"

# Run Post-Installation Verification
dotnet test --filter "FullyQualifiedName~TC02_Post_Install_Verify" --logger "console;verbosity=normal"

# Run Uninstallation
dotnet test --filter "FullyQualifiedName~TC03_Uninstallation" --logger "console;verbosity=normal"

# Run Complete End-to-End Lifecycle
dotnet test --filter "FullyQualifiedName~TC04_Full_Lifecycle" --logger "console;verbosity=normal"
```

---

## 5. Test Results & Reports
* **Dashboard Report:** `d:\Ez3D-i.Installer\Result\Report_Installation.html`
* **Text Summary:** `d:\Ez3D-i.Installer\Result\Results.txt`
* **JSON State:** `d:\Ez3D-i.Installer\Result\ResultsData.json`
* **Step Screenshots:** `d:\Ez3D-i.Installer\Result\<TestName>\*.png`
