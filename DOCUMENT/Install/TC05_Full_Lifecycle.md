# TC05: Full Installation & Uninstallation Lifecycle

## Metadata
- **Feature:** End-to-End Installation Lifecycle Verification
- **Type:** Integration / End-to-End Composite Flow
- **Priority:** High
- **Status:** Ready
- **Author:** QA Automation Team

## Prerequisites
- Host workstation with Windows 10/11 x64.
- Setup installer package available in `Installation Package/` or `D:\Test product\Ez3D-i\`.
- Elevated Administrator privileges.

## Workflow Phases
1. **Phase 1: Clean Installation:**
   - Execute `LaunchInstallerAction` $\rightarrow$ `HandleLanguageSelectionAction` $\rightarrow$ `StepThroughSetupWizardAction` $\rightarrow$ `WaitForInstallProgressAction` $\rightarrow$ `CompleteInstallationAction`.
   - Result: Ez3D-i deployed successfully.

2. **Phase 2: Post-Install State Verification:**
   - Execute `VerifyInstallationStateAction` asserting:
     - `Bin\Ez3D-i64.exe` exists.
     - `Desktop\Ez3D-i.lnk` shortcut exists.
     - Windows Registry key exists with matching version.

3. **Phase 3: Clean Uninstallation:**
   - Execute `LaunchUninstallerAction` $\rightarrow$ `ConfirmUninstallationAction` (Confirm Yes $\rightarrow$ Progress $\rightarrow$ Finish).
   - Result: Uninstallation completed.

4. **Phase 4: Post-Uninstall Cleanup Verification:**
   - Execute `VerifyUninstallationStateAction` asserting:
     - Binary is removed.
     - Desktop shortcut is deleted.
     - Windows Registry entry is purged.

## Expected Result
- Complete end-to-end lifecycle passes without any manual user intervention.
- System state is completely verified both post-install and post-uninstall.
