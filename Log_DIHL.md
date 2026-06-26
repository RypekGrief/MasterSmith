# Proje Gunlugu — DeathIsHighlyLikely

---

## Genel Bilgiler

- **Proje:** Death Is Highly Likely (Bannerlord Olum Orani Modu)
- **Gelistirici:** Rypek + OpenCode (deepseek-v4-pro)
- **Oyun:** Mount & Blade II: Bannerlord
- **API:** TaleWorlds.CampaignSystem, TaleWorlds.MountAndBlade, Harmony 2.x, MCMv5
- **.NET:** Framework 4.7.2, x64

---

## Yapilanlar

### Oznitelik 1 — Kusatma Olum Orani Artisi (Siege Death Rate Increase) (v3.3.0)

**Istenen:** Kusatma savaslarinda (duvar hucumu) asker olum orani MCM ayarinin x1.15 kati, kahraman olum orani x1.05 kati olsun. MCM toggle ile acilip kapatilabilsin.

**Uygulama:**

1. **DeathIsHighlyLikelySettings.cs** — Yeni MCM ayari eklendi:
   - `EnableSiegeDeathRateIncrease` (bool, default: true) — "Siege Settings" grubunda (GroupOrder = 3)

2. **DeathIsHighlyLikelyModel.cs** (3D savaslar) — Kusatma tespiti: `Mission.Current.IsSiegeBattle`
   - Hero branch: Age Factor sonrasi, clamp oncesi `customProbability *= 1.05f`
   - Troop branch: 0-clamp sonrasi, MinisterOfHealth oncesi `troopProbability *= 1.15f`

3. **MapSimulationDeathModel.cs** (harita simulasyonlari) — Kusatma tespiti: `party.MapEvent.IsSiegeAssault`
   - `using TaleWorlds.CampaignSystem.MapEvents` eklendi
   - Hero branch: reduction sonrasi `deathProb *= 1.05f`
   - Troop branch: 0-clamp sonrasi, MinisterOfHealth oncesi `troopDeathProb *= 1.15f`

4. **XML Doc Comments** — Tum .cs dosyalarindaki class, method ve property'lere Ingilizce `/// <summary>` eklendi (AGENTS.md MODE 6).

**Dogrulanan API'ler (Decompiled):**

| API | Konum |
|-----|-------|
| `Mission.IsSiegeBattle` (bool, read-only) | `TaleWorlds.MountAndBlade/Mission.cs:1844` |
| `Mission.IsFieldBattle` (bool, read-only) | `TaleWorlds.MountAndBlade/Mission.cs:1834` |
| `Mission.IsSallyOutBattle` (bool, read-only) | `TaleWorlds.MountAndBlade/Mission.cs:1854` |
| `Mission.MissionTeamAITypeEnum` (enum: NoTeamAI, FieldBattle, Siege, SallyOut, NavalBattle, NavalRaid) | `TaleWorlds.MountAndBlade/Mission.cs:8326` |
| `MapEvent.IsSiegeAssault` (bool, read-only) | `TaleWorlds.CampaignSystem/MapEvents/MapEvent.cs:407` |
| `MapEvent.IsSiegeOutside` (bool, read-only) | `TaleWorlds.CampaignSystem/MapEvents/MapEvent.cs:437` |
| `MapEvent.IsSallyOut` (bool, read-only) | `TaleWorlds.CampaignSystem/MapEvents/MapEvent.cs:427` |
| `MapEvent.EventType` (BattleTypes, read-only) | `TaleWorlds.CampaignSystem/MapEvents/MapEvent.cs:341` |
| `MapEvent.BattleTypes` enum (None, FieldBattle, Raid, Siege, Hideout, SallyOut, SiegeOutside, ...) | `TaleWorlds.CampaignSystem/MapEvents/MapEvent.cs:2718` |
| `PartyBase.MapEvent` (MapEvent, read-only) | `TaleWorlds.CampaignSystem/Party/PartyBase.cs:565` |

**Not:** Sadece `Siege` (duvar hucumu) dikkate alinmistir. `SiegeOutside` ve `SallyOut` dahil edilmemistir.

**Surum Degisikligi:**
- `DeathIsHighlyLikelySettings.Id`: `DeathIsHighlyLikely_v3_2_0` → `DeathIsHighlyLikely_v3_3_0`
- `SubModule.xml` Version: `v3.2.0` → `v3.3.0`

---

## Duzenleme Gecmisi

| # | Islem | Aciklama |
|---|-------|----------|
| 1 | Oznitelik | Kusatma olum orani artisi (Siege Death Rate Increase) eklendi |
| 2 | Dokumantasyon | 4 .cs dosyasindaki tum kod bloklarina XML doc comment eklendi |
| 3 | Surum | Settings Id ve SubModule.xml v3.3.0'a guncellendi |
| 4 | Log | Log_DIHL.md olusturuldu |
| 5 | API Ref | HowWork.md'ye yeni API'ler eklendi |

---

## Iletisim

- Kullanici: Rypek
- Asistan: OpenCode (deepseek-v4-pro)
- Dil: Turkce (proje konusma ve dokumantasyon dili)

---

## Referans Yollari

```
Oyun:           C:\Games\Mount & Blade II Bannerlord\
Decompiled CS:  C:\Users\USER\Desktop\Modding\RAG\Bannerlord DLLS\
  ├── Official\    (TaleWorlds.CampaignSystem, Core, MountAndBlade, ...)
  └── Frameworks\  (0Harmony, ButterLib, UIExtenderEx, MCMv5)
Proje:          C:\Users\USER\source\repos\DeathIsHighlyLikely\
```
