# Text Clipboard v1.3.0 — Retro Edition

Kompletny projekt WPF / .NET 10 dla Visual Studio 2026 (Windows 11).

## Nowości względem v1.2.0

- **Settings → Keyboard Shortcut...** — zmiana globalnego skrótu Quick Paste.
- Kliknij pole i naciśnij kombinację, np. `Ctrl + Shift + F8` albo `Win + Shift + X`.
- Po kliknięciu **Save** aplikacja sprawdza dostępność skrótu. Jeśli rejestracja się nie uda, **stary skrót nadal działa**.
- Po prawidłowej zmianie informacja o skrócie **od razu aktualizuje się w głównym oknie**, a także w About i podpowiedzi ikony w zasobniku.
- Skrót jest zapamiętywany w dotychczasowym pliku `data.json` i ładowany przy kolejnym uruchomieniu.
- **Reset to Default** ustawia `Win + Shift + X` (wymaga kliknięcia Save).
- Kombinacje Ctrl + Alt są blokowane, aby uniknąć problemów z `AltGr` na klawiaturach narodowych.
- **Dwuklik na zapisanym tekście w My Folders** otwiera istniejące okno Edit Text. Można zmienić nazwę i treść; Save zapisuje zmianę, Cancel pozostawia ją bez zmian. Dwuklik na folderze nie otwiera edytora.

## Funkcje zachowane z v1.2.0

- Historia 10 ostatnio skopiowanych tekstów, po jednym wierszu na wpis (pierwsza linia i wielokropek).
- Podgląd pełnego tekstu po dwukliku w Recent Copies albo naciśnięciu Preview; w podglądzie jest Copy Full Text.
- Maks. 1 000 000 znaków na wpis historii; Quick Paste wkleja cały tekst.
- Foldery i podfoldery, pięć motywów, Escape w Quick Paste, ikona przy zegarze, opcjonalny autostart.

## Dane i zgodność

Dane pozostają lokalnie w `%LOCALAPPDATA%\TextClipboard\data.json` (albo w katalogu EXE, jeśli istnieje `portable.mode`).
Wersja 1.3.0 dodaje do JSON tylko dwa pola: `HotkeyModifiers` i `HotkeyVirtualKey`. Wczytuje dane z wcześniejszych wydań bez migracji. **Zrób kopię data.json przed aktualizacją.**

Skopiowany tekst może zawierać poufne dane. Ta aplikacja nie jest menedżerem haseł.

## Budowanie

1. Rozpakuj ZIP do nowego katalogu.
2. Zakończ poprzednią wersję Text Clipboard (ikona w zasobniku → Exit Application).
3. Otwórz `TextClipboard.sln` w Visual Studio 2026 z obsługą .NET Desktop Development.
4. Build Solution (`Ctrl + Shift + B`), następnie `F5`.
5. Po testach na Windows uruchom `build-installer.ps1` (potrzebny Inno Setup 6/7):

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\build-installer.ps1
```

Plik wynikowy: `Installer\Output\TextClipboard_Setup_1.3.0.exe`.

## Testy do wykonania na Windows

1. Ustaw `Ctrl + Shift + F8`, zapisz; potwierdź nowy napis w głównym oknie, działanie skrótu i zachowanie po restarcie.
2. Spróbuj zajętego skrótu (np. używanego przez inną aplikację): stary nadal ma działać.
3. Spróbuj `Ctrl + Alt + X`: program ma odrzucić tę kombinację (AltGr / ź).
4. Reset to Default + Save powinien ponownie ustawić `Win + Shift + X`.
5. Dwuklik zapisanego tekstu w My Folders → edytuj nazwę/treść → Save → sprawdź Quick Paste i ponowne otwarcie.
6. Dwuklik folderu powinien służyć do rozwijania/zwijania; anulowanie edycji nie może zmieniać tekstu.
7. Sprawdź historię, Preview i kopiowanie dłuższych tekstów.

Kontrola w środowisku przygotowawczym obejmuje strukturę projektu, parsowanie XAML i powiązania eventów. **Nie wykonano kompilacji WPF ani instalatora na Windows.**
