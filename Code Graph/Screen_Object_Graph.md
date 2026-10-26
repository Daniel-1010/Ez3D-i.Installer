# Ez3D-i.Installer — Screen Object Graph

Detailed Page Object Models for InstallShield and Uninstaller wizard windows.

---

## 1. Class Hierarchy

```mermaid
classDiagram
    class InstallerWizardScreen {
        +AutomationBase Automation
        +Window CurrentWindow
        +WaitForWizardWindow(timeoutSeconds) Window
        +FindButton(buttonTexts) Button
        +FindButtonCoordinate(buttonTexts) Point
        +FindRadioButton(radioTexts) RadioButton
        +FindCheckBox(checkboxTexts) CheckBox
    }

    class UninstallerWizardScreen {
        +AutomationBase Automation
        +Window CurrentWindow
        +WaitForUninstallerWindow(timeoutSeconds) Window
        +FindButton(buttonTexts) Button
        +FindButtonCoordinate(buttonTexts) Point
    }

    class QuickSettingScreen {
        +AutomationBase Automation
        +Window QuickSettingWindow
        +Window SearchServerWindow
        +WaitForQuickSettingWindow(timeoutSeconds) Window
        +WaitForSearchServerWindow(timeoutSeconds) Window
        +FindSearchButtonCoordinate(win) Point
        +FindIpRowCoordinate(searchWin, targetIp) Point
        +FindOkButtonCoordinate(win, isSearchDialog) Point
        +GetServerIpAddressText(win) string
    }
```

---

## 2. Window Identification & Locators

### A. `InstallerWizardScreen`
* **Window Criteria:** Class `#32770`, Title containing `"Ez3D-i"`, `"InstallShield"`, or `"Setup"`. Minimum dimensions `200x150`.
* **Button Locators:**
  - `Next >`, `&Next >`, `Next`
  - `Install`, `&Install`
  - `Finish`, `&Finish`
  - `Cancel`
* **Radio Button Locators:**
  - `"accept the terms"`, `"I accept"`, `"accept"`, `"agree"`
  - `"restart my computer later"`
* **Checkbox Locators:**
  - `"Launch"`, `"Ez3D-i"`

### B. `UninstallerWizardScreen`
* **Window Criteria:** Class `#32770`, Title containing `"Ez3D-i"`, `"Uninstall"`, or `"Confirm"`.
* **Button Locators:**
  - `Yes`, `&Yes` (Removal confirmation)
  - `Next >`, `Remove`, `&Remove`
  - `Finish`, `&Finish`, `OK`
