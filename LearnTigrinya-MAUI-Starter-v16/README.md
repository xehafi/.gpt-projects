# Learn Tigrinya — .NET MAUI Starter (MVP skeleton)

This repo contains a runnable starter for the Learn Tigrinya app:
- Shell with tabs: **Learn**, **Review (SRS)**, **Explore (Alphabet Lab)**, **Profile (Avatars & settings)**
- Sample content loader for *Alphabet I*
- SRS engine stub (SM-2 variant)
- Minimal Alphabet Lab (tap ሀ–ሆ buttons)
- Avatar picker with 9 Eritrean groups (names as placeholders)

## Prerequisites
- Visual Studio 2022 (17.8+) with **.NET MAUI** workload (Android SDKs)
- .NET 8 SDK
- Android emulator or a device

## Run
1. Open `TigrinyaApp.sln` in Visual Studio.
2. Select **Android** and press **Run**.

## Fonts
For reliable Ethiopic rendering, add a font like **Noto Sans Ethiopic** to `Resources/Fonts`, then register it in `MauiProgram.cs` under `ConfigureFonts`.

## Next steps
- Lesson player (render trace/listen_select/read_tile from JSON)
- Trace canvas via SkiaSharp (stroke capture + tolerance scoring)
- Audio assets (Opus) and playback service
- SQLite persistence of SRS and progress
- Avatar art + outfits (culturally reviewed)


## New in v3
- **AudioService** with Plugin.Maui.Audio + Text-to-Speech fallback.
  - Place audio under `Resources/Raw/content/` and reference in JSON (e.g., `ha_01.opus`). If missing, the app will TTS the fallback.
- **SQLite persistence for SRS** (`learn_tigrinya.db3` in app data). SRS items load/save automatically.

### Notes
- Add an Ethiopic font (e.g., `NotoSansEthiopic.ttf`) to `Resources/Fonts` and register in `MauiProgram.cs` for consistent rendering.


## New in v4
- **Mini Ethiopic Keyboard** (`type_mini_keyboard` task): on-screen Ethiopic keys, backspace/space/clear, simple exact-match check.
- **Picture-Word task** (`picture_word`): selects a picture (placeholder emoji) matching the prompt. Replace with real images later via MAUI assets.


## New in v5
- **Trace scoring with glyph templates**: vector templates are rasterized to a heatmap (256×256). Your strokes are rasterized and scored by **overlap / template area**. Threshold = 60% (tweakable).
  - Templates live under `Resources/Raw/templates/ethiopic/*.json` (normalized 0–1 points). Currently seeded for **ሀ** (`ha.json`).
- **Real image assets for `picture_word`**: sample PNGs under `Resources/Images/animals/`. Replace these placeholders with your real art; references in JSON use paths like `animals/cow.png`.


## New in v7
- **Dialog Builder** (`dialog_goal`): goal-oriented micro-dialogues with audio buttons and multiple-choice lines; completes step-by-step.
- **Grammar Tips inline** (`grammar_tip`): compact tip with examples (Tigrinya + literal gloss + translation) and a quick check quiz.
- Added **Greetings I** lesson demonstrating both.


## New in v8
- **Avatar Editor v1** on Profile: choose base language, one of 9 Eritrean groups, and an outfit. Profile is saved in SQLite (`UserProfile`).
- **Numbers 0–20** skill with tasks:
  - `listen_number` (audio/TTS of a number → multiple choice)
  - `type_number` (numeric keypad entry)
  - `price_match` (pick matching price format; € examples included)


## New in v9
- **Image-based content packs**: `Animals I` and `Household I` with picture selection, label matching (EN/NL), and flashcards; assets under `Resources/Images/animals` and `Resources/Images/household`.
- **Placement test at first run**: quick 3-question check (letter, number, greeting). Stores **PlacementLevel** (A0/A1/A2) and suggests two starting skills. Suggested skills are highlighted as **Recommended** in the Learn tab.


## New in v10
- **Tigrinya labels + audio scaffolding** added to Animals/Household lessons:
  - `label_match` choices now include a `tig` field.
  - `flashcard` shows large Tigrinya text with a **▶ audio** button (uses `AudioService`, falls back to TTS).
  - Placeholder audio paths under `Resources/Raw/content/audio/...` (drop in real recordings anytime).
- **Start here panel** at the top of **Learn**:
  - Highlights up to two **Recommended** skills from placement results, with one-tap **Start** buttons.


## New in v11
- **Dynamic label language:** `label_match` now respects the profile’s Base Language (English or Dutch) when rendering labels.
- **Per-item audio:** `picture_word` and `label_match` include a ▶ button that plays the target Tigrinya word (file or TTS fallback).
- **Content QA checklist:** New QA page (Explore → *Open QA Review*) for native reviewers to approve spelling, audio, glosses, and cultural notes.
  - Checklists live in `Resources/Raw/content/qa/*.qa.json`.
  - Approvals are saved in SQLite (`QaRecord`) with reviewer name and timestamp.


## New in v12
- **Dutch UI strings** for Placement, QA, and Learn headers/buttons via a lightweight `LocalizationService` tied to the saved Base Language.
- **XP + Daily goals**: tracks Total XP, Today XP vs Goal, Streak (🔥), and Weekly XP. Lesson completion now grants **+10 XP + crown bonus (×5)** and shows it on completion.
- **Learn header banner** displays Daily Goal, Streak, and Week XP; updates automatically as you progress.


## New in v13
- **Daily goal picker** in Profile (10/20/30/50 XP). Saves to SQLite and updates the Learn banner.
- **Streak notifications**: on lesson completion, you’ll see 🔥 streak bump and “Daily goal met” when applicable.
- **More Dutch translations** across Explore/Review headers and Profile labels; Learn’s “Start” button is localized.


## New in v14 (Content sprint)
- **Family I** (mother/father/sister/brother + greetings tie-in), **Body I**, **Fruits & Vegetables I**, and **Basic Sentences I**.
- New sentence tasks:
  - `translate_select` — show a Tigrinya sentence, pick the correct EN/NL translation.
  - `order_words` — arrange Tigrinya word tiles to form the sentence.
- Placeholder images (under `Resources/Images/...`) and **audio file hooks** (under `Resources/Raw/content/audio/...`) are ready for studio recordings.
- QA checklists added for each new pack; please route for native review before release.


## New in v15 (Content – Family II & Sentences II)
- **Family II**: picture/label tasks for more relations and a scaffolded translation activity for “This is my …”
- **Basic Sentences II**: negation & word order scaffolds (Tigrinya forms marked “—” for native review).
- All new lines include **audio file hooks** and QA checklists; please run them through the QA Review page.


## New in v16 (Content – Animals II & Household II)
- **Animals II — Wildlife**: elephant, giraffe, zebra, leopard, etc. (Tigrinya spellings marked “—” for QA; audio hooks included).
- **Household II — Kitchen & Bath**: spoon, plate, knife, soap, towel, toothbrush, mirror (with audio hooks).
- Both packs include **picture-word**, **label-match**, and **flashcards**, and have QA checklists under `Resources/Raw/content/qa/`.
