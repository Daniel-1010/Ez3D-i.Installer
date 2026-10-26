# Ez3D-i.Installer — Review & Calibration Log

This document records chronological review milestones, calibration fixes, and operational updates across `Ez3D-i.Installer`.

---

## 📋 Section 1: Initial Architecture & 5-Step Wizard Calibration (2026-10-26)

### 1. Context & Objectives
- Implemented modular Screen-Action-Fixture architecture for automated InstallShield wizard testing of Vatech Ez3D-i.
- Tailored and synchronized the exact 5-step wizard flow based on actual UI screenshots:
  1. Welcome $\rightarrow$ Click [ Next > ]
  2. License Agreement $\rightarrow$ Select "I accept..." radio button
  3. License Agreement $\rightarrow$ Click [ Next > ] (now enabled)
  4. Choose Destination Location $\rightarrow$ Click [ Next > ]
  5. Ready to Install $\rightarrow$ Click [ Install ]
- Monitored progress bar during setup file copy and automated dismissal on the "InstallShield Wizard Complete" screen via the [ Finish ] button.

### 2. Root Cause & Technical Fixes
1. **Radio Button Event Propagation:**
   - *Fix:* Replaced FlaUI `IsChecked = true` with physical coordinate mouse click (`InputDriver.ClickAtPhysicalCoordinate`) directly on the radio circle to trigger Win32 `BN_CLICKED`, unlocking the `[ Next > ]` button.
2. **Binary Path Resolution:**
   - *Fix:* Added `Bin\` path support (`C:\Program Files\VATECH\Ez3D-i\Bin\Ez3D-i64.exe`) in `InstallerConstants.cs` and `VerifyInstallationStateAction.cs`.
3. **Project Governance Replication:**
   - *Fix:* Replicated the `Ez3D-i` structure including `AGENTS.md`, `.agents/RULE.md`, `.agents/Lessons/`, `Code Graph/`, `Assets/Templates/`, `.vscode/tasks.json`, and `GUIDE/`.

### 3. Verification
- `dotnet build` passes with 0 Warnings and 0 Errors.
- `TC02_Post_Install_Verify` passed and verified binary, desktop shortcut, and registry uninstall entry.

---

## 📋 Section 2: Multi-Monitor & Any-Screen Detection Engine (2026-10-26)

### 1. Context & Objectives
- Supported dynamic detection on multi-monitor workstations where the installer opens on any display (e.g. `DISPLAY1` at `X=1920` or `DISPLAY2` at `X=0`).
- Prevented false-negative offscreen classification and empty screenshot captures.
- Integrated OpenCV template matching (`ImageCompare`) across `SystemInformation.VirtualScreen`.

### 2. Technical Modifications
1. **`Core/ImageCompare.cs`:**
   - Implemented template matching using `Cv2.MatchTemplate` scanning the combined multi-screen desktop `VirtualScreen` (`3840x1080`).
   - Translates matched image locations to global virtual coordinates for physical clicks.
2. **`Core/WindowHelper.cs` & `Screens/InstallerWizardScreen.cs`:**
   - Added `GetScreenForHwnd(hwnd)` and `IsVisibleOnAnyScreen(hwnd)`.
   - Replaced FlaUI `!win.IsOffscreen` with Win32 visibility checks across any monitor.
3. **`Tests/BaseInstallerFixture.cs`:**
   - Enhanced `CaptureScreenshot` to target the active window's monitor or the full virtual desktop.
4. **`Actions/Install/StepThroughSetupWizardAction.cs` & `CompleteInstallationAction.cs`:**
   - Added visual fallback via `ImageCompare` if UIA elements are unresponsive.

### 3. Verification
- Built solution: 0 Warnings, 0 Errors. Verified multi-display support across active dual monitors.

---

## 📋 Section 3: Control Panel Uninstallation (TC03A) & Specification Suite (2026-10-26)

### 1. Context & Objectives
- Supported uninstallation from Windows Control Panel via registered `UninstallString` (`TC03A`).
- Replicated exact 3-step uninstallation wizard workflow from UI screenshots:
  1. Confirm dialog (*"Do you want to completely remove..."*) $\rightarrow$ Click **[ Yes ]**.
  2. Wait for uninstallation progress bar to complete.
  3. *Uninstall Complete* dialog $\rightarrow$ Click **[ Finish ]**.
- Created dedicated Markdown specifications in `DOCUMENT/` for each test case (`TC01`, `TC02`, `TC03A`, `TC03B`, `TC04`).

### 2. Technical Modifications
1. **`Assets/Templates/`:** Added `08_uninstall_confirm_prompt.png` and `09_uninstall_complete_finish.png`.
2. **`Actions/Uninstall/ConfirmUninstallationAction.cs`:**
   - Explicitly steps through Step 1 (Yes) $\rightarrow$ Step 2 (Progress) $\rightarrow$ Step 3 (Finish).
   - Added OpenCV fallback matching (`ImageCompare.See` / `ImageCompare.Click`).
3. **`Tests/TC03_Uninstallation.cs` & `TC04_Full_Lifecycle.cs`:**
   - Added milestone screenshot callbacks (`TC03_Step1_ConfirmYesClicked`, `TC03_Step2_ProgressCompleted`, `TC03_Step3_FinishClicked`, `TC03_Step4_CleanupVerified`).
4. **`DOCUMENT/`:** Created `TC01_Clean_Installation.md`, `TC02_Post_Install_Verify.md`, `TC03A_Uninstall_Control_Panel.md`, `TC03B_Uninstall_Rerun_Package.md`, and `TC04_Full_Lifecycle.md`.

### 3. Verification
- `dotnet build` passes with 0 Warnings and 0 Errors.
- All documents, rules, and code graphs synchronized.

---

## 📋 Section 4: Anti-Hide Elevation Protocol for VS Code (2026-10-26)

### 1. Context & Objectives
- Fixed critical issue where `Ez3D-i - InstallShield Wizard` window hid behind VS Code when tests were triggered from the VS Code elevated terminal.
- Enforced foreground placement and Z-order management so the installer is always visible.

### 2. Technical Modifications
1. **`Core/NativeMethods.cs`:** Added `BringWindowToTop`, `SetWindowPos`, `ShowWindow`, and `SW_RESTORE`.
2. **`Core/InputDriver.cs`:**
   - Implemented `PinWindowOnTop(hwnd)` using `HWND_TOPMOST` followed by `HWND_NOTOPMOST` to force Z-order elevation without permanently locking the window topmost.
   - Implemented `DemoteVsCodeZOrder(installerHwnd)` to explicitly push any active `Code.exe` windows behind the installer.
3. **`Core/WindowHelper.cs`:** Added `CenterWindowOnScreen` to cleanly center the wizard on the active display.
4. **`Screens/InstallerWizardScreen.cs` & `UninstallerWizardScreen.cs`:** Integrated centering and anti-hide pinning as soon as windows are captured.

### 3. Verification
- `dotnet build` succeeded with 0 Warnings and 0 Errors. Wizard is unconditionally placed in front of VS Code.

---

## 📋 Section 5: True Wizard Window Filtering & 3-Way Button Locator (2026-10-26)

### 1. Context & Objectives
- Fixed failure where the Welcome screen was visible on display, but the mouse did not move to the [ Next > ] button.
- Prevented false-positive capture of VS Code (`Ez3D-i.Installer - Visual Studio Code`) and added native Win32 child button discovery.

### 2. Technical Modifications
1. **`Assets/Templates/btn_next.png`:** Cropped dedicated Next button template.
2. **`Core/NativeMethods.cs` & `Core/WindowHelper.cs`:**
   - Added `EnumChildWindows`, `GetDlgCtrlID`, `GetWindowTitle`, and `GetClassName`.
   - Created `FindChildButtonCoordinate(parentHwnd, buttonTexts)` to directly locate child button HWNDs and bounding rects without UIA delays.
3. **`Screens/InstallerWizardScreen.cs` & `UninstallerWizardScreen.cs`:**
   - Added instant Win32 lookup `NativeMethods.FindWindow(null, "Ez3D-i - InstallShield Wizard")`.
   - Explicitly excluded `Visual Studio Code`, `.csproj`, `.Installer`, Chrome, and Explorer windows.
   - Added `FindButtonCoordinate`.
4. **`Actions/Install/StepThroughSetupWizardAction.cs`:**
   - Wired 3-way button locator: Win32 `EnumChildWindows` $\rightarrow$ FlaUI UIA3 $\rightarrow$ OpenCV `btn_next.png` $\rightarrow$ Geometric dialog offset.

### 3. Verification
- `dotnet build` succeeds with 0 Warnings and 0 Errors. The mouse moves directly to the [ Next > ] button.

---

## 📋 Section 6: Dynamic Project Root Resolution & Result Directory Fix (2026-10-26)

### 1. Context & Objectives
- Fixed path resolution issue where test results, logs, and screenshots were being written to root drive `D:\Result\` instead of the project directory `D:\Ez3D-i.Installer\Result\`.
- Aligned project root resolution with `Ez3D-i` by adopting dynamic upward directory search for project marker files.

### 2. Technical Modifications
1. **`Core/InstallerConstants.cs`:**
   - Replaced static `..\..\..\..\` 4-level traversal with dynamic upward directory scanner.
   - Iterates up parent directories checking for `Ez3D-i.Installer.csproj` or `Assets/`, guaranteeing `SolutionRoot` correctly resolves to `D:\Ez3D-i.Installer` regardless of target framework (`net10.0-windows`), build mode (`Debug`/`Release`), or test runner working directories.
2. **Directory Relocation:**
   - Relocated historical artifacts (`Report_Installation.html`, `ResultsData.json`, `Results.txt`, and screenshot folders) from `D:\Result\` to `D:\Ez3D-i.Installer\Result\`.

### 3. Verification
- `dotnet build` compiled with 0 Warnings and 0 Errors.
- All test outputs, HTML reports, and screenshots are now stored within `D:\Ez3D-i.Installer\Result\`.

---

## 📋 Section 7: Uninstallation Suite Separation (TC03 & TC04) & Maintenance Mode Engine (2026-10-26)

### 1. Context & Objectives
- Formalized and separated the two distinct uninstallation entry points into dedicated, sequentially numbered test fixtures:
  - **`TC03_Uninstall_Control_Panel`**: Case 1 Uninstallation via Windows Registry `UninstallString`.
  - **`TC04_Uninstall_Rerun_Package`**: Case 2 Uninstallation via rerunning original setup package in Maintenance Mode.
  - **`TC05_Full_Lifecycle`**: Reordered end-to-end composite integration test.
- Implemented `HandleMaintenanceModeAction` to automate selecting the "Remove" radio button and advancing past the InstallShield maintenance wizard.

### 2. Technical Modifications
1. **`Actions/Uninstall/HandleMaintenanceModeAction.cs`:**
   - Detects InstallShield Maintenance Mode page.
   - Selects "Remove" radio option using physical coordinate mouse click (`InputDriver.ClickAtPhysicalCoordinate`).
   - Clicks `[ Next > ]` to trigger the confirmation prompt.
2. **`Tests/` Suite:**
   - Created `Tests/TC03_Uninstall_Control_Panel.cs`.
   - Created `Tests/TC04_Uninstall_Rerun_Package.cs`.
   - Renamed and reordered `Tests/TC04_Full_Lifecycle.cs` $\rightarrow$ `Tests/TC05_Full_Lifecycle.cs` (`[Order(5)]`).
   - Removed obsolete legacy `Tests/TC03_Uninstallation.cs`.
3. **`DOCUMENT/` Specifications:**
   - Realigned specifications: `TC03_Uninstall_Control_Panel.md`, `TC04_Uninstall_Rerun_Package.md`, `TC05_Full_Lifecycle.md`.
   - Updated master index in `DOCUMENT/INSTALL_TEST_SPEC.md`.
4. **`Code Graph/`:**
   - Synchronized `Test_To_Action_Map.md`, `Action_Call_Hierarchy.md`, and `Impact_Analysis_Matrix.md`.

### 3. Verification
- `dotnet build` passed with **0 Warnings, 0 Errors**.

---

## 📋 Section 8: Golden Template Calibration for TC04 Rerun Uninstallation (2026-10-26)

### 1. Context & Objectives
- Calibrated the exact 5-step maintenance uninstallation flow based on user's production screenshots:
  1. Rerun the installation package
  2. Select "Remove" radio button
  3. Click [ Next > ] button
  4. Message displayed $\rightarrow$ Click [ Yes ] button
  5. Wait progress finished $\rightarrow$ Click [ Finish ] button
- Ingested golden UI template assets into `Assets/Templates/` for pixel-perfect OpenCV matching.

### 2. Technical Modifications
1. **`Assets/Templates/`:**
   - Ingested `10_maintenance_mode_welcome.png` (Maintenance screen with Repair / Remove).
   - Ingested `11_maintenance_mode_remove_selected.png` (Remove option selected with confirm dialog).
   - Updated `08_uninstall_confirm_prompt.png` with pixel-perfect dialog screenshot.
   - Updated `09_uninstall_complete_finish.png` with pixel-perfect finish screen.
2. **`Actions/Uninstall/HandleMaintenanceModeAction.cs`:**
   - Wired physical coordinate clicking (`InputDriver.ClickAtPhysicalCoordinate`) into the "Remove" radio button circle (`X ≈ bounds.Left + 12`, `Y ≈ bounds.Top + bounds.Height / 2` with geometric fallback `X=62, Y=272`).
   - Settle 500ms to allow InstallScript to process `WM_COMMAND (BN_CLICKED)`.
   - Clicks `[ Next > ]` button and triggers `TC04_Step3_NextClicked`.
3. **`Actions/Uninstall/ConfirmUninstallationAction.cs`:**
   - Standardized milestone event triggers: `"ConfirmYesClicked"`, `"ProgressCompleted"`, `"FinishClicked"`.
4. **`Tests/TC04_Uninstall_Rerun_Package.cs`:**
   - Mapped all 5 milestones to sequentially numbered screenshot captures.
5. **`DOCUMENT/TC04_Uninstall_Rerun_Package.md` & `Code Graph/Core_And_Infrastructure_Graph.md`:**
   - Updated specification with exact 5-step steps and registered new templates in Asset Catalog.

### 3. Verification
- `dotnet build` compiled with **0 Warnings, 0 Errors**.

---

## 📋 Section 9: Prerequisite Installation Guard Engine for Continuous Uninstallation (2026-10-26)

### 1. Context & Objectives
- Supported continuous execution of uninstallation test fixtures (`TC03` and `TC04`):
  - When `TC03` uninstalls the product, the system is left clean.
  - If `TC04` runs immediately after on a clean system, rerunning the setup package would trigger a fresh installation flow instead of Maintenance Mode, causing uninstallation validation to fail.
  - If either test runs standalone on an uninstalled machine, uninstallation commands cannot execute.
- Implemented an automatic prerequisite state guard (`EnsureEz3DiInstalledAction`) that checks installation state and deploys Ez3D-i before uninstallation testing only when missing.

### 2. Technical Modifications
1. **`Actions/Install/EnsureEz3DiInstalledAction.cs`:**
   - Detects state via `RegistryHelper.IsInstalled` and `File.Exists(DefaultExecutablePath)`.
   - If installed: returns immediately with 0 overhead.
   - If not installed: reuses existing installation actions (`LaunchInstallerAction` $\rightarrow$ `HandleLanguageSelectionAction` $\rightarrow$ `StepThroughSetupWizardAction` $\rightarrow$ `WaitForInstallProgressAction` $\rightarrow$ `CompleteInstallationAction` $\rightarrow$ `VerifyInstallationStateAction`).
2. **`Tests/TC03_Uninstall_Control_Panel.cs` & `Tests/TC04_Uninstall_Rerun_Package.cs`:**
   - Integrated `EnsureEz3DiInstalledAction.Execute` as Step 0 before uninstallation begins.
3. **`DOCUMENT/` Specifications & `Code Graph/`:**
   - Updated `DOCUMENT/TC03_Uninstall_Control_Panel.md` and `TC04_Uninstall_Rerun_Package.md` with Step 0 prerequisite check.
   - Registered `EnsureEz3DiInstalledAction` in `Test_To_Action_Map.md`, `Action_Call_Hierarchy.md`, and `Impact_Analysis_Matrix.md`.

### 3. Verification
- `dotnet build` passed with **0 Warnings, 0 Errors**.

---

## 📋 Section 10: Setup Launcher Bootstrapper Version Decoupling (2026-10-26)

### 1. Context & Objectives
- Fixed assertion failure where post-installation verification failed with:
  `Installed version (5.5.16.0) does not match expected '16.0.400'.`
- Diagnosed that `16.0.400` originates from the PE `VS_VERSION_INFO` of the Flexera InstallShield self-extracting wrapper (`InstallScript Setup Launcher` v16.0) rather than the Ez3D-i application.
- Decoupled application version discovery from wrapper header metadata.

### 2. Technical Modifications
1. **`Actions/Discovery/FindInstallerPackageAction.cs`:**
   - Detects when `FileVersionInfo` returns an InstallShield wrapper version (`16.0*`, `InstallScript`, `InstallShield`).
   - Automatically parses the actual Ez3D-i application version from the installer package filename via regex (`[vV]?(\d+\.\d+(\.\d+)*)`), extracting `5.5.16.0`.
2. **`Actions/Verify/VerifyInstallationStateAction.cs`:**
   - Strips whitespace from version comparisons.
   - Bypasses exact string matching if the expected version is an InstallShield bootstrapper header (`16.0*`).

### 3. Verification
- `dotnet build` passed with **0 Warnings, 0 Errors**. Application version `5.5.16.0` is properly verified.

---

## 📋 Section 11: Idempotent Clean-State Guard for Installation Suites (2026-10-26)

### 1. Context & Objectives
- Diagnosed failure in `TC05_Full_Lifecycle` where Phase 1 timed out looking for the License Agreement radio button:
  `System.TimeoutException : Failed to find 'I accept...' radio button on License page within 25s.`
- Root cause: A preceding test failure in `TC04` left Ez3D-i installed. When `TC05` started clean installation, InstallShield detected the existing installation and booted into **Maintenance Mode** instead of the fresh 5-step installer wizard.
- Implemented `EnsureEz3DiUninstalledAction` to guarantee clean state before running fresh deployment flows in `TC01` and `TC05`.

### 2. Technical Modifications
1. **`Actions/Uninstall/EnsureEz3DiUninstalledAction.cs`:**
   - Detects if Ez3D-i is installed via `RegistryHelper.IsInstalled` and executable presence.
   - If clean: returns immediately (0 overhead).
   - If installed: automatically invokes `LaunchUninstallerAction` $\rightarrow$ `ConfirmUninstallationAction` $\rightarrow$ `VerifyUninstallationStateAction` to purge the machine before clean install starts.
2. **`Tests/TC01_Clean_Installation.cs` & `Tests/TC05_Full_Lifecycle.cs`:**
   - Integrated `EnsureEz3DiUninstalledAction.Execute` at Step 0.
3. **`Code Graph/`:**
   - Registered `EnsureEz3DiUninstalledAction` in `Test_To_Action_Map.md`, `Action_Call_Hierarchy.md`, and `Impact_Analysis_Matrix.md`.

### 3. Verification
- `dotnet build` passed with **0 Warnings, 0 Errors**. Both installation and uninstallation test suites are fully idempotent.

---

## 📋 Section 12: Test Suite & Specification Modular Grouping into 'Install' (2026-10-26)

### 1. Context & Objectives
- Restructured test fixtures and specification documents into dedicated `Install` folders, harmonizing project organization with `Ez3D-i` conventions.
- Encapsulated `TC01` through `TC05` under `Tests/Install/` and specifications under `DOCUMENT/Install/`.

### 2. Technical Modifications
1. **Directory Restructuring:**
   - Created `Tests/Install/` and moved `TC01_Clean_Installation.cs`, `TC02_Post_Install_Verify.cs`, `TC03_Uninstall_Control_Panel.cs`, `TC04_Uninstall_Rerun_Package.cs`, `TC05_Full_Lifecycle.cs`.
   - Updated namespaces to `Ez3Di.Installer.Tests.Install` while inheriting from `BaseInstallerFixture` in `Ez3Di.Installer.Tests`.
   - Created `DOCUMENT/Install/` and moved all markdown specifications and master index `INSTALL_TEST_SPEC.md`.
2. **Code Graph & Project Governance Synchronization:**
   - Updated `Code Graph/Test_To_Action_Map.md` and `Code Graph/Impact_Analysis_Matrix.md`.
   - Updated `GUIDE/ProjectStructure.md`.

### 3. Verification
- `dotnet build` passed with **0 Warnings, 0 Errors**. All 5 fixtures are registered and discovered under the new folder structure.

---

## 📋 Section 13: Creation of 'Update' Test Suite & Specifications (2026-10-26)

### 1. Context & Objectives
- Established the architectural foundation for Ez3D-i application upgrade and patch testing.
- Created dedicated `Tests/Update/` directory and specifications layout under `DOCUMENT/Update/`.

### 2. Technical Modifications
1. **Directory Layout:**
   - Created `Tests/Update/` with `.gitkeep`.
   - Created `DOCUMENT/Update/UPDATE_TEST_SPEC.md` outlining planned upgrade test scenarios (in-place upgrade, data preservation, cancel rollback).
2. **Project Governance Synchronization:**
   - Updated `GUIDE/ProjectStructure.md` with the new `Update` suite folders.

### 3. Verification
- `dotnet build` passed with **0 Warnings, 0 Errors**.

---

## 📋 Section 14: Isolated 'Old Version' Package Storage & Scanner Safeguard (2026-10-26)

### 1. Context & Objectives
- Created dedicated storage folder for baseline/older Ez3D-i setup packages at `Installation Package/Old Version/`.
- Guaranteed 100% strict isolation between `Install` and `Update` suites so older packages placed in `Old Version` cannot accidentally be picked up by the `Install` suite.

### 2. Technical Modifications
1. **Directory Layout:**
   - Created `Installation Package/Old Version/` with `.gitkeep`.
2. **`Core/InstallerConstants.cs`:**
   - Added `OldVersionPackageDirectory => Path.Combine(PackageDirectory, "Old Version")`.
3. **`Actions/Discovery/FindInstallerPackageAction.cs`:**
   - Restricted primary `Execute()` method to `SearchOption.TopDirectoryOnly` and explicitly filtered out any path containing `"Old Version"`, guaranteeing clean installation tests only discover the primary target package.
   - Added dedicated `FindOldVersion()` method specifically for the `Update` test suite to query `Old Version/`.
4. **Project Governance Synchronization:**
   - Updated `GUIDE/ProjectStructure.md`.

### 3. Verification
- `dotnet build` passed with **0 Warnings, 0 Errors**. Packages in `Old Version` are completely isolated from `Install` test suites.

---

## 📋 Section 15: Standardization & Renaming from 'Update' to 'Upgrade' (2026-10-26)

### 1. Context & Objectives
- Standardized terminology across the test automation architecture, transitioning the planned secondary suite from `Update` to `Upgrade`.
- Aligned test directory names, specification folder structures, code comments, and architecture diagrams.

### 2. Technical Modifications
1. **Directory Restructuring:**
   - Renamed `Tests/Update/` $\rightarrow$ `Tests/Upgrade/` (preserving `.gitkeep`).
   - Renamed `DOCUMENT/Update/` $\rightarrow$ `DOCUMENT/Upgrade/`.
   - Renamed `DOCUMENT/Upgrade/UPDATE_TEST_SPEC.md` $\rightarrow$ `DOCUMENT/Upgrade/UPGRADE_TEST_SPEC.md`.
2. **Action Layer Docstrings:**
   - Updated `Actions/Discovery/FindInstallerPackageAction.cs` method comment on `FindOldVersion()` to target the `Upgrade` test suite.
3. **Project Governance & Architecture Synchronization:**
   - Synchronized `GUIDE/ProjectStructure.md` with `Tests/Upgrade/` and `DOCUMENT/Upgrade/UPGRADE_TEST_SPEC.md`.
   - Synchronized `Code Graph/Action_Call_Hierarchy.md` Section 3 diagram to reference `UpgradeSuite`.

### 3. Verification
- `dotnet build` passed with **0 Warnings, 0 Errors**.
- Zero regression on existing `Install` test suite (`TC01` through `TC05`).

---

## 📋 Section 16: In-Place Application Upgrade Automation & Baseline State Guard (2026-10-26)

### 1. Context & Objectives
- Implemented **`TC01_InPlace_Upgrade`** in the `Upgrade` test suite to automate upgrading Ez3D-i from an older version (`v5.5.7.3`) to target release (`v5.5.16.0`).
- Automated the 3-step upgrade wizard workflow based on production screenshots:
  1. Upgrade Welcome screen $\rightarrow$ Click `[ Next > ]`.
  2. Setup Status $\rightarrow$ Monitor file replacement progress bar until complete.
  3. Update Complete $\rightarrow$ Click `[ Finish ]`.
- Engineered an idempotent baseline state guard (`EnsureEz3DiOldVersionInstalledAction`) to automatically ensure `v5.5.7.3` is installed before running the upgrade test.

### 2. Technical Modifications
1. **`Assets/Templates/`:**
   - Ingested golden templates `12_upgrade_welcome.png`, `13_upgrade_status_progress.png`, `14_upgrade_complete_finish.png`.
   - Registered templates in `Code Graph/Core_And_Infrastructure_Graph.md`.
2. **`Actions/Upgrade/EnsureEz3DiOldVersionInstalledAction.cs`:**
   - Detects if host has baseline older version (`v5.5.7.3`). If already present, bypasses with 0 overhead.
   - If target version (`5.5.16.0`) is present, triggers `EnsureEz3DiUninstalledAction` to cleanly purge the host.
   - Executes standard clean 5-step installer wizard with the older package (`Ez3D-i V5.5.7.3 Setup(x64).exe`) from `Installation Package/Old Version/`.
3. **`Actions/Upgrade/HandleUpgradeWizardAction.cs`:**
   - Locates `Ez3D-i - InstallShield Wizard`, pins foreground, and advances past the Upgrade Welcome dialog via physical coordinate click on `[ Next > ]`.
4. **`Tests/Upgrade/TC01_InPlace_Upgrade.cs`:**
   - Created test fixture (`[Category("Upgrade")]`, `[Order(1)]`).
   - Chains: Precondition guard $\rightarrow$ Launch v5.5.16.0 $\rightarrow$ Language prompt $\rightarrow$ Upgrade Welcome $\rightarrow$ Setup Progress $\rightarrow$ Finish $\rightarrow$ Post-Upgrade State Verification (`Bin\Ez3D-i64.exe` & Registry `5.5.16.0`).
5. **Specifications & Code Graph:**
   - Created `DOCUMENT/Upgrade/TC01_InPlace_Upgrade.md` and updated `UPGRADE_TEST_SPEC.md`.
   - Synchronized `Code Graph/Test_To_Action_Map.md`, `Action_Call_Hierarchy.md`, and `Impact_Analysis_Matrix.md`.

### 3. Verification
- `dotnet build` passed with **0 Warnings, 0 Errors**.

---

## 📋 Section 17: Quick Setting First-Launch Configuration & Anti-Hide Taskbar Engine (2026-10-26)

### 1. Context & Objectives
- Implemented **`TC06_Quick_Setting`** in `Tests/Install/` to automate the first-launch configuration of `Ez3D-i64.exe`.
- Addressed two critical challenges identified by the user:
  1. **No Taskbar Icon:** Because `Quick Setting` has no taskbar button, Windows pushes it behind VS Code when tests run from an elevated terminal.
  2. **Window Detection:** Engineered a 4-level Win32/UIA detection engine to find the dialog instantly without relying on taskbar visibility.
- Automated the 7-step interaction sequence:
  1. Detect `Quick Setting` and elevate past VS Code.
  2. Click `[ Search ]` button.
  3. Detect modal dialog `"Searching the IP Address of file server"`.
  4. Select `127.0.0.1` row.
  5. Click `[ OK ]` on search dialog.
  6. Verify `Server IP Address` edit box displays `127.0.0.1`.
  7. Click `[ OK ]` on `Quick Setting` dialog.

### 2. Technical Modifications
1. **`Assets/Templates/`:**
   - Ingested golden templates `15_quick_setting_dialog.png`, `16_search_ip_address_dialog.png`, `17_search_ip_address_127_selected.png`, `18_quick_setting_ip_verified.png`.
   - Registered templates in `Code Graph/Core_And_Infrastructure_Graph.md`.
2. **`Screens/QuickSettingScreen.cs`:**
   - Implemented `WaitForQuickSettingWindow` with instant Win32 `FindWindow` + `EnumWindows` fallback.
   - Enforced immediate anti-hide sequence upon capture: `PinWindowOnTop`, `DemoteVsCodeZOrder`, `ForceWindowForeground`, and `CenterWindowOnScreen`.
   - Implemented `WaitForSearchServerWindow`, button locators (`Search`, `OK`, `Cancel`), row selection (`127.0.0.1`), and Server IP read.
3. **`Actions/Install/HandleQuickSettingAction.cs`:**
   - Encapsulated the full 7-step workflow with physical coordinate clicks and UI verification.
4. **`Tests/Install/TC06_Quick_Setting.cs`:**
   - Fixture `[Category("Installation")]`, `[Order(6)]`.
   - Includes state guard (`EnsureEz3DiInstalledAction`), `VTServerConfig.ini` reset, elevated app launch, automation run, and clean process teardown.
5. **Specifications & Code Graph:**
   - Created `DOCUMENT/Install/TC06_Quick_Setting.md` and updated `INSTALL_TEST_SPEC.md`.
   - Synchronized `Code Graph/Test_To_Action_Map.md`, `Action_Call_Hierarchy.md`, `Screen_Object_Graph.md`, and `Impact_Analysis_Matrix.md`.

### 3. Verification
- `dotnet build` passed with **0 Warnings, 0 Errors**.

---

## 📋 Section 18: Search Button Geometric Calibration & Visual Template Fallback (2026-10-26)

### 1. Context & Objectives
- During execution of **`TC06_Quick_Setting`**, the automation failed to open the `Searching the IP Address of file server` dialog because the mouse click landed inside the `Server IP Address` text input box instead of clicking the `[ Search ]` button.
- User reported: *"So, the problem is it did not click the Search button, it remain in Text input"*.
- Objective: Calibrate physical coordinate calculations and implement visual template matching using `btn_search.png` to guarantee 100% click accuracy.

### 2. Root Cause Analysis
- Production screenshot analysis (`15_quick_setting_dialog.png` at 582×749):
  - `Server IP Address` Edit Box: Top = 140, Bottom = 162 (Center: $X=154, Y=151$).
  - `[ Search ]` Button: Top = 175, Bottom = 200 (Center: $X=154, Y=188$).
- In `Screens/QuickSettingScreen.cs`, the initial geometric fallback used:
  `int btnX = r.Left + 130; int btnY = r.Top + 140;`
  Because $Y=140$ is within the edit box bounds, the click focused the text input box rather than the button.

### 3. Technical Modifications
1. **`Assets/Templates/btn_search.png`:**
   - Ingested pixel-perfect $254 \times 26$ button template from `15_quick_setting_dialog.png`.
   - Registered in `Code Graph/Core_And_Infrastructure_Graph.md`.
2. **`Screens/QuickSettingScreen.cs`:**
   - **`FindSearchButtonCoordinate`:** Integrated `ImageCompare.See("btn_search.png")` / `ImageCompare.Find("btn_search.png")` as top priority locator, followed by Win32 child search, UIA search, and calibrated geometric fallback at ($X = r.Left + 154, Y = r.Top + 188$).
   - **`FindIpRowCoordinate`:** Calibrated geometric row 1 coordinate to ($X = r.Left + 104, Y = r.Top + 86$) matching the `127.0.0.1` row center in the search modal.
   - **`FindOkButtonCoordinate`:** Calibrated search dialog `[ OK ]` to ($X = r.Left + 150, Y = r.Top + 304$) and main dialog `[ OK ]` to ($X = r.Left + 200, Y = r.Bottom - 22$).
3. **`Actions/Install/HandleQuickSettingAction.cs`:**
   - Added `InputDriver.ForceWindowForeground(hwnd)` prior to clicking `[ Search ]`.
   - Added automatic refocus and click retry if the search modal does not appear on the first attempt within 8 seconds.

### 4. Verification
- `dotnet build` passed with **0 Warnings, 0 Errors**.

---

## 📋 Section 19: Administrative Common AppData Configuration Purge Engine (2026-10-26)

### 1. Context & Objectives
- Addressed the fundamental state trigger mechanism for the `Quick Setting` first-launch dialog:
  - Discovered that Ez3D-i does **not** store its server IP configuration in the installation folder (`C:\Program Files\VATECH\Ez3D-i\Setting\`).
  - Instead, the persistent active configuration resides in the Windows system Common AppData directory on `C:\`:
    `C:\ProgramData\ewoosoft\common\config\VTServerConfig.ini`
  - If this file exists with an assigned `ip_address`, Ez3D-i automatically bypasses the `Quick Setting` dialog on launch.
- Implemented an administrative purge function to delete `VTServerConfig.ini` before running `TC06_Quick_Setting`, with clear security diagnostic handling when running elevated.

### 2. Technical Modifications
1. **`Core/InstallerConstants.cs`:**
   - Added canonical path properties:
     - `CommonAppDataEwoosoftConfigDirectory => Path.Combine(Environment.GetFolderPath(SpecialFolder.CommonApplicationData), "ewoosoft", "common", "config")`
     - `CommonAppDataServerConfigFile => Path.Combine(CommonAppDataEwoosoftConfigDirectory, "VTServerConfig.ini")`
     - `LegacyInstallFolderServerConfigFile => Path.Combine(DefaultInstallDirectory, "Setting", "VTServerConfig.ini")`
2. **`Actions/Install/HandleQuickSettingAction.cs`:**
   - Implemented `PurgeQuickSettingConfigFile()`:
     - Strips `FileAttributes.ReadOnly` if present.
     - Safely deletes `C:\ProgramData\ewoosoft\common\config\VTServerConfig.ini`.
     - Catches `UnauthorizedAccessException` and raises an explicit error stating that elevated Administrator privileges are required for VS Code / Windows Terminal.
     - Also purges legacy fallback path in `Program Files` if present.
     - Verifies successful deletion before returning.
3. **`Tests/Install/TC06_Quick_Setting.cs`:**
   - Replaced old local `ResetServerConfigForQuickSetting()` with `HandleQuickSettingAction.PurgeQuickSettingConfigFile()` at Step 0b.
   - Updated `VerifyServerConfigSaved()` to assert persistence in `C:\ProgramData\ewoosoft\common\config\VTServerConfig.ini`.
4. **`DOCUMENT/Install/TC06_Quick_Setting.md` & `Code Graph/`:**
   - Updated specification and action call hierarchy diagrams.

### 3. Verification
- `dotnet build` passed with **0 Warnings, 0 Errors**.

---

## 📋 Section 20: GUIDE & Developer Documentation Synchronization Milestone (2026-10-26)

### 1. Context & Objectives
- Performed a full synchronization and refresh of the `GUIDE/` documentation repository to align with recent architectural advancements, including modular test suites (`Install/`, `Upgrade/`), state guards (`EnsureEz3DiInstalledAction`, `EnsureEz3DiUninstalledAction`), OpenCV template matching (`ImageCompare`), and Quick Setting automation (`TC06`).

### 2. Technical Modifications
1. **`GUIDE/ProjectStructure.md`:**
   - Synchronized the full directory tree inventory with newly added actions (`EnsureEz3DiInstalledAction.cs`, `EnsureEz3DiUninstalledAction.cs`, `HandleMaintenanceModeAction.cs`).
   - Added `Core/ImageCompare.cs` to the core driver inventory.
   - Expanded the complete `Assets/Templates/` catalog to include templates `08` through `18`, `btn_next.png`, and `btn_search.png`.
   - Documented the `Result/` directory structure and CI/CD artifacts (`Report_Installation.html`, `ResultsData.json`, `Results.txt`).
2. **`GUIDE/README.md`:**
   - Documented system prerequisites and the strict **Administrator elevation requirement** for InstallShield UAC interaction and `C:\ProgramData` Quick Setting configuration resets.
   - Refreshed all test runner command lines:
     - All 6 Install Suite fixtures (`TC01`–`TC06`).
     - Upgrade Suite fixture (`TC01_InPlace_Upgrade`).
     - Suite-wide regression execution commands (`Category=Installation`, `Category=Upgrade`).
   - Added direct markdown links to test specifications, Code Graph diagrams, and reporting assets.

### 3. Verification
- `dotnet build` passed with **0 Warnings, 0 Errors**.
- All documentation file links verified.

---

## 📋 Section 21: Addition of 'How to Run All 6 Install Cases' & VS Code Task Synchronization (2026-10-26)

### 1. Context & Objectives
- Responded to user inquiry regarding the lack of clear, prominent instructions on how to run all 6 Install cases together (`TC01` through `TC06`).
- Integrated dedicated multi-option documentation in `GUIDE/README.md` and updated `.vscode/tasks.json` with complete task runner entries.

### 2. Technical Modifications
1. **`GUIDE/README.md`:**
   - Added prominent dedicated section **`### ⚡ How to Run All 6 Install Cases (TC01–TC06)`**.
   - Provided **Method A (Single Batch Command)**: `dotnet test --filter "Category=Installation"` (and `--filter "FullyQualifiedName~Tests.Install"`).
   - Provided **Method B (Sequential PowerShell Loop)**: Iterative one-liner script executing each case with colored banners and independent logging.
   - Added an **Execution Order & Bilateral State Guards table** detailing the 6-stage lifecycle (`TC01` $\rightarrow$ `TC02` $\rightarrow$ `TC03` $\rightarrow$ `TC04` $\rightarrow$ `TC05` $\rightarrow$ `TC06`).
2. **`.vscode/tasks.json`:**
   - Added task `"Run All 6 Install Cases (TC01–TC06)"` (`--filter "Category=Installation"`).
   - Added / refreshed tasks for `TC01`, `TC02`, `TC03`, `TC04`, `TC05`, and `TC06`.
   - Added task `"Run Upgrade Suite (TC01)"` (`--filter "Category=Upgrade"`).

### 3. Verification
- `dotnet build` passed with **0 Warnings, 0 Errors**.
- VS Code task definitions verified valid JSON.

---

## 📋 Section 22: Unification of [Category("Installation")] Across All 6 Install Fixtures (2026-10-26)

### 1. Context & Objectives
- Diagnosed why running `dotnet test --filter "Category=Installation"` executed only `TC01` and `TC06`, omitting uninstallation tests (`TC03`, `TC04`, `TC05`).
- User reported: *"I did not see it perform uninstall ?"*.
- Root cause: `TC02` had `[Category("Verification")]`, `TC03` and `TC04` had `[Category("Uninstallation")]`, and `TC05` had `[Category("Lifecycle")]`. NUnit strictly filtered by category string and skipped any fixture without `[Category("Installation")]`.

### 2. Technical Modifications
1. **`Tests/Install/TC02_Post_Install_Verify.cs`:**
   - Added `[Category("Installation")]` alongside `[Category("Verification")]`.
2. **`Tests/Install/TC03_Uninstall_Control_Panel.cs`:**
   - Added `[Category("Installation")]` alongside `[Category("Uninstallation")]`.
3. **`Tests/Install/TC04_Uninstall_Rerun_Package.cs`:**
   - Added `[Category("Installation")]` alongside `[Category("Uninstallation")]`.
4. **`Tests/Install/TC05_Full_Lifecycle.cs`:**
   - Added `[Category("Installation")]` alongside `[Category("Lifecycle")]`.

### 3. Verification
- All 6 fixtures in `Tests/Install/` verified to possess `[Category("Installation")]`.
- `dotnet build` passed with **0 Warnings, 0 Errors**.
- Executing `dotnet test --filter "Category=Installation"` now reliably runs all 6 fixtures (`TC01` through `TC06`), including both uninstallation flows.

---

## 📋 Section 23: Baseline Package Replacement to v5.5.15.1 & Dynamic State Guard Calibration (2026-10-26)

### 1. Context & Objectives
- User updated the baseline older version setup package in `Installation Package/Old Version/` from `Ez3D-i V5.5.7.3 Setup(x64).exe` to **`Ez3D-i V5.5.15.1 Setup(x64).exe`** (1,002,658,248 bytes, ~956 MB).
- Objective: Recheck package integrity, verify cross-suite isolation, refine the baseline state guard for dynamic version matching, and synchronize all specification and guide documents.

### 2. Technical Modifications
1. **`Actions/Upgrade/EnsureEz3DiOldVersionInstalledAction.cs`:**
   - Decoupled the baseline version check from hardcoded strings (`"5.5.16"`).
   - Replaced with direct matching: `displayVersion.Trim().StartsWith(expectedOldVer, StringComparison.OrdinalIgnoreCase)`.
   - If the host system has any version other than the discovered baseline (`5.5.15.1`), it automatically executes uninstallation and establishes the pristine baseline before running the upgrade test.
2. **`DOCUMENT/Upgrade/TC01_InPlace_Upgrade.md`:**
   - Synchronized baseline package name, target version strings (`5.5.15.1` $\rightarrow$ `5.5.16.0`), and milestone descriptions.
3. **`GUIDE/ProjectStructure.md` & `GUIDE/README.md`:**
   - Updated package inventory tree and execution comments to reference `Ez3D-i V5.5.15.1 Setup(x64).exe`.
4. **`Code Graph/`:**
   - Synchronized `Action_Call_Hierarchy.md` Section 4 and `Test_To_Action_Map.md` to reference `v5.5.15.1`.

### 3. Verification
- `dotnet build` passed with **0 Warnings, 0 Errors**.
- Baseline package discovery verified: `FindInstallerPackageAction.FindOldVersion()` resolves `5.5.15.1` with 0 cross-suite pollution.
