# START — Text Clipboard v1.3.0

Masz teraz dwa katalogi: stary projekt GitHub (v1.1.1) oraz nowy `TextClipboard_1.3.0_Settings`.

## Najpierw przetestuj v1.3.0 osobno

1. Zamknij **stary Text Clipboard** całkowicie (trójkąt przy zegarze → prawy przycisk ikony → Exit Application).
2. Zachowaj kopię `%LOCALAPPDATA%\TextClipboard\data.json`.
3. W tym katalogu otwórz `TextClipboard.sln` w Visual Studio 2026.
4. Użyj `Ctrl + Shift + B` (Build), a potem `F5` (Run).
5. Sprawdź **Settings → Keyboard Shortcut...**. Zmień skrót na `Ctrl + Shift + F8`, kliknij Save. Napis w górnej części okna powinien zmienić się automatycznie.
6. Sprawdź dwuklik zapisanych tekstów w **My Folders**, edytuj i zapisz.
7. Zamknij aplikację przez Exit i uruchom ponownie — ustawienia skrótu i zmieniony tekst powinny zostać zachowane.

**Dopiero gdy testy przejdą**, przenieś kod do istniejącego repozytorium. Nie przenoś `.git` ani danych `data.json` z dysku do GitHub.

## Po testach: aktualizacja starego repozytorium

Z nowego folderu przenieś do starego repozytorium GitHub:
- cały folder `TextClipboard` (scalając zawartość i zastępując starsze pliki),
- `Installer\TextClipboard.iss`, `build-installer.ps1`, `README_PL.md`, `START_TUTAJ.md`.

Zachowaj `.git` i `.gitignore` ze starego repozytorium. Skompiluj ponownie. Następnie Git Changes → Commit, Push. Kompilację instalatora i GitHub Release `v1.3.0` zrobimy po lokalnym teście.

Dokładniejsza instrukcja i lista testów: `README_PL.md`.
