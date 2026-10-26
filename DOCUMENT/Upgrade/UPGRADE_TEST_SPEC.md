# Ez3D-i Upgrade Test Automation Specification

## 1. Overview
This specification suite defines the automated testing workflows for Ez3D-i application version upgrades (e.g. upgrading an existing v5.5.12.3 installation to v5.5.16.0).

- **Target OS:** Windows 10/11 x64 (Elevated Administrator)
- **Framework:** .NET 10.0-windows, FlaUI UIA3, NUnit 4.3, OpenCvSharp4
- **Target Suite:** `Tests/Upgrade/`

---

## 2. Test Cases Catalog

| Test Case | Method | Description | Status | Specification |
| :--- | :--- | :--- | :--- | :--- |
| **TC01** | `Run_TC01_InPlace_Upgrade()` | Executes in-place version upgrade from baseline older version to v5.5.16.0. | ✅ **Implemented** | [`TC01_InPlace_Upgrade.md`](TC01_InPlace_Upgrade.md) |
| **TC02** | `Run_TC02_Upgrade_Preserve_Data()` | Verifies patient data, configuration files, and registry settings are preserved post-upgrade. | ⏳ Planned | Planned |
| **TC03** | `Run_TC03_Rollback_On_Cancel()` | Verifies that canceling an upgrade wizard gracefully rolls back without corrupting the existing install. | ⏳ Planned | Planned |

---

## 3. General Upgrade Workflow
1. **Prerequisite:** Pre-install target baseline version (e.g. Ez3D-i v5.5.12.3).
2. **Execute Updater:** Launch newer version setup package (e.g. Ez3D-i v5.5.16.0 Setup).
3. **Detect Upgrade Prompt:** Handle upgrade confirmation / overwrite wizard pages.
4. **Monitor Update Progress:** Wait for file replacement and database migration progress bar.
5. **Complete:** Click [ Finish ] on completion page.
6. **Verify Post-Upgrade State:** Assert updated file versions in `Bin\Ez3D-i64.exe` and updated registry `DisplayVersion`.
