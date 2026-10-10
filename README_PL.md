# Text Clipboard 1.1.1 — instalator Windows 11

## Co zawiera
- kompletny projekt Visual Studio 2026 / .NET 10 WPF (`TextClipboard.sln`)
- ikonę retro `TextClipboard/Assets/TextClipboard.ico` (również pasek tytułu i zasobnik przy zegarze)
- instalator Inno Setup (`Installer/TextClipboard.iss`) i automatyczny skrypt budowania (`build-installer.ps1`)
- funkcje historyczne v1.1: 10 recent texts, foldery, Quick Paste Ctrl+Alt+X, motywy, JSON w LocalAppData
- poprawki: `LinearGradientBrush`, kontrast rozwijanego menu w Night Mode, klawisz Escape w Quick Paste

## Budowanie (na Windows 11)
1. Rozpakuj ZIP do osobnego folderu.
2. Otwórz `TextClipboard.sln` w Visual Studio 2026 i skompiluj (Ctrl+Shift+B).
3. Zainstaluj Inno Setup 7 lub 6 z https://jrsoftware.org/isdl.php.
4. Otwórz **PowerShell** w folderze rozpakowanego ZIP (tam, gdzie jest `build-installer.ps1`).
5. Uruchom polecenie:
   `powershell -NoProfile -ExecutionPolicy Bypass -File .\build-installer.ps1`
6. Odbierz instalator `Installer\Output\TextClipboard_Setup_1.1.1.exe`.

## Zachowanie instalatora
- Instaluje dla obecnego użytkownika: `%LOCALAPPDATA%\Programs\TextClipboard` — bez uprawnień administratora.
- Skrót w menu Start, opcjonalny skrót na pulpicie.
- **Opcjonalny autostart**, domyślnie **wyłączony**. Po wybraniu: rejestr HKCU Run z `--startup`.
- `--startup` uruchamia aplikację w tle przy zegarze bez wyświetlania głównego okna.
- Usunięcie aplikacji nie usuwa danych z `%LOCALAPPDATA%\TextClipboard\data.json`.
- Aktualizacja nie powinna zmieniać plików danych; zawsze warto wykonać ich kopię przed instalacją.

## Ważne
Pliki wynikowe EXE i setup **wymagają skompilowania na Windows** z .NET 10 SDK i Inno Setup.
Projekt jest przygotowany do kompilacji, lecz nie był kompilowany na Windows w tym środowisku.
**Jeżeli masz nowsze ręczne zmiany, nie nadpisuj ich bez kopii:** paczka bazuje na poprzednio utworzonym projekcie i znanych poprawkach z rozmowy.
Windows Smart App Control może blokować niepodpisane pliki. Instalator nie wyłącza żadnych zabezpieczeń systemowych.
