# KFS Tetris

Project for possible deploy game into XBOX one.

--

# MonoGame Xbox Deployment Checklist

Tento checklist ti pomůže připravit MonoGame projekt na Windows pro kompilaci a deploy na Xbox.

---

## 1. Přesun projektu na Windows

* [ ] Zkopíruj MonoGame projekt z Macu na Windows (např. přes Git nebo externí disk).
* [ ] Zahrň všechny soubory:

  * `.csproj` soubory (pokud existují, viz poznámka níže)
  * složka `Content/` s `*.mgcb` a texturami
  * fonty, obrázky, zvuky
  * všechny zdrojové `.cs` soubory
* [ ] Ověř, že projekt běží lokálně na PC (Debug/x64).

> Poznámka: Pokud nemáš `.csproj`, bude potřeba ho vytvořit ve Visual Studiu.

---

## 2. Instalace nástrojů na Windows

1. **Visual Studio 2022 Community**

   * Workloads:

     * .NET desktop development
     * Universal Windows Platform development
     * Game development with C++ (pro Xbox templates a GDK)

2. **MonoGame 3.8+**

   * Stáhni a nainstaluj [https://www.monogame.net/downloads/](https://www.monogame.net/downloads/)
   * Během instalace vyber **Visual Studio Templates**

3. **Xbox Game Development Kit (GDK)**

   * Stáhni přes [https://developer.microsoft.com/en-us/games/xbox/](https://developer.microsoft.com/en-us/games/xbox/)
   * Přihlas se svým Microsoft Account

4. **Xbox Developer Mode**

   * Nainstaluj **Dev Mode Activation app** na konzoli
   * Přepni Xbox do **Developer Mode**
   * Připoj Xbox do stejné sítě jako PC

---

## 3. Otevření projektu ve Visual Studio

* [ ] Otevři MonoGame projekt (`.csproj`) ve VS 2022.
* [ ] Přepni **Target framework** na `.NET 6.0` (pokud používáš novější MonoGame šablony).
* [ ] Zkontroluj **Content Pipeline**:

  * Otevři `Content.mgcb` přes **MGCB Editor**
  * Zkontroluj, že všechny textury, fonty a zvuky se buildí.
* [ ] Spusť projekt lokálně na PC.

---

## 4. Příprava UWP projektu pro Xbox

* [ ] Přidej nový projekt typu **MonoGame Windows Universal**

  * File → New → Project → MonoGame Windows Universal
  * Pojmenuj např. `MonoKfsTetris.Xbox`
* [ ] Přidej odkazy na existující zdrojové `.cs` soubory
* [ ] Přidej `Content.mgcb` a ověř build pro UWP
* [ ] Nastav Target Device:

  * x64 / ARM / ARM64 dle konzole
  * Debug / Release dle potřeby
* [ ] Připoj Xbox do sítě, zapni Developer Mode
* [ ] Properties projektu → Debug → Target device → Remote Machine → zadej IP Xboxu

---

## 5. Deploy na Xbox

* [ ] Vyber **Release / x64 / Remote Machine (Xbox)**
* [ ] Klikni **Deploy** → VS zkompiluje projekt a nahraje na konzoli
* [ ] Otestuj běh hry

---

## 6. Čistý projekt a Git

* [ ] Odstraň Mac-specifické soubory:

  * `.DS_Store`, `bin/`, `obj/`, `.vs/`
* [ ] Připrav `.gitignore` pro C# + MonoGame:

```
bin/
obj/
*.user
*.suo
.vs/
*.pdb
*.exe
*.dll
*.app
*.DS_Store
```

* [ ] Přidej repo, commitni čistou verzi
* [ ] Měj samostatné složky pro PC / Xbox build

---

💡 **Tipy:**

* Nejprve fungující PC build, až pak přidávej Xbox.
* Xbox build je většinou **UWP**, kontroluj přístup k souborům a MGCB build.
* HardDrop/lockDelay a GameOver logika funguje stejně na PC i Xbox.
