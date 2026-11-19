<div align="center">

<img src="https://capsule-render.vercel.app/api?type=waving&color=0:0D1117,50:F59E0B,100:00FF88&height=180&section=header&text=Sprint%20Momentum&fontSize=40&fontColor=ffffff&fontAlignY=40" width="100%" alt="Sprint Momentum Banner" />

[![Game](https://img.shields.io/badge/Minecraft_Dungeons_II-UE_5.6-00FF88?style=for-the-badge)](https://github.com/X1miyaStudio/Sprint-Momentum-MD2)
[![Release](https://img.shields.io/badge/GitHub_Release-v1.0.0-181717?style=for-the-badge&logo=github&logoColor=white)](https://github.com/X1miyaStudio/Sprint-Momentum-MD2/releases/tag/v1.0.0)
![CurseForge](https://img.shields.io/badge/CurseForge-Under_Review-F16436?style=for-the-badge&logo=curseforge&logoColor=white)
![Loader](https://img.shields.io/badge/Blueprint_Loader-2.0%2B-blueviolet?style=for-the-badge)
[![SDK](https://img.shields.io/badge/Built_with-NeoRune-00BFFF?style=for-the-badge)](https://github.com/X1miyaStudio/Sprint-Momentum-MD2)
[![Studio](https://img.shields.io/badge/Studio-X1miyaStudio-064E3B?style=for-the-badge)](https://t.me/X1miyaStudio)

<p align="center">
  <b>Instant sprint, stamina management, and high-momentum enemy ramming for Minecraft Dungeons II.</b><br/>
  Brings SprintMod & Sprint Momentum mechanics into MD2 with a native-styled segmented stamina HUD and Blueprint Loader 2.0+ settings.
</p>

<p align="center">
  <img src="https://img.shields.io/badge/Language-English-00FF88?style=flat-square" alt="English" />
  <a href="README_RU.md">
    <img src="https://img.shields.io/badge/%D0%AF%D0%B7%D1%8B%D0%BA-%D0%A0%D1%83%D1%81%D1%81%D0%BA%D0%B8%D0%B9-0D1117?style=flat-square&logo=googletranslate&logoColor=white" alt="Русская версия" />
  </a>
</p>

</div>

---

## ✨ Features

* **3 Activation Modes** — `Hold to Sprint` (default `LeftShift`), `Toggle Sprint`, and `Auto-Sprint` (supports secondary gamepad keybinds).
* **3 Sprint Styles**:
  * **Instant + Ram (Sharp Start)** *(default)* — immediate full sprint speed on key press while building momentum for enemy ramming.
  * **Classic SprintMod (Instant)** — pure instant sprint speed boost without ramming.
  * **Gradual Ramp + Ram** — smooth speed acceleration up to full momentum.
* **Stamina & Wall-Collision Safety** — tracks real world displacement and `SWMovementComponent.IsOwnerRunningIntoWall()` so stamina never drains when standing still or pushing into walls. Set `Stamina Drain / sec` to `0` for infinite sprint.
* **Momentum Ram (GAS Integration)** — colliding with hostile targets at high momentum applies real GAS Health damage (`UATR_Health` via `SWCoreGASUtil`), knockback (`PullCharacter` + `LaunchCharacter`), hit flash, and impact audio.
* **Minecraft Dungeons II Styled HUD** — 8 beveled stamina segments, compact percentage/`RAM!` readout, thin momentum charge line, and configurable screen positions (`Above Hotbar` by default so it stays clear of artifact slots).

---

## 📥 Requirements & Installation

1. Install **Blueprint Loader 2.0+** into `Dungeons\Content\Paks\~mods\BlueprintLoader\`.
2. Download `SprintMomentum-1.0.0.zip` from [GitHub Releases](https://github.com/X1miyaStudio/Sprint-Momentum-MD2/releases).
3. Close **Minecraft Dungeons II** and extract the `SprintMomentum` folder into:
   * **Steam:** `<SteamLibrary>\steamapps\common\Minecraft Dungeons II\Dungeons\Content\Paks\~mods\SprintMomentum\`
   * **Xbox App / Game Pass:** `<Drive>:\XboxGames\Minecraft Dungeons II\Content\Dungeons\Content\Paks\~mods\SprintMomentum\`
4. Launch the game and configure the mod under **Settings → Mods → Sprint Momentum**.

---

## 🛠️ Building from Source

Requires **.NET 10 SDK**, `NeoRune.Templates`, and `NeoRune.Tool`:

```powershell
dotnet build -c Release
neorune pack
```

---

<div align="center">
  <b>— X1miyaStudio🧪</b><br/>
  <a href="https://t.me/X1miyaStudio">Telegram</a> · <a href="https://discord.gg/pwk6X8ecNJ">Discord</a> · <a href="https://www.youtube.com/@X1miyaStudio">YouTube</a>
</div>
