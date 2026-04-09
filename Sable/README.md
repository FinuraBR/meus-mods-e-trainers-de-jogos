# Sable Cheat Engine Trainer

**[Versão em Português](./LEIAME.md)**

## Overview

This is a reverse engineering project focused on developing a trainer for the game **Sable** using **Cheat Engine**. The tool allows real-time manipulation of game memory values, providing the player with various conveniences and debugging options without needing to modify the original game files. The trainer was designed based on game version **4.3**.

## Compatibility

*   **Game Version:** Sable v4.3
*   **Platform:** PC
*   **Required Tool:** [Cheat Engine](https://cheatengine.org/) (latest version recommended)

## Included Features

The trainer offers a set of cheats and debugging parameters accessible directly through the Cheat Engine interface. Check the boxes to activate cheats or adjust values to modify game parameters.

### Main Cheats

*   **Inf Stamina:** Removes stamina consumption, allowing unlimited sprinting, climbing, and other actions.
*   **Inf Purchase ("Inf Money"):** Allows purchases even without sufficient money.

### Debug Options & Game Parameters

These controls expose internal game variables, allowing direct modifications for debugging purposes or to customize the gameplay experience:

*   **Money:** Directly adjusts the player's money amount.
*   **Custom Map Marker Qty:** Controls the quantity of custom markers on the map.
*   **Fishing Minigame Fish Position:** Displays and potentially allows manipulation of the fish's position during the fishing minigame.
*   **Sprint Stamina Rate:** Adjusts the rate of stamina consumption when sprinting.
*   **Fishing Minigame Finished:** Indicates the completion state of the fishing minigame (0: In Progress/Not Fishing, 1: Finished).
*   **Fishing Minigame State:** Describes the current situation of the fish in the fishing minigame (0: Off Target, 1: On Target).
*   **Fishing State:** Details the different phases of the fishing process (e.g., Aiming, Charging, Casting, Victory, Failure).
*   **Bait Cast Strength:** Controls the force with which the bait is cast.
*   **Fishing Minigame Progress:** Speeds up or freezes progress in the fishing minigame.
*   **FPS Counter:** Toggles an internal game FPS counter (0: Off, 1: On).
*   **Fmod Global Parameter Debug:** Toggles debugging of global parameters for the FMOD audio system (0: Off, 1: On).
*   **Stamina Recharge Rate:** Modifies the speed at which the player's stamina recharges.
*   **Climbing Stamina:** Adjusts the amount of stamina consumed during climbing.
*   **Air Move Force:** Controls the force applied to the character's aerial movement.

## Project Files

*   `Sable.ct`: The main Cheat Engine table file, containing all Auto Assembler scripts and memory pointers.
*   `README.md`: This usage guide and project documentation.

## How to Use

To use the trainer, follow the steps below:

1.  **Prerequisite:** Ensure you have [Cheat Engine](https://cheatengine.org/) installed on your computer.
2.  Download the `Sable.ct` file directly from the [repository's Releases tab](https://github.com/FinuraBR/meus-mods-e-trainers-de-jogos/releases?q=Sable).
3.  Start the game **Sable**.
4.  With the game running, double-click the `Sable.ct` file you downloaded. This will open the table in Cheat Engine.
5.  In Cheat Engine, click the **computer icon with a magnifying glass** (located in the upper left corner and blinking).
6.  In the process list, locate and select the game executable **Sable.exe**.
7.  When Cheat Engine asks if you want to keep the current address/code list, click **Yes**.
8.  At the bottom of the program, you will see the "Cheats" and "Debug Options" categories. Click the checkboxes (`[ ]`) next to the cheats you wish to activate or manually change the values for the desired parameters.

## Technical Details

This trainer is built upon advanced reverse engineering techniques applied to games using the Unity engine with an IL2CPP backend.

### Reverse Engineering Methodology

1.  **IL2CPP Analysis:** Sable is a Unity game compiled with IL2CPP, meaning the original C# code is transpiled to C++ and then compiled into native code. This requires a native binary reverse engineering approach (x64 assembly).
2.  **AOB (Array of Bytes) Scanning:** The primary technique for locating points of interest in the game's code is scanning for byte patterns (`aobscanmodule`). This allows identifying specific assembly instruction sequences (`E8 E3 97 E5 FF`, `0F 9E C0 48 83 C4 28 C3 E8 87`, etc.) within the `GameAssembly.dll` module, which contains the game's logic.

### Hooking and Code Injection Techniques

Modifications are implemented through **Auto Assembler** scripts in Cheat Engine, utilizing the following techniques:

1.  **Trampoline Injection:** Small code blocks (`jmp newmem_...`) are injected at addresses found via AOB. These blocks redirect execution to a dynamically allocated memory region (`alloc(newmem, ...)`) where the modified code resides. After the custom code executes, execution is returned to the game (`jmp return_...`).
2.  **Instruction Modification:**
    *   **NOP (No Operation):** For cheats like "Inf Stamina," instructions that decrement stamina are replaced with `nop`s, effectively disabling the consumption logic.
    *   **XOR r8d, r8d:** In some cases, an instruction like `xor r8d,r8d` is used to zero out a register that would be used in a stamina calculation, ensuring the value remains maximum.
    *   **Stack Pointer Adjustment (`add rsp,28`):** In "Inf Purchase," `rsp` manipulation is used to skip a portion of the function that would likely handle money deduction or a balance check.
3.  **Global Memory Allocation (`globalalloc`):** For "Debug Options," addresses of crucial game variables (such as `dinheiro`, `posicao_peixe_minigame_pesca`, `sprintstaminarate`, etc.) are identified at runtime. The trainer injects code to capture the address of these variables and store it in global symbols (`mov [symbol], rax`), allowing Cheat Engine to display and make them editable in the table.

### Specific Implementation Examples

*   **Inf Stamina:** Identifies the point where stamina is subtracted or checked in `SableCharacterController.set_CurrentClimbDistance` and `SableCharacterController.set_CurrentStamina` (reference based on XML `CodeEntry`s), and injects `NOP`s to nullify the decrement operation.
*   **Inf Purchase:** Locates the money deduction routine and the `setle al` instruction (set if less or equal), which is typically part of a balance validation. By modifying this section, the purchase cost is effectively ignored.
*   **Exposing Debug Parameters:** For variables like `Fishing Minigame Fish Position` or `Sprint Stamina Rate`, the trainer intercepts instructions that read or write to these variables (e.g., `movss xmm1,[rcx+0000021C]`) and, before the original operation, saves the variable's address into an allocated global memory region. This allows the user to directly view and modify the value.
