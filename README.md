# 𝑮𝒓𝒆𝒆𝒏 𝑯𝒐𝒓𝒊𝒛𝒐𝒏

> Ein nachhaltiger 3D-Stadtbau-Simulator, gebaut mit Unity 6 im Rahmen einer Umschulung an der SRH Fachschulen GmbH.

![Unity Version](https://img.shields.io/badge/Unity-6000.4.5f1-black)
![Render](https://img.shields.io/badge/Pipeline-URP-blue)
![Sprache](https://img.shields.io/badge/C%23-.NET-purple)
![Team](https://img.shields.io/badge/Team-5%20Personen-green)

---

## 📋 Projektinfo

| | |
|---|---|
| **Genre** | 3D City-Builder / Wirtschaftssimulation |
| **Engine** | Unity 6000.4.5f1 |
| **Render Pipeline** | Universal Render Pipeline (URP) |
| **Input** | Unity Input System |
| **Perspektive** | 3D, freie RTS-Kamera |
| **Arbeitsform** | Teamarbeit (5 Personen) |
| **Team / Studio** | Pausenklingel Crew UG |
| **Versionierung** | Git / GitHub |

---

## 📸 Screenshot

<!-- Hier Screenshot einfügen -->

---

## 🎮 Was ist das?

Green Horizon ist ein 3D-Stadtbausimulator in der Tradition von SimCity, nur mit Fokus auf Nachhaltigkeit. Der Spieler baut eine Stadt, trifft dabei aber Entscheidungen, die den **CO₂-Fußabdruck** der Stadt beeinflussen. Ziel ist eine wachsende, glückliche und energieautarke Stadt, ohne dabei die Umwelt zu zerstören.

Jedes Gebäude, jede Straße und jede Energieentscheidung hat Konsequenzen. Erneuerbare Energien senken den CO₂-Ausstoß, Industrie- und Wohngebäude erhöhen ihn und verbrauchen Strom. Ein tägliches Wirtschafts-Tick verrechnet Einnahmen, Verschmutzung und Energiebilanz. Dazu kommen Zufallsereignisse wie Dürren oder Überflutungen, die die Stadt herausfordern. Den Rahmen bildet ein Phasensystem, das die Stadt von der Gründung bis zur nachhaltigen Wende begleitet.

---

## ✨ Features

- ✅ Freies Bauen auf einem **Grid-System** mit Gebäuden und Straßen
- ✅ **Automatische Straßenerkennung**: Geraden, Ecken, T-Kreuzungen und Kreuzungen werden beim Ziehen automatisch korrekt platziert und rotiert
- ✅ **CO₂-Budget-System**: jede Bauentscheidung beeinflusst den Fußabdruck, und eine Überschreitung kostet Strafen
- ✅ **GreenCoins** als Währung mit täglichen Einnahmen pro Gebäude
- ✅ **Circular Economy**: Recycling-Gebäude senken CO₂ und sparen Kosten
- ✅ **Energiebilanz**: Verbrauch gegen erneuerbare Erzeugung (Wind, Solar, Wasser)
- ✅ **Zufriedenheitssystem** mit visueller Smiley-Rückmeldung
- ✅ **Spielphasen**: Start, Growth, Crisis, Change
- ✅ **Zufallsereignisse**: Dürren, Überflutungen, Hitzewellen, politische Unruhen
- ✅ **Verkehrs-KI**: Autos und Fußgänger mit Wegfindung und Rush-Hour-Logik
- ✅ **Tag/Nacht-Zyklus** mit dynamischer Beleuchtung und Gebäude- und Autolichtern
- ✅ **Tutorial-System** und ein interaktives **Nachhaltigkeits-Quiz**
- ✅ Baumenü mit Kategorien, Gebäude-Tooltips und Abhängigkeiten
- ✅ Demolish-Modus (auch als Drag), Gebäude-Rotation, Bau-Vorschau (grün/rot)
- ✅ Pause-Menü mit Slide-Animation, Einstellungen (Auflösung, Grafik, Lautstärke, Vollbild)

---

## 🕹️ Steuerung

| Taste | Funktion |
|---|---|
| `W` / `A` / `S` / `D` | Kamera bewegen |
| `Rechte Maustaste` | Kamera drehen |
| `Mausrad` | Zoomen |
| `Tab` | Baumenü öffnen / schließen |
| `1` … `6` | Kategorie-Tabs im Baumenü |
| `Linke Maustaste` | Gebäude platzieren / Straße ziehen (Drag) |
| `R` | Gebäude rotieren |
| `X` | Demolish-Modus (Abriss, auch als Drag) |
| `Escape` | Pause / laufende Platzierung abbrechen |

Die Kamera hält sich an Bewegungs-Grenzen (`minBounds` und `maxBounds`), interpoliert weich und ist im Pause-Zustand deaktiviert.

---

## 🔄 Gameplay-Loop & Spielphasen

Den Fortschritt steuert der **`PhaseManager`** anhand von Einwohnerzahl und CO₂-Level:

```
Start ──(Einwohner ≥ 50)──► Growth ──(CO₂ ≥ 70 %)──► Crisis ──(Einwohner ≥ 200 und CO₂ ≤ 40 %)──► Change
```

| Phase | Bedeutung | Übergangsbedingung |
|---|---|---|
| **Start** | Gründung der Stadt | Einwohner ≥ `residentsForGrowth` (50) |
| **Growth** | Stadt wächst schnell | CO₂ ≥ `co2ForCrisis` (0.7) |
| **Crisis** | Umweltkrise, Handlungsdruck | Einwohner ≥ `residentsForChange` (200) und CO₂ ≤ `co2ForChange` (0.4) |
| **Change** | Nachhaltige Wende erreicht (Zielzustand) | |

CO₂ wird dabei auf `currentFootprint / maxFootprint` normalisiert (Default 1000 t). Ein Phasenwechsel feuert das Event `OnPhaseChanged`, an das sich andere Systeme hängen können.

---

## 📅 Der Tages-Tick (Wirtschaftskern)

Der wirtschaftliche Kern der Simulation ist der **`CityTickManager`**. Er zählt über `daysPerSecond` die vergangenen Tage. Bei jedem neuen Tag läuft ein Wirtschafts-Tick über alle platzierten Gebäude:

```csharp
Buildings.ForEach(b => {
    totalIncome    += b.IncomePerHour * 24;
    totalPollution += b.Pollution;
    totalResidents += b.Residents;
});

GreenCoinManager.Instance.AddGold(totalIncome);                       // 1. Einnahmen
totalPollution = CircularEconomyManager.Instance.ProcessDayTick(...); // 2. Recycling zieht CO₂ ab
Co2BudgetManager.Instance.AddPollution(totalPollution);              // 3. Rest-CO₂ aufs Budget
EnergyBalanceManager.Instance.ProcessDayTick();                      // 4. Energiebilanz prüfen
FloatingNumberManager.Instance.ShowResidentChange(diff);            // 5. UI-Feedback
```

**Reihenfolge pro Tag:**
1. **Einnahmen** aller Gebäude wandern in die GreenCoins
2. **Circular Economy**: Recycling-Gebäude reduzieren die Brutto-Verschmutzung und bringen Kostenersparnis
3. **CO₂-Budget**: die Netto-Verschmutzung wird auf den Fußabdruck addiert
4. **Energiebilanz**: Über- oder Unterversorgung mit Strom wird geprüft (Strafen bei Defizit)
5. **UI-Update**: Gold, Einwohner und Floating Numbers

Der Tick pausiert komplett, wenn der `GameStateManager` auf `Paused` steht.

---

## 🏗️ Bau-System (`Dennis/Placement/Building/`)

Zentrale Klasse ist **`BuildingSystem`** (Singleton). Sie verwaltet Platzierungs-, Straßen- und Demolish-Modus und arbeitet eng mit dem Grid zusammen.

- **`BuildingGrid`**: 2D-Zellenraster (`BuildingGridCell[,]`), das Buch führt über belegte Zellen, Straßenzellen (`_roadCells`) und die Zuordnung von Gebäuden zu Zellen. Rechnet Welt- und Gridkoordinaten ineinander um (`CellSize = 1`).
- **`BuildingData`** (ScriptableObject): alle Gebäude-Eigenschaften an einem Ort:

  | Feld | Bedeutung |
  |---|---|
  | `Cost` | Baukosten in GreenCoins |
  | `RequiresRoad` | benötigt Straßenanschluss |
  | `RequiredBuilding` | Abhängigkeit von anderem Gebäude |
  | `Residents` | Einwohner |
  | `IncomePerHour` | Einkommen pro Stunde |
  | `Pollution` | Umweltverschmutzung |
  | `EnergyUsage` | Stromverbrauch (kW) |
  | `CO2Reduction` | CO₂-Reduktion pro Tag (Recycling) |
  | `CostSavingsPerDay` | Ersparnis pro Tag |
  | `GridSize` | Grundfläche (z. B. „2x3") |

- **`BuildingPreview`**: Vorschau-Objekt mit Gültigkeits-Feedback (grün = platzierbar, rot = blockiert)
- **`BuildingShapeUnit` / `BuildingModel`**: mehrzellige Gebäudeformen und Modelldaten
- **Validierung:** Die Platzierung scheitert mit einer **Announcement**, wenn kein Straßenanschluss (`noRoadAnnouncement`) oder ein fehlendes Voraussetzungs-Gebäude (`noRequirementAnnouncement`) vorliegt.

---

## 🛣️ Straßen-System (`Dennis/Placement/Road/`)

Straßen zieht man per **Drag** (`RoadDragHandler`), und sie werden automatisch zur passenden Form aufgelöst. Der **`RoadResolver`** ist eine reine Utility-Klasse. Er bestimmt aus den vier Nachbarn (N/S/O/W) das richtige Prefab und die Rotation. Im Grunde ist das ein Marching über eine 4-Bit-Maske:

```csharp
connections switch {
    4                    => (crossPrefab,     0°),   // Kreuzung
    3 when n && s && e   => (tJunctionPrefab, 180°), // T-Stück
    2 when n && e        => (cornerPrefab,   -90°),  // Ecke
    2 when e && w        => (straightPrefab,   0°),  // Gerade
    1 when n || s        => (straightPrefab,  90°),  // Endstück
    ...
};
```

- **`RoadData`** (ScriptableObject) hält die Prefabs (`straightPrefab`, `cornerPrefab`, `tJunctionPrefab`, `crossPrefab`).
- Während des Ziehens werden Nachbarzellen live neu bewertet (`_toRecheck`), sodass bestehende Straßen ihre Form anpassen.
- Der Handler nutzt **Object Pooling** für die Vorschau-Segmente.

---

## 💰 Wirtschaft & Kreislauf (`GreenCoinManager`, `CircularEconomyManager`)

- **`GreenCoinManager`** (Singleton): Startkapital 10 000 GreenCoins. Die API besteht aus `AddGold`, `SpendGold` (schlägt bei zu wenig Geld fehl) und `SetGold` und feuert `OnGoldChanged` für die UI.
- **`CircularEconomyManager`**: summiert `CO2Reduction` und `CostSavingsPerDay` aller Recycling-Gebäude, schreibt die Ersparnis täglich gut und gibt die **Netto-Verschmutzung** an den Tick zurück.

---

## 🌫️ CO₂-System (`Furkan/Co2BudgetManager`, `Andy/Co2Manager`)

- **`Co2BudgetManager`** (Singleton, `DontDestroyOnLoad`): führt den `currentFootprint` in Tonnen. Beim Überschreiten von `maxFootprint` (1000) fällt täglich `pollutionPenalty` (100) an, ab `warningThreshold` (80 %) gibt es Warnungen. Feuert `OnBudgetChanged`.
- **`Co2Manager`**: die visuelle Anzeige. Sie füllt ein UI-Image und färbt es von Weiß nach Rot, je näher der Wert an 1.0 kommt.

---

## ⚡ Energie (`Samil/EnergyBalanceManager`, `Can/RenewableEnergy`)

- **`EnergyBalanceManager`**: berechnet `TotalEnergyDemand` (Summe aus `EnergyUsage`) gegen `EnergySupply` (erneuerbare Erzeugung). Bei Unterversorgung (`IsUnderSupplied`) gibt es täglich Gold- und Happiness-Strafen, mit Cooldown und Announcement.
- **`RenewableEnergy`**: verwaltet Investitionen in **Wind, Solar und Wasser**. Jede `EnergyInvestment` hat Kosten, Energie-Output, Happiness-Bonus und einen CO₂-Reduktions-Level (1 bis 3).

---

## 😊 Zufriedenheit (`Happinessmanager`, `HappinessThreshold`)

Das Zufriedenheits-System sammelt Boni und Mali aus Energie, Ereignissen und erneuerbaren Energien und zeigt das Ergebnis über **Schwellenwert-Smileys** (`HappinessThreshold`, also ein Sprite ab einem bestimmten Prozentwert) im HUD an.

---

## 🌪️ Zufallsereignisse (`Furkan/Ereignisse`)

Der `Ereignisse`-Manager überwacht CO₂, Geld und Müll und löst je nach Situation Krisen aus:

| Ereignis | Bedingung (Beispiel) | Auswirkung |
|---|---|---|
| **Dürre** | CO₂ = 70 | Geld -10, Müll +5, CO₂ steigt |
| **Überflutung** | CO₂ = 75 und Müll > 50 | Schäden |
| **Hitzewelle** | CO₂ = 50 | Belastung |
| **Politische Unruhen** | CO₂ = 50 und Geld < 30 | Unzufriedenheit |

Jedes Event nutzt ein Trigger-Flag, damit es nicht mehrfach direkt hintereinander feuert.

---

## 🌅 Tag/Nacht-Zyklus (`Dennis/DayAndNight/`)

- **`LightManager`** (`[ExecuteAlways]`): treibt `timeOfDay` (0 bis 24 h) voran, mit getrennter Dauer für Tag (6 bis 18 Uhr) und Nacht. Interpoliert Ambient-, Fog- und Sonnenfarbe über einen **`LightingPreset`** (ScriptableObject mit Gradients) und rotiert das Directional Light. Feuert `OnDayNightChanged`.
- **`BuildingLightController` / `CarLightController`**: schalten Gebäude- und Fahrzeuglichter passend zur Tageszeit.

---

## 🚗 Verkehrs-KI (`Furkan/`)

- **`AiDirector`**: spawnt Autos und Fußgänger zwischen Häusern und „Special Structures", mit Limits (`maxActiveCars`, `maxActivePedestrians`), Dichteprüfung und einem **Rush-Hour-Multiplikator** (8 bis 18 Uhr).
- **`CarAI` / `PedestrianAI`**: folgen berechneten Pfaden auf getrennten Spuren (`laneOffset`).
- **`AdjacencyGraph`**: ein Graph über die Straßenzellen für die Wegfindung, getrennt für Autos und Fußgänger.
- **`CarController` / `CarSpawner` / `RoadMarker` / `Marker`**: Fahrphysik, Spawning und Wegpunkte.

---

## 🎓 Quiz & Tutorial (`Samil/Quiz/`, `Dennis/Tutorial/`)

- **`QuizManager`**: ein Nachhaltigkeits-Quiz. Die Fragen kommen aus einer `TextAsset` und werden über `QuizTextParser` zu `QuizQuestion`-Objekten geparst. Es gibt eine Intro-Tafel, gemischte Fragen, Feedback pro Antwort und ein Ergebnis-Panel mit Retry und Continue. Aufrufen lässt sich das Quiz über `BlackboardInteractable` in der Spielwelt, und bei Bestehen geht es in die `RealScene`.
- **`TutorialManager`**: eine Schritt-für-Schritt-Einführung (`TutorialStep[]`) mit Panel und „Weiter"-Button.

---

## 🧱 Architektur & technische Highlights

- **Generisches Singleton** (`Dennis/Manager/Singleton<T>`) für die meisten Manager. Einige nutzen ein manuelles Singleton mit `DontDestroyOnLoad`.
- **Event-getriebene Kommunikation:** `OnGoldChanged`, `OnBudgetChanged`, `OnPhaseChanged`, `OnDayNightChanged` und `OnGameStateChanged` entkoppeln die Systeme voneinander.
- **ScriptableObjects** für alle Datendefinitionen: `BuildingData`, `RoadData`, `LightingPreset` und `AnnouncementData`. Damit liegt das Balancing im Editor statt im Code.
- **`GameStateManager`:** hält den zentralen Zustand (Gameplay oder Paused), gesetzt beim Szenenwechsel. Tick und Kamera reagieren darauf.
- **Object Pooling** für Straßen-Previews und Verkehrsagenten.
- **Modularer Aufbau:** die Scripts sind nach Teammitglied und Domäne in Namespaces getrennt (`Dennis.Placement`, `Furkan`, `Samil.Manager`, `Andy.Manager`, `Can.Manager`).

---

## 🎬 Szenen

| Szene | Rolle |
|---|---|
| `Assets/Scenes/MainMenu.unity` | Hauptmenü (Start-Szene) |
| `Assets/Scenes/MainScene.unity` | Hauptspiel (Stadtbau) |
| `TestScene - *` | Entwickler-Testszenen je Teammitglied |

---

## 📂 Projektstruktur

```
Assets/Scripts/
├── Dennis/          Building- & Road-System, Grid, Game-Phasen,
│   ├── Placement/   BuildingSystem, BuildingGrid, RoadResolver, RoadDragHandler
│   ├── Gamephase/   PhaseManager, GamePhase, Events
│   ├── Manager/     GreenCoinManager, Singleton<T>
│   ├── DayAndNight/ LightManager, LightingPreset
│   └── Tutorial/    TutorialManager, TutorialStep
├── Andy/            UI, Kamera, GameState, Pause
│   ├── Manager/     GameStateManager, PauseMenuController, TabManager,
│   │                TooltipManager, AnnouncementManager, FloatingNumberManager
│   └── CityStats/   Co2Manager, HappinessUiManager, HappinessThreshold
├── Furkan/          CO2-Budget, Ereignisse, Verkehrs-KI
│                    Co2BudgetManager, Ereignisse, AiDirector, CarAI,
│                    PedestrianAI, AdjacencyGraph, PlacementManager
├── Samil/           Tick, Energie, Kreislauf, Quiz, Menü-Kamera
│   ├── Manager/     CityTickManager, EnergyBalanceManager, CircularEconomyManager
│   └── Quiz/        QuizManager, QuizQuestion, QuizTextParser, BlackboardInteractable
└── Can/             Erneuerbare Energien, Hauptmenü, Audio
                     RenewableEnergy, Happinessmanager, Mainmenu, Audiomanager
```

---

## 🚀 Installation & Start

1. Repository klonen:
   ```bash
   git clone https://github.com/Keradean/Green-Horizon.git
   ```
2. Unity Hub öffnen, dann **Add project from disk**
3. Unity-Version **6000.4.5f1** auswählen
4. Projekt öffnen und die Szene **`Assets/Scenes/MainMenu.unity`** laden
5. Play drücken

---

## 🎨 Third-Party-Assets

- **AllSkyFree**: Skyboxen für den Tag/Nacht-Zyklus (Cartoon Sky, Night, Sunset und weitere)
- **Simple Vehicle Pack**: Fahrzeugmodelle für den Verkehr
- **nTools Prefab Painter**: Editor-Tool zum Platzieren von Deko-Prefabs
- **TextMesh Pro**: UI-Text
- Audio-Assets für Buttons und Ambiente

*Alle verwendeten Assets sind lizenzfrei oder entsprechend lizenziert.*

---

## 🧑‍🤝‍🧑 Team

| Name | Rolle |
|---|---|
| **Dennis** | Lead / Projektmanagement / Building-System / Road-System / Grid / Tag-Nacht / Tutorial |
| **Andy** | UI / Manager-Systeme / Kamera / Pause / GameState |
| **Furkan** | CO₂-System / Ereignisse / Verkehrs-KI |
| **Samil** | Tick-Manager / Energiebilanz / Circular Economy / Quiz |
| **Can** | Erneuerbare Energien / Hauptmenü / Audio |

---

## 📜 Lizenz

Dieses Projekt wurde zu Ausbildungszwecken erstellt.
Alle Rechte vorbehalten, SRH Fachschulen GmbH / Pausenklingel Crew UG.
</content>
