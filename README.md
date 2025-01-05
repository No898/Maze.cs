# Úkol - Projekt Trpaslík (Maze)

## Zadání

Vytvořte konzolovou aplikaci, která:

1. **Bludiště**
   - Načte bludiště definované v souboru `maze.dat` (viz příloha).
   - Bludiště se načte do paměti a vypíše na obrazovku (rozměr konzole se nastaví dle velikosti bludiště).
   - Značení:
     - `S` = Start
     - `F` = Cíl
     - `#` = Zeď

2. **Trpaslík**
   - Vytvořte objekt trpaslíka, který:
     - Se pohybuje bludištěm (pouze nahoru/dolu, vlevo/vpravo, nikoliv diagonálně).
     - Snaží se najít cestu ven.
       - Implementujte pomocí **Strategy** nebo **Polymorfismu**.

3. **Chování trpaslíků**
   - V bludišti postupně vložte 4 trpaslíky, každý po 5 sekundách:
     1. **Točí se doleva.**
     2. **Točí se doprava.**
     3. **Teleportuje se náhodně do cíle (Star Trek mód).**
     4. **Najde při vložení cestu k cíli a sleduje ji (algoritmus hledání cesty musí být dynamický, cesta nesmí být hard-coded, bludiště bude při testování změněno).**
   - Při každém kroku se vypíše aktuální pozice trpaslíka v bludišti (jedna iterace čeká 100 ms před dalším krokem).

4. **Dokončení**
   - Aplikace čeká, dokud všichni trpaslíci nedorazí do cíle.

## Požadavky
- **Platforma:** C# .NET 4.8
- **Důraz na:**
  - Správné datové typy pro uložení bludiště.
  - Čistý kód.
  - Nepřekreslování bludiště, pokud to není nutné.
  - Správná implementace polymorfismu/Strategy vzoru, ideálně s využitím Factory pro trpaslíky.


## Řešení úkolu

### Načtení bludiště
- Bludiště se načítá ze souboru `Maze.dat`, který je validován, aby všechny řádky měly stejnou délku.
- Program automaticky detekuje polohy startu (`S`) a cíle (`F`).

### Kontrola velikosti konzole*
- Velikost konzole je ověřována před spuštěním simulace. Pokud není dostatečná, uživatel je vyzván ke změně velikosti.

### Implementované strategie
Čtyři různé strategie pohybu trpaslíků jsou realizovány pomocí polymorfismu:
1. **Točení doleva:** Trpaslík používá algoritmus „levé stěny“.
2. **Točení doprava:** Trpaslík používá algoritmus „pravé stěny“.
3. **Teleportace:** Trpaslík se náhodně teleportuje na volné pozice v bludišti.
4. **Sledování cesty:** Trpaslík si pomocí BFS najde nejkratší cestu a sleduje ji.

### Překreslování bludiště
- Překresluje se pouze pohyb trpaslíků, vypsané pozice trpaslíků, aby se minimalizovalo zbytečné překreslování.

### Simulace
- Trpaslíci jsou do bludiště vkládáni postupně s odstupem 5 sekund.
- Pohyb probíhá v iteracích, každá iterace trvá 100 ms.
- Simulace končí, když všichni trpaslíci dosáhnou cíle.

---

## Spuštění aplikace
### Požadavky

1. **Platforma**: Windows 10 nebo novější.
2. **.NET SDK**: Verze 6.0 nebo novější. Stáhněte si ji z [oficiálního webu .NET](https://dotnet.microsoft.com/).

### Kroky ke spuštění

1. **Stažení projektu**:
   - Stáhněte projekt a umístěte soubor `maze.dat` do stejné složky jako aplikace.

2. **Kompilace a spuštění**:
   - Otevřete příkazovou řádku (`Command Prompt`) nebo PowerShell.
   - Přejděte do složky projektu:
     ```cmd
     cd cesta\k\složce
     ```
   - Spusťte aplikaci příkazem:
     ```cmd
     dotnet run
     ```

### Důležité upozornění

Aplikaci je třeba spouštět **pouze** v příkazové řádce (CMD) nebo PowerShellu v režimu **Windows Console Host**. 
Moderní terminály, jako je **Windows Terminal** nebo integrované terminály v IDE (např. Visual Studio Code), nepodporují změnu velikosti konzole prostřednictvím kódu, což může způsobit problémy s vykreslováním bludiště.

#### Jak spustit aplikaci:
1. Otevřete **CMD** nebo **PowerShell**:
   - Stiskněte **Win + R**, napište `cmd` nebo `powershell` a potvrďte klávesou Enter.
2. Spusťte aplikaci příkazem:
   ```bash
   Maze.exe


### Tip pro uživatele Windows 11

Aplikace vyžaduje správné nastavení konzole ve Windows 11. Ujistěte se, že máte změněno nastavení konzole:
1. Otevřete **Nastavení > Ochrana osobních údajů a zabezpečení > Pro vývojáře > Terminál**.
2. Změňte volbu **Hostitelská konzole** na **Konzole Windows** místo možnosti **Nechat rozhodnout Windows**.

