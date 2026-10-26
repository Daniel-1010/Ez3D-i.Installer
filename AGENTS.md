# 🚨 ANTIGRAVITY TOP RULES (PINNED)

> [!CAUTION]
> **TOP MANDATORY RULES FOR ANTIGRAVITY WORKING ON THIS PROJECT (ENFORCED ON EVERY TURN):**

---

## 📌 THE 12 TOP PINNED RULES FOR EZ3D-I INSTALLER

### 1. 📋 DO NOT EDIT CODE — ALWAYS GIVE THE PLAN FIRST
* **Never modify, edit, or create code without presenting a step-by-step implementation plan to the user first.**
* The turn **must end** after presenting the plan to await user review and explicit approval ("Do it", "Proceed").
* Never combine plan drafting and code edits in the same turn.

### 2. 📖 ALWAYS READ THE RULE FILE IN THE `.agents/` FOLDER
* Read all rule files in `d:\Ez3D-i.Installer\.agents\` and specification documents in `d:\Ez3D-i.Installer\DOCUMENT\` before taking action.
* Keep [`RULE.md`](file:///d:/Ez3D-i.Installer/.agents/RULE.md) synchronized with any new tests or actions.

### 3. 🔍 CODE GRAPH FIRST — ZERO MANUAL DISCOVERY FOR CROSS-FILE EDITS
* **NEVER** perform manual directory crawling or blind project-wide searches to determine blast radius.
* **Mandatory 2-Step Blast Radius Protocol**:
  1. Open [`Code Graph/Impact_Analysis_Matrix.md`](file:///d:/Ez3D-i.Installer/Code%20Graph/Impact_Analysis_Matrix.md) FIRST to get the risk tier and all dependent suites.
  2. Open [`Code Graph/Test_To_Action_Map.md`](file:///d:/Ez3D-i.Installer/Code%20Graph/Test_To_Action_Map.md) or [`Action_Call_Hierarchy.md`](file:///d:/Ez3D-i.Installer/Code%20Graph/Action_Call_Hierarchy.md) to get the exact list of dependent files in seconds.
* Only open the specific target files identified by the Code Graph.

### 4. 🔒 DO NOT PUT CODE TO GITHUB WITHOUT PERMISSION
* Never execute `git push` or publish code to GitHub/remote repositories without explicit permission from the user.

### 5. 📝 ALWAYS GIVE THE DETAILS ANSWER
* Always provide comprehensive, line-by-line, and thorough technical details, code references, and complete explanations in all answers and test status updates.

### 6. 📝 AUTOMATIC REVIEW & LESSON SYNCHRONIZATION AFTER EVERY CHANGE
* After diagnosing any test failure, making any bug fix, calibration, or code modification, the agent **MUST ALWAYS AUTOMATICALLY UPDATE**:
  1. [`GUIDE/REVIEW.md`](file:///d:/Ez3D-i.Installer/GUIDE/REVIEW.md) (with a new chronological numbered section log).
  2. [`.agents/Lessons/01_Failures_And_What_Went_Wrong.md`](file:///d:/Ez3D-i.Installer/.agents/Lessons/01_Failures_And_What_Went_Wrong.md) (with failure symptoms, root-cause mechanisms, and technical resolutions).
  3. [`.agents/Lessons/02_Lessons_Learned_And_AI_Guidelines.md`](file:///d:/Ez3D-i.Installer/.agents/Lessons/02_Lessons_Learned_And_AI_Guidelines.md) (with permanent preventative protocols and coding rules).
* **This must happen automatically on EVERY task/turn without requiring the user to prompt or remind the agent.**

### 7. 🖥️ ALWAYS RUN `DOTNET TEST` FROM VSCODE TERMINAL — DO NOT RUN FROM AI IDE
* UI automation tests must **ALWAYS** be executed by the user directly in their elevated Administrator VSCode Terminal / Windows Terminal.
* **DO NOT** execute `dotnet test` from background subagent tasks in the AI IDE because background sub-processes trigger Windows UIPI UAC `Win32Exception: Access is denied` errors on elevated setup processes.

### 8. ⚡ OPTIMIZE RULE — ALWAYS REUSE EXISTING CODE ACROSS THE ENTIRE PROJECT
* **BEFORE creating any new function, class, or file**, always scan the **entire project** (`Actions/`, `Core/`, `Screens/`, `Tests/`) for existing equivalents.
* **DO NOT CREATE A NEW FUNCTION / CLASS / FILE IF AN EQUIVALENT ALREADY EXISTS ANYWHERE IN THE PROJECT.**
* If an existing function can be reused with minor parameter changes, **extend or call it** — do not duplicate it.

### 9. 🗺️ ALWAYS CONSULT AND UPDATE THE CODE GRAPH (`d:\Ez3D-i.Installer\Code Graph\`)
* **PRE-CHANGE CHECK**: Before modifying any shared Action (`Actions/`), Screen (`Screens/`), or Core driver (`Core/`), **ALWAYS** check [`Code Graph/Impact_Analysis_Matrix.md`](file:///d:/Ez3D-i.Installer/Code%20Graph/Impact_Analysis_Matrix.md).
* **POST-CHANGE SYNCHRONIZATION**:
  - New / Modified Test $\rightarrow$ Update [`Code Graph/Test_To_Action_Map.md`](file:///d:/Ez3D-i.Installer/Code%20Graph/Test_To_Action_Map.md)
  - New / Modified Action $\rightarrow$ Update [`Code Graph/Action_Call_Hierarchy.md`](file:///d:/Ez3D-i.Installer/Code%20Graph/Action_Call_Hierarchy.md)
  - New / Modified Screen $\rightarrow$ Update [`Code Graph/Screen_Object_Graph.md`](file:///d:/Ez3D-i.Installer/Code%20Graph/Screen_Object_Graph.md)
  - New Reference Template (`.png`) $\rightarrow$ Register in [`Code Graph/Core_And_Infrastructure_Graph.md`](file:///d:/Ez3D-i.Installer/Code%20Graph/Core_And_Infrastructure_Graph.md)

### 10. 🖱️ HARDWARE INPUT DWELL-TIME & PHYSICAL COORDINATE CLICKS
* For InstallShield / Win32 wizard radio buttons and buttons, always use physical coordinate mouse clicks via [`InputDriver.ClickAtPhysicalCoordinate`](file:///d:/Ez3D-i.Installer/Core/InputDriver.cs) to ensure Win32 `WM_COMMAND` / `BN_CLICKED` messages are generated (UIA pattern alone does not always notify InstallScript).

### 11. 🛡️ ELEVATION & PROCESS ISOLATION
* InstallShield setup launchers (`setup.exe`, `_is`, `msiexec`) run with elevated Administrator privileges. Ensure tests terminate lingering instances cleanly via `ProcessHelper.KillProcessesByName` before launch.

### 12. ♻️ STRICT RESOURCE DISPOSAL & ZERO COMPILER WARNINGS
* Wrap every `Bitmap`, `Graphics`, `RegistryKey`, and `Process` in a `using` block, and ensure `dotnet build` passes with `0 Warnings, 0 Errors` (enforced via `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`).

---

*This file (`AGENTS.md`) is located in the repository root and is automatically injected into the Antigravity prompt on every turn.*
