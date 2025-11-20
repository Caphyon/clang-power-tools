
# Copilot recipe – Update clang-format & LLVM metadata

When the user says something like:

> Update clang-format and LLVM version to `<NEW_LLVM_VERSION>` with CPT previous release tag `<CPT_PREVIOUS_RELEASE_TAG>`

you must do **all** of the following steps from the **repo root** (`clang-power-tools`).

> ⚠️ **Important:**  
> The value of `kCptGithubLlvm` is **the Clang Power Tools GitHub release that contains the previous assets**  
> (`#Url to assets (clang++ and clang-tidy) from previous release made by Clang Power Tools on github`).  
> If the user did **not** provide `<CPT_PREVIOUS_RELEASE_TAG>`, you **must ask the user** for it before editing
> `get-llvm-helper.ps1`. Do **not** guess it.

---

## 1. Run the PowerShell updater

From the **repo root**, run:

```powershell
& ".\ClangPowerTools\Tidy_Checks_Automation\update-clang-format.ps1" -LlvmVersion <NEW_LLVM_VERSION>
```

Example for LLVM 21.1.6:

```powershell
& ".\ClangPowerTools\Tidy_Checks_Automation\update-clang-format.ps1" -LlvmVersion 21.1.6
```

This downloads:

```
https://github.com/llvm/llvm-project/releases/download/llvmorg-<NEW_LLVM_VERSION>/LLVM-<NEW_LLVM_VERSION>-win64.exe
```

runs the installer into a temporary folder, then copies the new `clang-format.exe` to:

```
ClangPowerTools/ClangPowerToolsShared/Executables/clang-format.exe
```

You do **not** need to manually edit the EXE; just ensure this path is updated.

---

## 2. Update C# version lists

### 2.1 File: `ClangPowerTools/ClangPowerToolsShared/MVVM/Constants/LLVMVersionsAlternate.cs`

In the `VersionAlternateUri` list, insert the new version **at the top** (as the first element), keeping the rest unchanged.

Pattern:

```diff
public static List<string> VersionAlternateUri { get; } = new List<string>
{
+  "<NEW_LLVM_VERSION>",
   "18.1.2",
   "17.0.1",
   "16.0.6",
   // ...
};
```

---

### 2.2 File: `ClangPowerTools/ClangPowerToolsShared/MVVM/Constants/LlvmVersions.cs`

In the `Versions` list, insert the new version **at the top** (first element), keeping the rest unchanged.

Pattern:

```diff
public static List<string> Versions { get; } = new List<string>
{
+  "<NEW_LLVM_VERSION>",
   "18.1.2",
   "17.0.1",
   "16.0.6",
   // ...
};
```

---

## 3. Update the PowerShell helper script

### File: `ClangPowerTools/ClangPowerToolsShared/Tooling/v1/psClang/get-llvm-helper.ps1`

At the top of this file there is a comment:

```powershell
#Url to assets (clang++ and clang-tidy) from previous release made by Clang Power Tools on github
```

This means:

- `kCptGithubLlvm` must point to the **previous Clang Power Tools GitHub release** that contains the clang++/clang-tidy assets.
- `kCptGithubLlvmVersion` is a human-readable description for the LLVM version.

> 🔴 **Rule for Copilot:**  
> If the user did **not** provide `<CPT_PREVIOUS_RELEASE_TAG>`, you must ask:  
> *“What is the Clang Power Tools GitHub release tag that contains the previous clang++/clang-tidy assets (for kCptGithubLlvm)?”*  
> Only after the user answers, update `kCptGithubLlvm`.

Once the user has provided `<CPT_PREVIOUS_RELEASE_TAG>`, update both variables like this:

```diff
-Set-Variable -name kCptGithubLlvm -value "https://github.com/Caphyon/clang-power-tools/releases/download/v2023.9.0" `
+Set-Variable -name kCptGithubLlvm -value "https://github.com/Caphyon/clang-power-tools/releases/download/<CPT_PREVIOUS_RELEASE_TAG>" `
                                   -option Constant
-Set-Variable -name kCptGithubLlvmVersion -value "18.1.2 (LLVM 18.1.2)" -Option Constant
+Set-Variable -name kCptGithubLlvmVersion -value "<NEW_LLVM_VERSION> (LLVM <NEW_LLVM_VERSION>)" -Option Constant
```

---

## 4. Summary for Copilot

When asked to update LLVM to `<NEW_LLVM_VERSION>`:

1. **Run the update script** from the repo root:

```powershell
& ".\ClangPowerTools\Tidy_Checks_Automation\update-clang-format.ps1" -LlvmVersion <NEW_LLVM_VERSION>
```

2. **Edit** `ClangPowerTools/ClangPowerToolsShared/MVVM/Constants/LLVMVersionsAlternate.cs`:
   - Insert `<NEW_LLVM_VERSION>` as the first element in `VersionAlternateUri`.

3. **Edit** `ClangPowerTools/ClangPowerToolsShared/MVVM/Constants/LlvmVersions.cs`:
   - Insert `<NEW_LLVM_VERSION>` as the first element in `Versions`.

4. **Edit** `get-llvm-helper.ps1`:
   - If `<CPT_PREVIOUS_RELEASE_TAG>` is **not provided**, ask the user for it.
   - After receiving it, update:
     - `kCptGithubLlvm` → `"https://github.com/Caphyon/clang-power-tools/releases/download/<CPT_PREVIOUS_RELEASE_TAG>"`
     - `kCptGithubLlvmVersion` → `"<NEW_LLVM_VERSION> (LLVM <NEW_LLVM_VERSION>)"`

5. After all changes, show diffs for:
   - `LLVMVersionsAlternate.cs`
   - `LlvmVersions.cs`
   - `get-llvm-helper.ps1`
   - Confirm the script replaced `clang-format.exe`.
