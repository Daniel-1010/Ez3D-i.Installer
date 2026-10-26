# Ez3D-i.Installer — Architecture Graph

## 1. 4-Tier Architectural Hierarchy

```mermaid
flowchart TB
    subgraph T1["Tier 1: Test Suites"]
        TC01["TC01: Clean Installation"]
        TC02["TC02: Post-Install Verify"]
        TC03["TC03: Uninstallation"]
        TC04["TC04: Full Lifecycle"]
    end

    subgraph T2["Tier 2: Screen Objects"]
        IWS["InstallerWizardScreen"]
        UWS["UninstallerWizardScreen"]
    end

    subgraph T3["Tier 3: Modular Actions"]
        A_Find["FindInstallerPackageAction"]
        A_Launch["LaunchInstallerAction"]
        A_Lang["HandleLanguageSelectionAction"]
        A_Step["StepThroughSetupWizardAction"]
        A_Wait["WaitForInstallProgressAction"]
        A_Done["CompleteInstallationAction"]
        A_Verify["VerifyInstallationStateAction"]
        A_UnLaunch["LaunchUninstallerAction"]
        A_UnConfirm["ConfirmUninstallationAction"]
        A_UnVerify["VerifyUninstallationStateAction"]
    end

    subgraph T4["Tier 4: Core & Infrastructure"]
        C_Input["InputDriver"]
        C_Win["WindowHelper"]
        C_Proc["ProcessHelper"]
        C_Reg["RegistryHelper"]
        C_Res["ResultWriter"]
        C_Const["InstallerConstants"]
    end

    T1 --> T3
    T3 --> T2
    T2 --> T4
    T3 --> T4
```

---

## 2. Layer Responsibilities & Isolation Boundaries

1. **Tier 1 (Tests):**
   - Pure orchestration and assertions. No direct Win32 P/Invoke or low-level FlaUI automation element calls.
   - All tests inherit from [`BaseInstallerFixture`](file:///d:/Ez3D-i.Installer/Tests/BaseInstallerFixture.cs).

2. **Tier 2 (Screens):**
   - Responsible for locating UI elements within the InstallShield dialog (`#32770`) without business logic.
   - Wraps FlaUI UIA3 element queries with timeout tolerance.

3. **Tier 3 (Actions):**
   - High-level automated operations (e.g. stepping through the 5-step wizard, monitoring progress bar, checking registry keys).
   - Reusable across individual test fixtures and end-to-end composite fixtures.

4. **Tier 4 (Core & Infrastructure):**
   - Stateless helper methods for Win32 API interactions, mouse event simulation, process management, and reporting.
