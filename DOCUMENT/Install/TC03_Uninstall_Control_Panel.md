# TC03: Uninstallation via Windows Control Panel

## Metadata
- **Feature:** Application Uninstallation (Case 1: Control Panel)
- **Type:** Functional / Negative (Cleanup)
- **Priority:** High
- **Status:** Ready
- **Author:** QA Automation Team

## Prerequisites
- Host machine running Windows 10/11 x64.
- Ez3D-i installed and registered in Windows Registry (automatically deployed via `EnsureEz3DiInstalledAction` if not currently installed).
- Elevated Administrator privileges.

## Test Steps
0. **Step 0: Prerequisite Deployment Check:**
   - Execute `EnsureEz3DiInstalledAction` checking Registry and File System.
   - If Ez3D-i is missing (e.g. from prior uninstallation run), automatically perform clean installation first so uninstallation can be validated.

1. **Step 1: Uninstaller Discovery & Launch:**
   - Query the Windows Registry for Ez3D-i's registered `UninstallString` (e.g. `"C:\Program Files (x86)\InstallShield Installation Information\{B3B77B64-AA2E-42CE-A47C-20F1F15A7EDD}\setup.exe" -runfromtemp -l0x0409 -removeonly`).
   - Launch the uninstaller command with elevated permissions.
   - Wait up to 30 seconds for the uninstaller dialog to appear.

2. **Step 2: Confirm Removal Prompt:**
   - Detect the confirmation message dialog with title `Ez3D-i - InstallShield Wizard` and text:
     *"Do you want to completely remove the selected application and all of its features?"*
   - Locate and physically click the **[ Yes ]** button.
   - Capture screenshot `TC03_Step1_ConfirmYesClicked.png`.

3. **Step 3: Monitor Progress Bar:**
   - Wait for the setup status / file removal progress bar to run.
   - Log elapsed time every 10 seconds (timeout up to 8 minutes).
   - Capture screenshot `TC03_Step2_ProgressCompleted.png`.

4. **Step 4: Complete Uninstallation:**
   - Detect the *Uninstall Complete* dialog with text:
     *"InstallShield Wizard has finished uninstalling Ez3D-i."*
   - Locate and physically click the **[ Finish ]** button.
   - Capture screenshot `TC03_Step3_FinishClicked.png`.
   - Wait for the uninstaller process to exit cleanly.

5. **Step 5: Post-Uninstall Cleanup Verification:**
   - Assert `C:\Program Files\VATECH\Ez3D-i\Bin\Ez3D-i64.exe` is deleted (`File.Exists == false`).
   - Assert `C:\Users\Public\Desktop\Ez3D-i.lnk` is deleted (`File.Exists == false`).
   - Assert Windows Registry uninstall key is removed (`RegistryHelper.IsInstalled == false`).
   - Capture screenshot `TC03_Step4_CleanupVerified.png`.

## Expected Result
- Uninstaller launches successfully from the Windows Control Panel command.
- Confirmation dialog is accepted with `[ Yes ]`.
- Progress bar runs and completes without error.
- Finish dialog is dismissed with `[ Finish ]`.
- All binaries, shortcuts, and registry entries are completely removed from the operating system.
