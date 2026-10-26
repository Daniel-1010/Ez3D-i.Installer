# TC02: Post-Installation State Verification

## Metadata
- **Feature:** Installation State Verification
- **Type:** Functional / Positive / Assertion
- **Priority:** High
- **Status:** Ready
- **Author:** QA Automation Team

## Prerequisites
- Ez3D-i has completed installation on the host machine (`TC01`).

## Test Steps
1. **Executable Presence:** Assert that `Ez3D-i64.exe` exists in the installation directory (`C:\Program Files\VATECH\Ez3D-i\Bin\Ez3D-i64.exe`).
2. **Version Metadata:** Read `FileVersionInfo` and assert file version and product version match expected package version.
3. **Desktop Shortcut:** Assert that the public desktop shortcut exists (`C:\Users\Public\Desktop\Ez3D-i.lnk`).
4. **Registry Hive Registration:** Query Windows Uninstall Registry hives (`HKLM\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall` and 64-bit hive):
   - Assert `DisplayName` contains `"Ez3D-i"`.
   - Assert `DisplayVersion` is non-empty and matches installed build.
   - Assert `UninstallString` is present and valid.

## Expected Result
- Binary file exists and is executable.
- Desktop shortcut is created on the public desktop.
- Registry uninstall entry is registered correctly in `WOW6432Node`.
- Assertions pass with 0 errors.

## Environment & Data
- **Executable Path:** `C:\Program Files\VATECH\Ez3D-i\Bin\Ez3D-i64.exe`
- **Shortcut Path:** `C:\Users\Public\Desktop\Ez3D-i.lnk`
- **Output Result Folder:** `Result/TC02_Post_Install_Verify/`
