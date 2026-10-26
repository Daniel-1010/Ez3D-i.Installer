# 🚨 ANTIGRAVITY TOP RULES FOR EZ3D-I INSTALLER

> [!CAUTION]
> **TOP MANDATORY RULES FOR ANTIGRAVITY WORKING ON EZ3D-I INSTALLER:**

---

## 📌 THE 12 TOP PINNED RULES

1. **📋 DO NOT EDIT CODE — ALWAYS GIVE THE PLAN FIRST**
   - Never modify, edit, or create code without presenting a step-by-step implementation plan to the user first.

2. **📖 ALWAYS READ THE RULE FILE IN THE `.agents/` FOLDER**
   - Read all rule files in `d:\Ez3D-i.Installer\.agents\` and specification documents in `d:\Ez3D-i.Installer\DOCUMENT\` before taking action.
   - Keep this file synchronized when adding new tests, actions, or assets.

3. **🔍 CODE GRAPH FIRST — ZERO MANUAL DISCOVERY FOR CROSS-FILE EDITS**
   - **NEVER** perform manual directory crawling or blind project-wide searches to determine blast radius.
   - **Mandatory 2-Step Blast Radius Protocol**:
     1. Open [`Code Graph/Impact_Analysis_Matrix.md`](file:///d:/Ez3D-i.Installer/Code%20Graph/Impact_Analysis_Matrix.md) FIRST to get the risk tier and all dependent suites.
     2. Open [`Code Graph/Test_To_Action_Map.md`](file:///d:/Ez3D-i.Installer/Code%20Graph/Test_To_Action_Map.md) or [`Action_Call_Hierarchy.md`](file:///d:/Ez3D-i.Installer/Code%20Graph/Action_Call_Hierarchy.md) to get the exact list of dependent files in seconds.

4. **🔒 DO NOT PUT CODE TO GITHUB WITHOUT PERMISSION**
   - Never execute `git push` or publish code to GitHub/remote repositories without explicit permission from the user.

5. **📝 ALWAYS GIVE THE DETAILS ANSWER**
   - Always provide comprehensive, line-by-line, and thorough technical details, code references, and complete explanations in all answers and test status updates.

6. **📝 AUTOMATIC REVIEW & LESSON SYNCHRONIZATION AFTER EVERY CHANGE**
   - After diagnosing any test failure, making any bug fix, calibration, or code modification, the agent **MUST ALWAYS AUTOMATICALLY UPDATE**:
     1. [`GUIDE/REVIEW.md`](file:///d:/Ez3D-i.Installer/GUIDE/REVIEW.md) (with a new chronological numbered section log).
     2. [`.agents/Lessons/01_Failures_And_What_Went_Wrong.md`](file:///d:/Ez3D-i.Installer/.agents/Lessons/01_Failures_And_What_Went_Wrong.md) (with failure symptoms, root-cause mechanisms, and technical resolutions).
     3. [`.agents/Lessons/02_Lessons_Learned_And_AI_Guidelines.md`](file:///d:/Ez3D-i.Installer/.agents/Lessons/02_Lessons_Learned_And_AI_Guidelines.md) (with permanent preventative protocols and coding rules).

7. **🖥️ ALWAYS RUN `DOTNET TEST` FROM VSCODE TERMINAL — DO NOT RUN FROM AI IDE**
   - UI automation tests must **ALWAYS** be executed by the user directly in their elevated VSCode Terminal / Windows Terminal.
   - Background subagent execution causes Windows UIPI Access Denied errors.

8. **⚡ OPTIMIZE RULE — ALWAYS REUSE EXISTING CODE ACROSS THE ENTIRE PROJECT**
   - Before creating any new function, class, or file, scan `Actions/`, `Core/`, `Screens/`, `Tests/` for existing equivalents.
   - Do not duplicate existing logic.

9. **🗺️ ALWAYS CONSULT AND UPDATE THE CODE GRAPH (`d:\Ez3D-i.Installer\Code Graph\`)**
   - Pre-change: Check [`Impact_Analysis_Matrix.md`](file:///d:/Ez3D-i.Installer/Code%20Graph/Impact_Analysis_Matrix.md).
   - Post-change: Synchronize `Test_To_Action_Map.md`, `Action_Call_Hierarchy.md`, `Screen_Object_Graph.md`, and `Core_And_Infrastructure_Graph.md`.

10. **🔄 DETERMINISTIC FILE SYNCHRONIZATION MATRIX**

    | If You Modify / Add... | You MUST Immediately Update: |
    | :--- | :--- |
    | **Action / Screen / Driver** | 1. [`Code Graph/Action_Call_Hierarchy.md`](file:///d:/Ez3D-i.Installer/Code%20Graph/Action_Call_Hierarchy.md)<br>2. [`Code Graph/Test_To_Action_Map.md`](file:///d:/Ez3D-i.Installer/Code%20Graph/Test_To_Action_Map.md)<br>3. Affected `DOCUMENT/INSTALL_TEST_SPEC.md`<br>4. [`GUIDE/REVIEW.md`](file:///d:/Ez3D-i.Installer/GUIDE/REVIEW.md) |
    | **Test Fixture (.cs)** | 1. [`Code Graph/Test_To_Action_Map.md`](file:///d:/Ez3D-i.Installer/Code%20Graph/Test_To_Action_Map.md)<br>2. [`Code Graph/Impact_Analysis_Matrix.md`](file:///d:/Ez3D-i.Installer/Code%20Graph/Impact_Analysis_Matrix.md)<br>3. [`GUIDE/REVIEW.md`](file:///d:/Ez3D-i.Installer/GUIDE/REVIEW.md) |
    | **Reference Template (.png)** | 1. [`Code Graph/Core_And_Infrastructure_Graph.md`](file:///d:/Ez3D-i.Installer/Code%20Graph/Core_And_Infrastructure_Graph.md)<br>2. `Assets/Templates/` |

11. **🖱️ HARDWARE INPUT DWELL-TIME & PHYSICAL COORDINATE CLICKS**
    - Always pass valid `HWND` to [`InputDriver.ClickAtPhysicalCoordinate`](file:///d:/Ez3D-i.Installer/Core/InputDriver.cs) with adequate sleep (100–300ms) to ensure Win32 events trigger InstallShield logic.

12. **♻️ STRICT RESOURCE DISPOSAL & ZERO COMPILER WARNINGS**
    - Wrap all Win32 / UIA / Image resources in `using` blocks. Build must succeed with 0 warnings and 0 errors.

---

## 📂 Project Specification Index
* [INSTALL_TEST_SPEC.md](file:///d:/Ez3D-i.Installer/DOCUMENT/INSTALL_TEST_SPEC.md) — Master installation & uninstallation testing specification index.
* [TC01_Clean_Installation.md](file:///d:/Ez3D-i.Installer/DOCUMENT/TC01_Clean_Installation.md) — TC01 clean installation specification.
* [TC02_Post_Install_Verify.md](file:///d:/Ez3D-i.Installer/DOCUMENT/TC02_Post_Install_Verify.md) — TC02 post-installation state verification.
* [TC03A_Uninstall_Control_Panel.md](file:///d:/Ez3D-i.Installer/DOCUMENT/TC03A_Uninstall_Control_Panel.md) — TC03A Windows Control Panel uninstallation specification.
* [TC03B_Uninstall_Rerun_Package.md](file:///d:/Ez3D-i.Installer/DOCUMENT/TC03B_Uninstall_Rerun_Package.md) — TC03B Rerun package maintenance mode specification.
* [TC04_Full_Lifecycle.md](file:///d:/Ez3D-i.Installer/DOCUMENT/TC04_Full_Lifecycle.md) — TC04 end-to-end deployment lifecycle specification.
