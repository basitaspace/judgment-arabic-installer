# Judgment Arabic Localization — Standalone Installer

A Windows utility for installing the Judgment Arabic patch. In the locally built standalone version, the installer application and patch payload are embedded inside **one EXE file**; end users do not need to select a separate ZIP or connect to the internet.

## Current patch status

- Installer version: 1.0.0.
- The local patch package was prepared on 14 November 2026.
- The local package includes `db.judge.en.par`, two Arabic font resources, and the attribution text.
- This does not mean every game resource is fully translated. The observed `ui.judge.en.par` file had not changed, and the available audit did not confirm that all other resources had been translated.

## Using the standalone build

1. Install a legitimate copy of Judgment.
2. Run `Judgment-Arabic-Installer.exe`.
3. Select the game folder, or its parent folder containing the Judgment folder.
4. Click **Install Arabic Translation**.
5. The installer reads the embedded patch files, verifies their checksums, and backs up existing files before replacing them.

**No separate ZIP file is required** when using the standalone build.

To revert, run the installer again, select the game folder, and click **Restore Latest Backup**.

## Building from source

Building requires Windows, the .NET Framework 4.x C# compiler (`csc.exe`), and the private patch package at:

    private-payload\Judgment-Arabic-Patch-2026-11-14.zip

Run:

    powershell -ExecutionPolicy Bypass -File .\build.ps1

Output:

    dist\Judgment-Arabic-Installer.exe

The build script embeds the ZIP as an assembly resource inside the EXE. The `private-payload` directory is intentionally excluded from Git, so the standalone build cannot be reproduced from the public repository alone without privately supplying the patch package.

## Privacy and safety

- The program does not connect to the internet or send game files.
- It verifies allowed file names and SHA-256 checksums before writing.
- It rejects disallowed paths inside the embedded archive.
- It backs up existing files before replacing them.
- The game must be closed during installation or restoration.

## Disclaimer and distribution

This is an unofficial community tool and is not affiliated with SEGA or Ryu Ga Gotoku Studio. Judgment, its trademarks, and original game files belong to their respective rights holders. This public repository contains installer source code and documentation only; it does not contain the patch package or game resources. A locally built EXE contains modified resources derived from game files; do not publicly distribute it unless you hold the necessary distribution rights or permission from the rights holder.
