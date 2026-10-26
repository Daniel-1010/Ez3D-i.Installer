# TC04: Uninstallation via Rerunning Setup Package

## Metadata
- **Feature:** Application Uninstallation (Case 2: Rerun Package Maintenance Mode)
- **Type:** Functional / Negative (Maintenance Wizard)
- **Priority:** High
- **Status:** Ready
- **Author:** QA Automation Team

## Prerequisites
- Host machine running Windows 10/11 x64.
- Ez3D-i installed on host machine (automatically deployed via `EnsureEz3DiInstalledAction` if machine is clean, guaranteeing Maintenance Mode triggers upon rerunning setup).
- Ez3D-i original setup executable is available in `Installation Package/` or `D:\Test product\Ez3D-i\`.
- Elevated Administrator privileges.

## Test Steps
0. **Step 0: Prerequisite Deployment Check:**
   - Execute `EnsureEz3DiInstalledAction` checking Registry and File System.
   - If Ez3D-i is missing (e.g. from prior uninstallation run), automatically perform clean installation first so Maintenance Mode is guaranteed.

1. **Step 1: Rerun the Installation Package:**
   - Execute the setup package (`Ez3D-i V5.5.12.3 Setup(x64).exe`) with elevated permissions on an already-installed system.
   - If prompted for language ("Choose Setup Language"), click **[ OK ]**.
   - Capture screenshot `TC04_Step1_PackageLaunched.png`.

2. **Step 2: Select Remove Radio Button:**
   - Detect the InstallShield Maintenance Mode page (*Welcome / Modify, repair, or remove the program*).
   - Locate the **"Remove"** radio button (switching away from the default *Repair* option).
   - Send physical coordinate mouse click (`InputDriver.ClickAtPhysicalCoordinate`) into the radio circle.
   - Settle 500ms to allow InstallScript to update UI state.
   - Capture screenshot `TC04_Step2_RemoveSelected.png`.

3. **Step 3: Click Next Button:**
   - Locate the **[ Next > ]** navigation button.
   - Click the button using physical coordinate click.
   - Capture screenshot `TC04_Step3_NextClicked.png`.

4. **Step 4: Message Displayed > Click Yes Button:**
   - Modal confirmation dialog appears (*"Do you want to completely remove the selected application and all of its features?"*).
   - Locate and physically click the **[ Yes ]** button.
   - Capture screenshot `TC04_Step4_ConfirmYesClicked.png`.

5. **Step 5: Wait Progress Finished > Click Finish Button:**
   - Wait for the setup status / file and registry removal progress bar to complete.
   - Detect the *Uninstall Complete* page (*"InstallShield Wizard has finished uninstalling Ez3D-i."*).
   - Capture screenshot `TC04_Step5_ProgressCompleted.png`.
   - Locate and click the **[ Finish ]** button.
   - Capture screenshot `TC04_Step5_FinishClicked.png`.
   - Wait for the setup process to exit cleanly.

6. **Post-Uninstall Cleanup Verification:**
   - Assert `C:\Program Files\VATECH\Ez3D-i\Bin\Ez3D-i64.exe` is deleted (`File.Exists == false`).
   - Assert `C:\Users\Public\Desktop\Ez3D-i.lnk` is deleted (`File.Exists == false`).
   - Assert Windows Registry uninstall key is removed (`RegistryHelper.IsInstalled == false`).
   - Capture screenshot `TC04_Step6_CleanupVerified.png`.

## Expected Result
- Setup package launches and recognizes existing installation, displaying Maintenance Mode.
- "Remove" radio button is selected and accepted via `[ Next > ]`.
- Removal confirmation dialog is confirmed with `[ Yes ]`.
- Progress bar completes without errors.
- Finish dialog is dismissed with `[ Finish ]`.
- All binaries, shortcuts, and registry entries are cleanly purged from the system.
