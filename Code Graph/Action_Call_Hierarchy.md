# Ez3D-i.Installer — Action Call Hierarchy

Detailed call hierarchies and execution flow diagrams for all Action implementations.

---

## 1. Installation Flow

```mermaid
flowchart TD
    subgraph PreClean["EnsureEz3DiUninstalledAction"]
        CheckInstalled["Check RegistryHelper.IsInstalled() & File.Exists()"]
        CheckInstalled -- Installed --> AutoUninstall["Run Uninstallation Actions\n(Launch -> Confirm -> Progress -> Finish)"]
        CheckInstalled -- Clean --> CleanPass["Bypass (0 Overhead)"]
    end

    subgraph Launch["LaunchInstallerAction"]
        Find["FindInstallerPackageAction.Execute()"] --> Kill["ProcessHelper.KillProcessesByName()"]
        Kill --> Start["ProcessHelper.StartElevated()"]
        Start --> WaitWin["InstallerWizardScreen.WaitForWizardWindow()"]
    end

    subgraph Step5["StepThroughSetupWizardAction (5 Steps)"]
        S1["Step 1: Click Next on Welcome"] --> S2["Step 2: Select 'I accept...' Radio"]
        S2 --> S3["Step 3: Click Next on License (Enabled)"]
        S3 --> S4["Step 4: Click Next on Destination"]
        S4 --> S5["Step 5: Click Install on Ready Screen"]
    end

    subgraph Progress["WaitForInstallProgressAction"]
        Loop["Wait Loop (15m Timeout)"] --> CheckFinish{"Finish Button Enabled?"}
        CheckFinish -- Yes --> ProgDone["Signal Progress Finished"]
        CheckFinish -- No --> CheckModal["Dismiss Prerequisite Popups (#32770)"]
    end

    subgraph Complete["CompleteInstallationAction"]
        Uncheck["Uncheck Launch Checkbox"] --> ClickFinish["Click Finish Button"]
        ClickFinish --> WaitExit["ProcessHelper.WaitForExit()"]
    end

    Launch --> Step5 --> Progress --> Complete
```

---

## 2. Uninstallation & Verification Flow

```mermaid
flowchart TD
    subgraph Verify["VerifyInstallationStateAction"]
        CheckBin["Assert File.Exists(Bin/Ez3D-i64.exe)"]
        CheckShort["Assert File.Exists(Desktop Shortcut)"]
        CheckReg["RegistryHelper.IsInstalled()"]
        CheckBin --> CheckShort --> CheckReg
    end

    subgraph Guard["EnsureEz3DiInstalledAction"]
        Check["Check RegistryHelper.IsInstalled() & File.Exists()"]
        Check -- Not Installed --> AutoInstall["Run Full Installation Actions\n(Launch -> License -> Progress -> Finish)"]
        Check -- Installed --> Pass["Bypass (0 Overhead)"]
    end

    subgraph UninstallCP["Uninstallation Action Suite (Case 1: Control Panel - TC03)"]
        ULaunch["LaunchUninstallerAction\n(Reads UninstallString from Registry)"]
        UConfirm["ConfirmUninstallationAction\n(Step 1: Yes -> Step 2: Progress -> Step 3: Finish)"]
        UVerify["VerifyUninstallationStateAction\n(Asserts binary, shortcut & registry removed)"]
        ULaunch --> UConfirm --> UVerify
    end

    subgraph UninstallRerun["Uninstallation Action Suite (Case 2: Rerun Package - TC04)"]
        RLaunch["LaunchInstallerAction\n(Rerun setup package)"]
        RLang["HandleLanguageSelectionAction\n(Handle language dialog)"]
        RMaint["HandleMaintenanceModeAction\n(Select 'Remove' radio & Click Next >)"]
        RConfirm["ConfirmUninstallationAction\n(Step 1: Yes -> Step 2: Progress -> Step 3: Finish)"]
        RVerify["VerifyUninstallationStateAction\n(Asserts binary, shortcut & registry removed)"]
        RLaunch --> RLang --> RMaint --> RConfirm --> RVerify
    end

    Guard --> UninstallCP
    Guard --> UninstallRerun
```

---

## 3. Package Discovery & Upgrade Isolation Flow

```mermaid
flowchart TD
    subgraph Discovery["FindInstallerPackageAction"]
        DiscExec["FindInstallerPackageAction.Execute()\n(Scans ONLY root 'Installation Package/'\nExcludes 'Old Version/')"]
        DiscOld["FindInstallerPackageAction.FindOldVersion()\n(Scans strictly 'Installation Package/Old Version/')"]
        VersionExtract["Extract Version via Regex [vV]?(\\d+\\.\\d+(\\.\\d+)*)\n(Decouples from 16.0.400 InstallShield wrapper)"]
        DiscExec --> VersionExtract
        DiscOld --> VersionExtract
    end

    subgraph InstallSuite["Install Test Suite (TC01-TC05)"]
        TC_Install["Clean Install / Verify / Uninstall"]
    end

    subgraph UpgradeSuite["Upgrade Test Suite (TC01)"]
        TC_Upgrade["Baseline Deployment -> In-Place Upgrade"]
    end

    DiscExec --> TC_Install
    DiscOld --> UpgradeSuite
```

---

## 4. In-Place Upgrade Execution Flow (`TC01_InPlace_Upgrade`)

```mermaid
flowchart TD
    subgraph PreGuard["EnsureEz3DiOldVersionInstalledAction"]
        CheckOld["Check RegistryHelper & Filesystem"]
        CheckOld -- Old Version Installed --> OldPass["Bypass (0 Overhead)"]
        CheckOld -- Target v5.5.16 Installed --> AutoClean["EnsureEz3DiUninstalledAction\n(Purge v5.5.16)"]
        AutoClean --> DeployOld["Deploy Baseline v5.5.15.1\n(Launch -> 5 Steps -> Progress -> Finish)"]
        CheckOld -- Clean System --> DeployOld
    end

    subgraph UpgradeExec["Upgrade Execution Flow"]
        LaunchTarget["LaunchInstallerAction\n(Launch v5.5.16.0)"]
        LangTarget["HandleLanguageSelectionAction\n(Handle language dialog)"]
        WelcomeTarget["HandleUpgradeWizardAction\n('...will update installed version...'\nClick [ Next > ])"]
        ProgressTarget["WaitForInstallProgressAction\n(Wait progress finished)"]
        FinishTarget["CompleteInstallationAction\n(Click [ Finish ] on Update Complete)"]
        VerifyTarget["VerifyInstallationStateAction\n(Assert v5.5.16.0 in registry & binary)"]

        LaunchTarget --> LangTarget --> WelcomeTarget --> ProgressTarget --> FinishTarget --> VerifyTarget
    end

    PreGuard --> UpgradeExec
```

---

## 5. Quick Setting Execution Flow (`TC06_Quick_Setting`)

```mermaid
flowchart TD
    subgraph Pre["Step 0: Precondition"]
        GuardInstall["EnsureEz3DiInstalledAction\n(Verify Ez3D-i installed)"]
        ResetConfig["HandleQuickSettingAction.PurgeQuickSettingConfigFile()\n(Delete C:\\ProgramData\\ewoosoft\\...\\VTServerConfig.ini)"]
        GuardInstall --> ResetConfig
    end

    subgraph LaunchApp["Step 1: Launch Application"]
        StartApp["ProcessHelper.StartElevated(Ez3D-i64.exe)"]
    end

    subgraph QuickSet["HandleQuickSettingAction"]
        WaitQS["QuickSettingScreen.WaitForQuickSettingWindow()\n(Multi-level Win32 FindWindow + Anti-Hide Elevation)"]
        Pin["InputDriver.PinWindowOnTop() & DemoteVsCodeZOrder()"]
        ClickSearch["Click [ Search ] Button"]
        WaitSearch["QuickSettingScreen.WaitForSearchServerWindow()\n('Searching the IP Address of file server')"]
        SelectIp["Click '127.0.0.1' Row"]
        ClickSearchOk["Click [ OK ] on Search Dialog"]
        VerifyIp["Assert Server IP Address == '127.0.0.1'"]
        ClickMainOk["Click [ OK ] on Quick Setting Dialog"]

        WaitQS --> Pin --> ClickSearch --> WaitSearch --> SelectIp --> ClickSearchOk --> VerifyIp --> ClickMainOk
    end

    subgraph CleanUp["Teardown"]
        KillApp["ProcessHelper.KillProcessesByName('Ez3D-i64')"]
    end

    Pre --> LaunchApp --> QuickSet --> CleanUp
```

