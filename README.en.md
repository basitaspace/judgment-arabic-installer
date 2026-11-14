# Judgment Arabic Localization Installer

A Windows utility that installs a user-supplied Judgment Arabic patch ZIP, verifies its SHA-256 checksums, and backs up existing files before replacement.

## Current patch status

The local package prepared alongside this source uses the currently installed files observed on 14 November 2026. The main database file was last modified locally at 13:49:54. The package includes that database, two Arabic font resources, and the attribution text.

This does not claim that every game resource is fully translated. The observed ui.judge.en.par file had not changed, and the available audit did not confirm that all remaining resources had been translated and applied.

## Usage

1. Install a legitimate copy of Judgment.
2. Run Judgment-Arabic-Installer.exe.
3. Select the game folder, or its parent folder containing Judgment.
4. Select the Arabic patch ZIP.
5. Click Install.
6. Original files are backed up under _Arabic_Backups in the game folder.

Use Restore latest backup to revert the most recent installation.

## Patch ZIP format

The ZIP root must contain:
- db.judge.en.par
- font.judge/en/meta_ot_cond_book.dds
- font.judge/en/meta_ot_cond_book_italic.dds
- SHA256SUMS.txt

Optional: tarib_judgment.txt and PATCH_INFO.txt.

Do not commit or publicly distribute game archives, extracted proprietary assets, or modified game resource files unless you have the necessary rights or permission. The local private-payload folder is intentionally ignored by Git.

## Build

On Windows with the .NET Framework C# compiler installed:

    powershell -ExecutionPolicy Bypass -File .\build.ps1

Output: dist\Judgment-Arabic-Installer.exe.

## Disclaimer

Unofficial community tool; not affiliated with SEGA or Ryu Ga Gotoku Studio. Judgment and its original game assets belong to their respective rights holders. This repository contains installer source code only, not game files or the translation payload.
