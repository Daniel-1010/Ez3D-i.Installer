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

| Test Case | Method | Spec Document | Description |
| :--- | :--- | :--- | :--- |
| **TC01** | `Run_TC01_Clean_Installation()` | [`TC01_Clean_Installation.md`](file:///d:/Ez3D-i.Installer/DOCUMENT/Install/TC01_Clean_Installation.md) | Launches setup wizard, completes 5-step wizard flow, monitors progress, and finishes. |
| **TC02** | `Run_TC02_Post_Install_Verify()` | [`TC02_Post_Install_Verify.md`](file:///d:/Ez3D-i.Installer/DOCUMENT/Install/TC02_Post_Install_Verify.md) | Verifies binaries (`Bin\Ez3D-i64.exe`), desktop shortcut, version info, and Windows Registry keys. |
| **TC03** | `Run_TC03_Uninstall_Control_Panel()` | [`TC03_Uninstall_Control_Panel.md`](file:///d:/Ez3D-i.Installer/DOCUMENT/Install/TC03_Uninstall_Control_Panel.md) | Uninstallation from Windows Control Panel (Confirmation $\rightarrow$ Progress $\rightarrow$ Finish). |
| **TC04** | `Run_TC04_Uninstall_Rerun_Package()` | [`TC04_Uninstall_Rerun_Package.md`](file:///d:/Ez3D-i.Installer/DOCUMENT/Install/TC04_Uninstall_Rerun_Package.md) | Uninstallation via rerunning setup package (Maintenance Remove mode). |
| **TC05** | `Run_TC05_Full_Lifecycle()` | [`TC05_Full_Lifecycle.md`](file:///d:/Ez3D-i.Installer/DOCUMENT/Install/TC05_Full_Lifecycle.md) | End-to-end composite: Clean Install $\rightarrow$ State Verification $\rightarrow$ Uninstallation $\rightarrow$ Final Cleanup Assert. |
| **TC06** | `Run_TC06_Quick_Setting()` | [`TC06_Quick_Setting.md`](file:///d:/Ez3D-i.Installer/DOCUMENT/Install/TC06_Quick_Setting.md) | 1st launch Quick Setting: anti-hide elevation past VS Code, Search IP (127.0.0.1) selection, and persistence assert. |

### Setup Wizard 5-Step Workflow
1. **Step 1:** Click **Next >** button on *Welcome to the InstallShield Wizard for Ez3D-i* screen.
2. **Step 2:** Select **"I accept the terms of the license agreement"** radio button on *License Agreement* screen.
3. **Step 3:** Click **Next >** button on *License Agreement* screen (unlocked after acceptance).
4. **Step 4:** Click **Next >** button on *Choose Destination Location* screen (`C:\Program Files\VATECH`).
5. **Step 5:** Click **Install** button on *Ready to Install the Program* screen to start file copy.

### Installation Progress & Completion Phase
* **Setup Status Monitoring:** Monitored via [`WaitForInstallProgressAction`](file:///d:/Ez3D-i.Installer/Actions/Install/WaitForInstallProgressAction.cs). Tracks file copying progress (e.g. `Bin\fei_proxy_VisilogIP.dll`) and auto-handles prerequisite popups.
* **InstallShield Wizard Complete:** Handled via [`CompleteInstallationAction`](file:///d:/Ez3D-i.Installer/Actions/Install/CompleteInstallationAction.cs). Focuses the window, clicks the **Finish** button, and waits for installer process exit.

### Uninstallation Workflows
* **TC03 (Control Panel):**
  1. Detect confirmation dialog (*"Do you want to completely remove..."*) $\rightarrow$ Click **[ Yes ]**.
  2. Wait for uninstallation progress bar to complete.
  3. Detect *"Uninstall Complete"* dialog $\rightarrow$ Click **[ Finish ]**.
  4. Assert removal of binary, desktop shortcut, and registry key.
* **TC04 (Rerun Setup Package):**
  1. Rerun setup package $\rightarrow$ Handle language prompt if shown.
  2. Detect Maintenance Mode dialog $\rightarrow$ Select **"Remove"** radio button $\rightarrow$ Click **[ Next > ]**.
  3. Detect confirmation dialog $\rightarrow$ Click **[ Yes ]**.
  4. Wait for uninstallation progress bar to complete.
  5. Detect *"Uninstall Complete"* dialog $\rightarrow$ Click **[ Finish ]**.
  6. Assert removal of binary, desktop shortcut, and registry key.

---

## 4. Execution Commands (VSCode Terminal)

Per **Rule 7**, execute tests directly in your elevated VSCode / Windows Terminal:

```powershell
cd d:\Ez3D-i.Installer

# Run Clean Installation
dotnet test --filter "TC01_Clean_Installation"

# Verify Post-Install State
dotnet test --filter "TC02_Post_Install_Verify"

# Run Control Panel Uninstallation (Case 1)
dotnet test --filter "TC03_Uninstall_Control_Panel"

# Run Rerun Package Uninstallation (Case 2)
dotnet test --filter "TC04_Uninstall_Rerun_Package"

# Run Full End-to-End Lifecycle
dotnet test --filter "TC05_Full_Lifecycle"
```

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
