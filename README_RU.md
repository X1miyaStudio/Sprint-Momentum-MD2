<div align="center">

<img src="https://capsule-render.vercel.app/api?type=waving&color=0:0D1117,50:F59E0B,100:00FF88&height=180&section=header&text=Sprint%20Momentum&fontSize=40&fontColor=ffffff&fontAlignY=40" width="100%" alt="Sprint Momentum Banner" />

<p align="center">
  <b>Мгновенный спринт, система выносливости и таран врагов на полном разгоне для Minecraft Dungeons II.</b><br/>
  Переносит идеи SprintMod и Sprint Momentum в MD2 с сегментной шкалой стамины в стиле игры и настройками в Blueprint Loader 2.0+.
</p>

<p align="center">
  <a href="README.md">🇬🇧 English documentation</a>
</p>

</div>

---

## ✨ Возможности

* **3 режима активации:** `Hold to Sprint` (удержание `LeftShift`), `Toggle Sprint` (переключение) и `Auto-Sprint` (с поддержкой второй привязки на геймпад).
* **3 стиля спринта:**
  * **Instant + Ram (Sharp Start)** *(по умолчанию)* — резкое ускорение сразу при нажатии + зарядка моментума для тарана врагов.
  * **Classic SprintMod (Instant)** — чистый мгновенный спринт без тарана.
  * **Gradual Ramp + Ram** — плавный разгон скорости.
* **Умная стамина без ложного расхода:** отслеживает реальное смещение в мире и столкновение со стенами (`IsOwnerRunningIntoWall`), поэтому выносливость не тратится, если упереться в стенку. При `Stamina Drain / sec = 0` спринт становится бесконечным.
* **Таран врагов (через GAS):** на высоком моментуме столкновение с врагами наносит реальный урон здоровью через `UATR_Health` / `SWCoreGASUtil`, отбрасывает (`PullCharacter` + `LaunchCharacter`), вызывает вспышку удара и звук.
* **HUD в стиле Minecraft Dungeons II:** 8 объёмных делений стамины в трёхслойной рамке, процент/индикатор `RAM!` с тенью, тонкая полоска заряда и выбор позиции (`Above Hotbar` над слотами артефактов по умолчанию).

---

## 📥 Требования и установка

1. Установите **Blueprint Loader 2.0+** в `Dungeons\Content\Paks\~mods\BlueprintLoader\`.
2. Скачайте `SprintMomentum-1.0.0.zip` из [GitHub Releases](https://github.com/X1miyaStudio/Sprint-Momentum-MD2/releases).
3. Закройте **Minecraft Dungeons II** и распакуйте папку `SprintMomentum` в:
   * **Steam:** `<SteamLibrary>\steamapps\common\Minecraft Dungeons II\Dungeons\Content\Paks\~mods\SprintMomentum\`
   * **Xbox App / Game Pass:** `<Drive>:\XboxGames\Minecraft Dungeons II\Content\Dungeons\Content\Paks\~mods\SprintMomentum\`
4. Запустите игру и настройте мод во вкладке **Settings → Mods → Sprint Momentum**.

---

## 🛠️ Сборка из исходников

Требуется **.NET 10 SDK** и `NeoRune`:

```powershell
dotnet build -c Release
neorune pack
```

---

<div align="center">
  <b>— X1miyaStudio🧪</b><br/>
  <a href="https://t.me/X1miyaStudio">Telegram</a> · <a href="https://discord.gg/pwk6X8ecNJ">Discord</a> · <a href="https://www.youtube.com/@X1miyaStudio">YouTube</a>
</div>
