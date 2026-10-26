# 02 - Lessons Learned And AI Guidelines

Permanent engineering rules, patterns, and guidelines for developing test automation against Windows InstallShield wizards.

---

## 📌 Core Engineering Guidelines

### 1. The 3-Tier Window Attachment Pattern
When interacting with modal or elevated setup wizards:
1. `NativeMethods.ShowWindow(hwnd, SW_RESTORE)` if iconic.
2. `InputDriver.PinWindowOnTop(hwnd)` setting `HWND_TOPMOST` and attaching thread input (`AttachThreadInput`).
3. Settle delay (150–200ms) before simulating hardware input.

### 2. Physical Coordinate Clicks for Win32 Controls
Never rely solely on UIA invoke/selection patterns for native Win32 controls (`#32770`):
* Always compute the physical bounding rectangle via `WindowHelper.GetWindowBounds(window)`.
* Click inside the target control boundary using `NativeMethods.mouse_event`.

### 3. Progressive Polling with Timeout Safeguards
* Setup wizards may perform disk I/O, prerequisite checks, or driver registrations that take several minutes.
* Use progressive polling intervals (`Thread.Sleep(1500)`) with periodic progress logging (every 10–15 seconds) and a 15-minute safety ceiling.

### 4. Zero Orphan Process Policy
Always terminate obsolete instances of `Setup`, `_is`, and `msiexec` before launching new installations using `ProcessHelper.KillProcessesByName`. In test teardown, verify any spawned process has exited or kill it within `finally` blocks.

### 5. Multi-Monitor Virtual Screen Awareness
In environments with multiple monitors:
* Never assume the installer is on `Screen.PrimaryScreen`. Always locate the active display using `WindowHelper.GetScreenForHwnd(hwnd)` or `Screen.FromHandle(hwnd)`.
* Do not rely on FlaUI's `IsOffscreen` for window existence; verify using Win32 `NativeMethods.IsWindowVisible(hwnd)` and `VirtualScreen` intersection.
* When using OpenCV visual matching (`ImageCompare`), capture the entire `SystemInformation.VirtualScreen` so templates are detected regardless of monitor placement.

### 6. The Anti-Hide Z-Order Elevation Rule
When tests are launched from an IDE (VS Code) on the same monitor as the target application:
* Windows foreground lock must be actively unlocked: pulse the `VK_MENU` (Alt) key and invoke `SwitchToThisWindow`.
* Apply `HWND_TOPMOST` with `SWP_SHOWWINDOW` and center the window on the active monitor's working area.
* Demote IDE processes (`Code.exe`) from the topmost layer using `SetWindowPos(HWND_NOTOPMOST)` so the IDE cannot cover the test wizard.

### 7. Dynamic Project Root Discovery Protocol
Never rely on hardcoded relative directory hops (`..\..\..\..\`) to find project roots:
* Target frameworks and output directories change across .NET versions and build configurations (e.g. `bin\Debug\net10.0-windows\` has 3 levels of depth, whereas other targets or shadow-copy runners may differ).
* Always walk up parent directories via `DirectoryInfo.Parent` starting from `AppDomain.CurrentDomain.BaseDirectory` until a project anchor (`.csproj`, `Assets/`, or `Solution.sln`) is discovered.
* Provide a safe fallback only if the loop terminates at the drive root.

### 8. Distinct Lifecycle Separation for Uninstallation Entry Points
Always segregate installer maintenance uninstallation from Windows Control Panel uninstallation:
* **Control Panel (`TC03`):** Uses the registered `UninstallString` with `-removeonly` flags, directly invoking the confirmation prompt dialog without wizard introduction.
* **Setup Package Rerun (`TC04`):** Launches `Setup.exe` against an existing installation, presenting InstallShield's maintenance mode. Always handle radio button selection (e.g., "Remove") and click `[ Next > ]` before chaining to the confirmation dialog.
* Both flows must converge on the shared verification action (`VerifyUninstallationStateAction`) to ensure complete removal of binaries, desktop shortcuts, and registry hives.

### 9. Prerequisite State Guard Pattern for Destructive Tests
When automating destructive or teardown tests (e.g. uninstallation, file cleanup):
* Always guard the test entry point with a prerequisite state check (`EnsureEz3DiInstalledAction`).
* If the pre-condition is met (application is present in registry and filesystem), proceed immediately with 0 delay.
* If the pre-condition is unmet (clean machine or post-teardown state), automatically bootstrap the required environment by executing the deployment pipeline first. This allows tests to run continuously in test suites without manual intervention.

### 10. Setup Wrapper Version Decoupling Protocol
Never trust `FileVersionInfo` on self-extracting setup executables (`Setup.exe`):
* InstallShield, Inno Setup, WiX, and NSIS bootstrapper launchers contain their own internal compiler version (e.g. `16.0.400` for InstallShield 2010), not the packaged software's version.
* Always inspect filename conventions via regex (`[vV]?(\d+\.\d+(\.\d+)*)`) to extract the real application release version.
* Ensure verification assertions ignore bootstrapper version strings (`16.0*`) and compare against the actual installed application binary (`Ez3D-i64.exe`) or Windows Registry `DisplayVersion`.

### 11. Bilateral State Idempotency Protocol
Ensure both installation and uninstallation suites are fully self-healing:
* **Installation Entry Points (`TC01`, `TC05`):** Must verify the host is clean via `EnsureEz3DiUninstalledAction` before launching `Setup.exe`. If a lingering installation is present, automatically uninstall it first to prevent InstallShield from launching into Maintenance Mode.
* **Uninstallation Entry Points (`TC03`, `TC04`):** Must verify the application is installed via `EnsureEz3DiInstalledAction` before attempting uninstallation. If missing, automatically deploy the application first.
* This bidirectional guard allows tests to be executed in any sequence (e.g. `TC04` before `TC01`, or continuous regression runs) without state collision.

### 12. Strict Test Package Boundary Isolation Protocol
When multiple versions of setup packages are required (e.g., baseline old version for `Upgrade` testing vs. current release for `Install` testing):
* Never store multiple installer versions together in the root package drop directory.
* Isolate historical or baseline installers in dedicated subdirectories (e.g. `Installation Package/Old Version/`).
* Package discovery engines for clean `Install` test suites must strictly scope searches to `SearchOption.TopDirectoryOnly` and filter out any secondary directories.
* Provide dedicated, namespaced discovery methods (e.g. `FindOldVersion()`) exclusively for upgrade suites to guarantee zero cross-suite interference.

### 13. Idempotent Baseline State Guard for Upgrade Test Suites
When testing application upgrades, updates, or service pack migrations:
* Never assume the test machine is already sitting on the baseline older version.
* Always guard the upgrade entry point with an automated baseline validator (`EnsureEz3DiOldVersionInstalledAction`).
* If the machine has the target/newer version installed, automatically invoke uninstallation teardown first, followed by deploying the older baseline package via the standard clean installation pipeline.
* If the machine is clean, deploy the older baseline package directly.
* If the machine is already on the baseline version, bypass setup with zero delay.
* This guarantees that upgrade test fixtures can run repeatedly, sequentially, or standalone on any system state with 100% self-healing reliability.

### 14. Taskbar-Less Window Anti-Hide Protocol
When automating windows or setup dialogs that do not possess a taskbar icon (e.g. `Quick Setting`, splash screens, utility popups):
* Never rely on taskbar button enumeration or basic desktop UIA searches alone, as they can fail or delay when a window is obscured.
* Use instant Win32 kernel lookups (`NativeMethods.FindWindow(null, exactTitle)`) and fallback to `EnumWindows` with window class and prefix filtering.
* Immediately upon detection, execute the 3-step anti-hide protocol:
  1. `InputDriver.PinWindowOnTop(hwnd)` (break foreground locks).
  2. `InputDriver.DemoteVsCodeZOrder(hwnd)` (demote IDE terminal below target).
  3. `InputDriver.ForceWindowForeground(hwnd)` (attach thread input & restore).
  4. `WindowHelper.CenterWindowOnScreen(hwnd)` (center on monitor for physical click accuracy).

### 15. Pixel-Accurate Coordinate Measurement for Custom Qt/MFC Controls
When interacting with dialogs or utilities where UI elements are custom-painted and FlaUI / UIA cannot resolve individual control bounding rectangles:
* **Never guess or approximate vertical/horizontal offsets** by eyeballing. In forms with adjacent input fields and action buttons, a 20–30px discrepancy will focus the wrong control (e.g. clicking into a text box instead of the button underneath).
* Extract exact pixel coordinates from actual unscaled PNG captures ($X_{min}, X_{max}, Y_{min}, Y_{max}$) to calculate the exact center point.
* Always couple geometric fallbacks with dedicated OpenCV visual templates (`ImageCompare.Find` / `ImageCompare.See`) cropped specifically from high-DPI production UI captures.
* Ensure target dialogs are brought to foreground and settled (`Thread.Sleep(300)`) prior to dispatching physical hardware mouse clicks.

### 16. System Common AppData (`C:\ProgramData`) Administrative Purge Protocol
When tests or pre-conditions require resetting application configuration stored in Windows Common AppData (`%ProgramData%` / `Environment.SpecialFolder.CommonApplicationData`):
* Never assume configuration files reside exclusively in the application installation directory (`Program Files`). Always check `%ProgramData%\<vendor>\...` for persistent application configuration.
* Removing files in `%ProgramData%` strictly requires elevated Windows Administrator permissions. Always wrap deletion operations in targeted `try / catch (UnauthorizedAccessException)` blocks that explicitly notify the developer/runner:
  `"ACCESS DENIED: Deleting configuration files from C:\ProgramData requires elevated Administrator privileges. Ensure VS Code or Terminal is launched as Administrator."`
* Always clear `FileAttributes.ReadOnly` before calling `File.Delete()` to ensure system/read-only flags do not abort the cleanup.
* Verify file deletion before continuing the test sequence to ensure the application starts with a pristine, unconfigured state.

### 17. Multi-Tier NUnit Category Taxonomy Protocol
To prevent CLI filter exclusions when running suite-level batch tests:
* Every test fixture must be decorated with its primary suite category (`[Category("Installation")]` or `[Category("Upgrade")]`) so that top-level suite runners (`dotnet test --filter "Category=Installation"`) always discover and execute the entire suite without silently omitting fixtures.
* Apply additional granular sub-categories as secondary attributes (`[Category("Uninstallation")]`, `[Category("Verification")]`, `[Category("Lifecycle")]`) to preserve isolated execution capability for targeted debug runs.
* Never use divergent or mutually exclusive category strings for tests that belong to the same logical suite.

### 18. Large Binary Management and Git Ignored Package Directory Protocol
* Never commit installer executable binaries (`*.exe`) larger than 100 MB to GitHub repositories.
* Enforce `Installation Package/**/*.exe` in `.gitignore` at all times.
* Retain folder structure across git clones using empty `.gitkeep` markers.
* Provide clear instructions in `GUIDE/README.md` and `DOCUMENT/` on how to download or place necessary setup binaries locally.

### 19. GitHub Repository Remote Validation & Provisioning Verification
* Before initiating a push to newly created GitHub remotes, verify credential manager mapping and validate remote readiness with `git ls-remote origin`.
* If GitHub returns transient 404 / "Repository not found" on a newly created private repository, wait for backend metadata synchronization to complete before attempting push.
* Ensure branch naming adheres to repository standards (`main` vs `master`) using `git branch -M main`.






