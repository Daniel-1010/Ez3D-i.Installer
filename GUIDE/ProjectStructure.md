# Ez3D-i.Installer — Project Structure

Complete directory and component inventory for `Ez3D-i.Installer`.

---

```
d:\Ez3D-i.Installer\
├── .agents/                                       # AI Agent Rules, Hooks & Calibration
│   ├── Lessons/
│   │   ├── 01_Failures_And_What_Went_Wrong.md     # Pitfall & root-cause case studies
│   │   └── 02_Lessons_Learned_And_AI_Guidelines.md# Best practices & engineering guidelines
│   ├── scripts/
│   │   └── pre-command-guard.ps1                  # Guard hook for elevated test execution
│   ├── hooks.json                                 # Agent hook definitions
│   └── RULE.md                                    # Comprehensive agent rules & spec index
├── .vscode/                                       # Editor configuration
│   └── tasks.json                                 # Pre-configured test runner tasks
├── Actions/                                       # High-level test actions (Keywords)
│   ├── Discovery/
│   │   └── FindInstallerPackageAction.cs          # Dynamic scanner for installer *.exe
│   ├── Install/
│   │   ├── EnsureEz3DiInstalledAction.cs          # Idempotent state guard for installed Ez3D-i
│   │   ├── LaunchInstallerAction.cs               # Elevated launcher for setup.exe
│   │   ├── HandleLanguageSelectionAction.cs       # Language prompt handler
│   │   ├── StepThroughSetupWizardAction.cs        # 5-step setup wizard workflow
│   │   ├── WaitForInstallProgressAction.cs        # Progress bar monitor (up to 15m)
│   │   ├── CompleteInstallationAction.cs          # Finish button click & process exit
│   │   └── HandleQuickSettingAction.cs            # Quick Setting, server IP search & config purge
│   ├── Verify/
│   │   └── VerifyInstallationStateAction.cs       # Post-install file/registry assert
│   ├── Upgrade/
│   │   ├── EnsureEz3DiOldVersionInstalledAction.cs# Idempotent baseline old version guard
│   │   └── HandleUpgradeWizardAction.cs           # Update welcome dialog step-through
│   └── Uninstall/
│       ├── EnsureEz3DiUninstalledAction.cs        # Idempotent state guard for clean host state
│       ├── HandleMaintenanceModeAction.cs         # Selects Remove radio & advances maintenance mode
│       ├── LaunchUninstallerAction.cs             # Reads registry UninstallString
│       ├── ConfirmUninstallationAction.cs         # Uninstaller step-through
│       └── VerifyUninstallationStateAction.cs     # Assert complete file/reg removal
├── Assets/                                        # Reference UI template images
│   └── Templates/
│       ├── 01_welcome_wizard.png                  # Welcome screen
│       ├── 02_license_agreement_unaccepted.png    # License screen (unaccepted)
│       ├── 03_license_agreement_accepted.png      # License screen (accepted)
│       ├── 04_destination_location.png            # Destination folder screen
│       ├── 05_ready_to_install.png                # Ready to install screen
│       ├── 06_setup_status_progress.png           # Setup status with progress bar
│       ├── 07_wizard_complete_finish.png          # Wizard complete with Finish
│       ├── 08_uninstall_confirm_prompt.png        # Uninstall confirmation dialog
│       ├── 09_uninstall_complete_finish.png       # Uninstall complete dialog
│       ├── 10_maintenance_mode_welcome.png        # Maintenance mode welcome (Repair / Remove)
│       ├── 11_maintenance_mode_remove_selected.png# Maintenance mode with Remove selected
│       ├── 12_upgrade_welcome.png                 # Upgrade welcome screen
│       ├── 13_upgrade_status_progress.png         # Upgrade status progress screen
│       ├── 14_upgrade_complete_finish.png         # Upgrade complete finish screen
│       ├── 15_quick_setting_dialog.png            # Quick Setting dialog (1st launch, empty IP)
│       ├── 16_search_ip_address_dialog.png        # Search server IP address dialog
│       ├── 17_search_ip_address_127_selected.png  # Search dialog with 127.0.0.1 selected
│       ├── 18_quick_setting_ip_verified.png       # Quick Setting with 127.0.0.1 populated
│       ├── btn_next.png                           # Dedicated Next button template
│       ├── btn_search.png                         # Dedicated Search button template
│       └── .gitkeep
├── Code Graph/                                    # Architecture & dependency knowledge graph
│   ├── Action_Call_Hierarchy.md                   # Call trees for all actions
│   ├── Architecture_Graph.md                      # 4-tier system architecture
│   ├── Core_And_Infrastructure_Graph.md           # P/Invoke, UIA3 & Assets catalog
│   ├── Impact_Analysis_Matrix.md                  # Blast radius & reverse dependency matrix
│   ├── README.md                                  # Code Graph index
│   ├── Screen_Object_Graph.md                     # Screen object models & locators
│   └── Test_To_Action_Map.md                      # Test-to-action coverage map
├── Core/                                          # Low-level infrastructure & Win32 drivers
│   ├── ImageCompare.cs                            # Multi-monitor visual OpenCV template matching
│   ├── InputDriver.cs                             # Hardware mouse & foreground pinning
│   ├── InstallerConstants.cs                      # Absolute paths, Common AppData, & registry keys
│   ├── NativeMethods.cs                           # Win32 User32/Kernel32 P/Invoke
│   ├── ProcessHelper.cs                           # Elevated process launcher & killer
│   ├── RegistryHelper.cs                          # 32/64-bit Registry reader
│   ├── ResultWriter.cs                            # HTML, JSON & text report generator
│   └── WindowHelper.cs                            # HWND extraction & bounds calculation
├── DOCUMENT/                                      # Test specifications
│   ├── Install/                                   # Install test specification suite
│   │   ├── INSTALL_TEST_SPEC.md                   # Installation test specification index
│   │   ├── TC01_Clean_Installation.md             # TC01 Spec
│   │   ├── TC02_Post_Install_Verify.md            # TC02 Spec
│   │   ├── TC03_Uninstall_Control_Panel.md        # TC03 Spec
│   │   ├── TC04_Uninstall_Rerun_Package.md        # TC04 Spec
│   │   ├── TC05_Full_Lifecycle.md                 # TC05 Spec
│   │   └── TC06_Quick_Setting.md                  # TC06 Spec
│   └── Upgrade/                                   # Upgrade test specification suite
│       ├── UPGRADE_TEST_SPEC.md                   # Upgrade test specification index
│       └── TC01_InPlace_Upgrade.md                # TC01 Upgrade Spec
├── GUIDE/                                         # Developer & automation guides
│   ├── ProjectStructure.md                        # This document
│   ├── README.md                                  # Quick-start guide
│   └── REVIEW.md                                  # Review log & calibration history
├── Installation Package/                          # Drop directory for target installer *.exe
│   ├── Old Version/                               # Isolated directory for baseline older versions (Upgrade suite)
│   │   ├── Ez3D-i V5.5.15.1 Setup(x64).exe        # Baseline older setup package
│   │   └── .gitkeep
│   └── .gitkeep
├── Result/                                        # Generated test output artifacts
│   ├── Report_Installation.html                   # Visual HTML test run dashboard
│   ├── ResultsData.json                           # Structured JSON results for CI/CD
│   └── Results.txt                                # Standard output test log
├── Screens/                                       # Page Object Models for UI automation
│   ├── InstallerWizardScreen.cs                   # InstallShield wizard locators
│   ├── UninstallerWizardScreen.cs                 # Uninstaller dialog locators
│   └── QuickSettingScreen.cs                      # Quick Setting & Server IP dialog locators
├── Tests/                                         # Test fixtures (NUnit 4.3)
│   ├── BaseInstallerFixture.cs                    # Test fixture base (screenshot & reporting)
│   ├── Install/                                   # Dedicated test suite folder for Install/Uninstall
│   │   ├── TC01_Clean_Installation.cs             # Test Case 01: Clean Installation
│   │   ├── TC02_Post_Install_Verify.cs            # Test Case 02: Verification
│   │   ├── TC03_Uninstall_Control_Panel.cs        # Test Case 03: Control Panel Uninstallation
│   │   ├── TC04_Uninstall_Rerun_Package.cs        # Test Case 04: Rerun Package Uninstallation
│   │   ├── TC05_Full_Lifecycle.cs                 # Test Case 05: Full End-to-End Lifecycle
│   │   └── TC06_Quick_Setting.cs                  # Test Case 06: Quick Setting 1st Launch Configuration
│   └── Upgrade/                                   # Dedicated test suite folder for Upgrades
│       ├── TC01_InPlace_Upgrade.cs                # Test Case UP01: In-Place Application Upgrade
│       └── .gitkeep
├── .gitignore
├── AGENTS.md                                      # Pinned AI rules
└── Ez3D-i.Installer.csproj                        # Project configuration (.NET 10.0-windows)
```
