# Screens

Every screen and modal in the app, and how they connect.

> **Status: in progress.** Tabs, overlays and the Instruction, Survey and Secret modals are
> implemented. The rest of this describes the intended design until later stages finish.

## Screens and overlays

| Class                    | Layer           | Purpose                       |
| ------------------------ | --------------- | ----------------------------- |
| `FreestyleScreen`        | Tab root        | Freestyle pronunciation       |
| `FlashcardsScreen`       | Tab root        | Flashcard deck list           |
| `AdvancedScreen`         | Tab root        | Advanced home                 |
| `ProgressScreen`         | Tab root        | Phoneme progress              |
| `FlashcardSessionScreen` | Screen          | A flashcard session           |
| `NumberGameScreen`       | Screen          | Number game                   |
| `ASATaskListScreen`      | Screen          | ASA task list                 |
| `ASATaskScreen`          | Screen          | One ASA speaking task         |
| `ASAResultsScreen`       | Screen          | ASA feedback for a task       |
| `ASAProfileScreen`       | Screen          | ASA level and account         |
| `ScoreDetailScreen`      | Overlay         | Per-word pronunciation detail |
| `PhonemeScreen`          | Overlay         | Explanation of one phoneme    |
| `InfoScreen`             | Overlay         | About the app                 |

## Modals

| Modal                 | Class                  | Opened from        | Back                       |
| --------------------- | ---------------------- | ------------------ | -------------------------- |
| Instruction           | `InstructionModal`     | Any tab or Phoneme | Closes, not marked as seen |
| Survey                | `SurveyModal`          | Network response   | Closes                     |
| Secret                | `SecretModal`          | Info               | Closes                     |
| Consent               | `ModalScreen.Attach`   | Advanced           | Same as Refuse             |
| Background form       | `ModalScreen.Attach`   | Advanced           | Closes                     |
| Task loading          | `ModalScreen.Attach`   | Task               | Blocked until an error     |
| Profile loading       | `ModalScreen.Attach`   | Task list          | Blocked until an error     |
| Overall feedback      | `ModalScreen.Attach`   | Task list          | Closes                     |
| Results info          | `ModalScreen.Attach`   | Results            | Closes                     |
| Results dimension     | `ModalScreen.Attach`   | Results            | Closes                     |
| Results feedback      | `ModalScreen.Attach`   | Results            | Closes                     |
| Profile settings      | `ModalScreen.Attach`   | Profile            | Closes                     |
| Delete confirmation   | `ModalScreen.Attach`   | Profile            | Closes                     |
| Profile feedback      | `ModalScreen.Attach`   | Profile            | Closes                     |

Back leaves an instruction unmarked, as closing one by navigation does today, so it shows
again on the next visit. Only OK marks it as seen.

## Tabs and general screens

Any tab can be selected from any screen on the `Screen` layer. Overlays and modals cover
the tab bar, so they must be closed first.

```mermaid
flowchart TD
    launch(["App launch"]) --> freestyle["Freestyle"]

    freestyle -- "Detail button" --> detail["Score detail (overlay)"]
    freestyle -. "Every 50th recording, if survey not done" .-> survey["Survey (modal)"]

    flashcards["Flashcards"] -- "Deck tap or long press" --> session["Flashcard session"]
    session -- "Back or queue empty" --> flashcards
    session -- "Result text" --> detail
    session -. "Every 200th record, if survey not done" .-> survey

    detail -- "Explain button" --> phoneme["Phoneme (overlay)"]
    progress["Progress"] -- "Phoneme row" --> phoneme
    progress -- "Info button" --> info["Info (overlay)"]
    info -. "Secret button, off in the scene" .-> secret["Secret (modal)"]

    advanced["Advanced"] -- "Number game button" --> numbergame["Number game"]
    numbergame -- "Back" --> advanced
    advanced -- "ASA button" --> asa[["ASA flow, see below"]]
```

Back from an overlay or modal returns to whatever opened it, so those arrows are omitted.

## ASA flow

```mermaid
flowchart TD
    advanced["Advanced"] -- "ASA button" --> gate{"Onboarding state"}
    gate -- "No consent" --> consent["Consent (modal)"]
    gate -- "Consent, no form" --> form["Background form (modal)"]
    gate -- "Both complete" --> list["Task list"]

    consent -- "Accept" --> form
    consent -- "Refuse, dim tap or back" --> advanced
    form -- "Server confirms user" --> list
    form -- "Back" --> advanced

    list -- "Back" --> advanced
    list -- "Task 1 to 5" --> task["Task"]
    list -. "Automatic after 10 tasks" .-> overall["Overall feedback (modal)"]
    list -- "Profile button" --> profileloading["Profile loading (modal)"]

    profileloading -- "Request succeeds" --> profile["Profile"]
    profileloading -- "Back after failure" --> list
    profile -- "Back" --> list
    profile -- "Dismiss after account deletion" --> advanced

    task -- "Back, when not recording" --> list
    task -- "Send" --> loading["Task loading (modal)"]
    loading -- "Back after failure" --> task
    loading -- "Results button" --> results["Results"]
    results -- "New Task or back" --> list
```

## Navigator calls

| Situation                  | Call                                                      |
| -------------------------- | --------------------------------------------------------- |
| Tab button                 | `SelectTab(tabRoot)`                                      |
| Open a deck                | `Push<FlashcardSessionScreen>()`                          |
| Session queue empty        | `Back()`                                                  |
| Open a task                | `Push<ASATaskScreen>().DisplayTask(task)`                 |
| Open results               | `Push<ASAResultsScreen>()`                                |
| New Task                   | `PopTo<ASATaskListScreen>()`                              |
| Back on results            | `HandleBack` calls `PopTo<ASATaskListScreen>()`           |
| Profile request succeeds   | `Push<ASAProfileScreen>()`, then fill it with the stats   |
| Account deleted            | `PopToRoot()`                                             |
| Survey due                 | `Open<SurveyModal>()`                                     |
