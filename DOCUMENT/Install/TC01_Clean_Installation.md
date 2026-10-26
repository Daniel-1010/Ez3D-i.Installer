# TC01: Clean Installation

## Metadata
- **Feature:** End-to-End Installation
- **Type:** Functional / Positive
- **Priority:** High
- **Status:** Ready
- **Author:** QA Automation Team

## Prerequisites
- Windows 10/11 x64 with Administrator privileges.
- Target setup package (`*.exe`) placed in `Installation Package/` or fallback repository `D:\Test product\Ez3D-i\`.
- Host system has no prior corrupted or half-installed Ez3D-i instances.

## Test Steps
1. **Discovery:** Scan `Installation Package/` for setup executable; if empty, scan `D:\Test product\Ez3D-i\`.
2. **Launch:** Terminate any lingering `Setup`, `_is`, or `msiexec` processes; launch installer with elevated privileges (`runas`).
3. **Language Selection:** Detect and confirm language dialog (`[ OK ]`) if prompted.
4. **Setup Wizard 5-Step Execution:**
   - **Step 1:** On *Welcome to the InstallShield Wizard for Ez3D-i*, click **[ Next > ]**.
   - **Step 2:** On *License Agreement*, click the radio button circle for **"I accept the terms of the license agreement"** (triggers Win32 `BN_CLICKED` to unlock Next).
   - **Step 3:** On *License Agreement*, click **[ Next > ]** (now enabled).
   - **Step 4:** On *Choose Destination Location* (`C:\Program Files\VATECH`), click **[ Next > ]**.
   - **Step 5:** On *Ready to Install the Program*, click **[ Install ]**.
5. **Progress Monitoring:** Wait for the green progress bar to complete file extraction and copying (timeout up to 15 minutes). Auto-confirm any prerequisite popups.
6. **Completion:** On *InstallShield Wizard Complete*, uncheck any launch checkboxes, select "Restart later" if prompted, click **[ Finish ]**, and wait for process termination.

## Expected Result
- Installer wizard navigates all 5 pages cleanly without blocking or hanging.
- Setup file extraction and registry registration completes.
- Setup process exits cleanly within 30 seconds of clicking Finish.
- Screenshots are captured at each milestone: `TC01_Step1_InstallerLaunched.png`, `TC01_Step3_Step1_Welcome_NextClicked.png`, `TC01_Step3_Step2_LicenseAccepted.png`, `TC01_Step3_Step3_License_NextClicked.png`, `TC01_Step3_Step4_Destination_NextClicked.png`, `TC01_Step3_Step5_InstallClicked.png`, `TC01_Step4_ProgressFinished.png`, `TC01_Step5_InstallationCompleted.png`.

## Environment & Data
- **Target Target:** `C:\Program Files\VATECH\Ez3D-i\Bin\Ez3D-i64.exe`
- **Output Result Folder:** `Result/TC01_Clean_Installation/`
- **Frameworks:** .NET 10.0 / FlaUI UIA3 / OpenCVSharp4 / NUnit 4.3
