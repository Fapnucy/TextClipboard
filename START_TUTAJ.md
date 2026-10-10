# Text Clipboard 1.1.0 — Retro Edition

Kompletny projekt dla **Visual Studio 2026**, Windows 11 i **.NET 10 WPF**.
Nie musisz ręcznie wklejać kodu ani dodawać plików do istniejącego projektu.

## Uruchomienie w Visual Studio

1. **Zamknij aktualnie działający Text Clipboard** przez ikonę przy zegarze (`Exit Application`). Nie uruchamiaj dwóch kopii jednocześnie — będą konkurować o skrót Ctrl + Alt + X.
2. **Zrób kopię dotychczasowego projektu i danych.** Plik z danymi jest zwykle w `%LOCALAPPDATA%\TextClipboard\data.json`. Nie usuwaj go.
3. Rozpakuj ZIP do nowego folderu, np. `C:\Projects\TextClipboard_Retro_VisualStudio_1.1`.
4. Otwórz **Visual Studio 2026 → Plik → Otwórz → Projekt lub rozwiązanie** i wskaż **`TextClipboard.sln`** z rozpakowanego folderu.
5. Poczekaj na wczytanie projektu. Uruchom **Ctrl + Shift + B** (kompilacja), następnie **F5**.

## Co zawiera wersja 1.1.0

- Główne okno inspirowane **Windows XP Olive Classic**, z klasycznym menu File / Edit / Theme / Help i zaokrąglonymi przyciskami.
- 5 motywów: **Olive Classic**, **Luna Blue**, **Silver Frost**, **Classic Gray**, **Night Mode**. Wybrany motyw jest zapisywany i przywracany.
- **Edit → Clear History** jako jedyna pozycja menu Edit, poza przyciskami na dole.
- **Help → About Text Clipboard**: wersja 1.1.0 (Retro Edition), utworzono 9 października 2026, autor Maciej Gosciniak.
- Historia **ostatnich 10 unikalnych tekstów**, zachowywana między uruchomieniami.
- Trwała biblioteka tekstów w folderach i podfolderach, tworzenie/edycja/usuwanie.
- Możliwość **odznaczenia** wybranego wpisu lub folderu przez ponowne kliknięcie albo kliknięcie pustego miejsca. Gdy nic nie jest zaznaczone, nowy folder powstaje na najwyższym poziomie.
- Szybkie menu przy kursorze (`Ctrl + lewy Alt + X`) z listą tekstów i rozwijanymi folderami. `Esc` zamyka menu. Menu jest lekko prześwitujące i ma zaokrąglone rogi.
- Wklejanie wybranego tekstu do poprzednio aktywnego programu (jeśli Windows pozwoli odzyskać jego fokus).
- Ikonka przy zegarze; zamknięcie przyciskiem X ukrywa główne okno, a `Exit Application` kończy proces.
- **Brak opcji Pause / Resume monitoring**. Kopiowany tekst jest przechwytywany przez cały czas działania programu.
- Interfejs aplikacji po **angielsku**.

## Dane i zgodność z wcześniejszą wersją

Program domyślnie czyta/zapisuje `%LOCALAPPDATA%\TextClipboard\data.json`.
Stare foldery i teksty pozostają w tym samym pliku; `ThemeName` zostanie automatycznie uzupełnione przy nowym zapisie.
**Nie usuwaj pliku `data.json`** i zachowaj jego kopię przed pierwszym uruchomieniem nowej wersji.

**Uwaga o prywatności:** dane w `data.json` nie są szyfrowane. Program może przechowywać także hasła, tokeny lub inne poufne teksty skopiowane z różnych aplikacji. Możesz wyczyścić historię, ale to nie czyści schowka systemowego Windows.

## Test po uruchomieniu

1. Skopiuj kilka zwykłych tekstów (`Ctrl + C`) i sprawdź historię.
2. Kliknij wpis i kliknij go ponownie — zaznaczenie powinno zniknąć.
3. Zaznacz folder, kliknij go ponownie, a następnie użyj **New Folder** — nowy folder powinien powstać obok dotychczasowego.
4. Wybierz każdy motyw z menu **Theme** i sprawdź jego zapamiętanie po ponownym uruchomieniu.
5. W Notatniku naciśnij `Ctrl + lewy Alt + X`: sprawdź wybór, podmenu, `Esc` i wklejanie.
6. Sprawdź menu **Edit** i **Help**, a następnie działanie ikonki przy zegarze.

**Uwaga o polskiej klawiaturze:** `Ctrl + Alt + X` może kolidować z `AltGr + X` (litera „ź”). Standardowa rejestracja globalnego skrótu nie gwarantuje rozróżnienia lewego i prawego Alt. Jeśli przeszkadza w pisaniu „ź”, zmienimy kombinację na inną.

## Publikacja EXE Portable

Zamknij aplikację i w PowerShell w folderze z rozwiązaniem uruchom:

```powershell
.\publish-portable.ps1
```

Skrypt tworzy samodzielny plik EXE w folderze `publish-portable` (wymaga Windows i .NET SDK 10). Żeby przechowywać **dane obok EXE**, utwórz pusty plik `portable.mode` w jego folderze. Wtedy historia i foldery będą zapisywane tam jako `data.json`, pod warunkiem że ten folder pozwala na zapis. Bez pliku `portable.mode` aplikacja nadal zapisuje w `%LOCALAPPDATA%`.

## Ograniczenia weryfikacji

Sprawdzono kompletność plików projektu, poprawność XML/XAML i istnienie metod obsługi zdarzeń. **Nie wykonano kompilacji Windows WPF** w środowisku przygotowania. Jeśli Visual Studio wyświetli błędy, prześlij screenshot Listy błędów, a przygotuję poprawiony pakiet.
