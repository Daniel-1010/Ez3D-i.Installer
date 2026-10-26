# Ez3D-i.Installer — Impact Analysis Matrix

Reverse dependency tracking and risk blast-radius evaluation for changes across the framework.

---

## 📌 Risk Tiers & Blast Radius

| Tier | Component Type | Files | Risk Level | Dependent Test Suites |
| :--- | :--- | :--- | :--- | :--- |
| **Tier 1** | **Core Drivers** | `NativeMethods.cs`, `InputDriver.cs`, `WindowHelper.cs` | 🔴 **CRITICAL** | All Tests (`TC01`–`TC06`, `Upgrade/TC01`) |
| **Tier 2** | **Lifecycle Helpers** | `ProcessHelper.cs`, `RegistryHelper.cs`, `InstallerConstants.cs` | 🟠 **HIGH** | `TC01`–`TC06`, `Upgrade/TC01` |
| **Tier 3** | **Screen Objects** | `InstallerWizardScreen.cs`, `UninstallerWizardScreen.cs`, `QuickSettingScreen.cs` | 🟡 **MEDIUM** | `TC01`, `TC03`, `TC04`, `TC05`, `TC06`, `Upgrade/TC01` |
| **Tier 4** | **Action Keywords** | `StepThroughSetupWizardAction.cs`, `WaitForInstallProgressAction.cs`, `CompleteInstallationAction.cs`, `HandleMaintenanceModeAction.cs`, `EnsureEz3DiInstalledAction.cs`, `EnsureEz3DiUninstalledAction.cs`, `EnsureEz3DiOldVersionInstalledAction.cs`, `HandleUpgradeWizardAction.cs`, `HandleQuickSettingAction.cs` | 🟡 **MEDIUM** | `TC01`, `TC03`, `TC04`, `TC05`, `TC06`, `Upgrade/TC01` |
| **Tier 5** | **Test Fixtures** | `Tests/Install/TC01...` to `TC06...`, `Tests/Upgrade/TC01_InPlace_Upgrade.cs` | 🟢 **LOW** | Isolated single test fixture |

---

## 🔍 Reverse Dependency Matrix

| If You Modify... | Impacted Actions | Impacted Tests |
| :--- | :--- | :--- |
| **`InputDriver.cs`** | `HandleLanguageSelectionAction`<br>`StepThroughSetupWizardAction`<br>`CompleteInstallationAction`<br>`ConfirmUninstallationAction` | `TC01`, `TC03`, `TC04` |
| **`ProcessHelper.cs`** | `LaunchInstallerAction`<br>`CompleteInstallationAction`<br>`LaunchUninstallerAction`<br>`ConfirmUninstallationAction` | `TC01`, `TC03`, `TC04` |
| **`RegistryHelper.cs`** | `VerifyInstallationStateAction`<br>`LaunchUninstallerAction`<br>`VerifyUninstallationStateAction` | `TC02`, `TC03`, `TC04` |
| **`InstallerWizardScreen.cs`** | `LaunchInstallerAction`<br>`StepThroughSetupWizardAction`<br>`WaitForInstallProgressAction`<br>`CompleteInstallationAction` | `TC01`, `TC04` |
| **`StepThroughSetupWizardAction.cs`** | *(Direct caller)* | `TC01`, `TC04` |
