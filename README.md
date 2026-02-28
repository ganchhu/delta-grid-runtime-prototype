# delta-grid-runtime-prototype
# delta-grid Match – Unity
### delta-grid-runtime-prototype

A cleanly architected runtime-generated memory matching game built in Unity. This project demonstrates structured game state management, resume-safe persistence, and event-driven UI design.

---

##   Project Summary

A dynamic card-matching game where players select grid difficulty, preview card positions, and match pairs to win. The system supports safe mid-game resume without corrupt states.

---

##   Technical Highlights

- **Runtime Grid Generation** (2x2 → 6x6)
- **Dynamic Card Instantiation & Layout Scaling**
- **Coroutine-Based Flip & Match Animations**
- **Event-Driven UI Updates (Decoupled Architecture)**
- **JSON Persistence via PlayerPrefs**
- **Safe Resume System (Matched-only restoration)**
- **Clean Restart & Lifecycle Handling**

---

##   Architecture

**GameManager** – Game flow, board generation, match logic, save/restore  
**Card** – Animation & interaction logic  
**ScoreManager** – Tries, matches, accuracy calculation  
**SaveManager** – JSON serialization & resume handling  
**UIManager** – Event-driven UI updates & panel flow


---

##   Engineering Focus

- Separation of concerns
- Coroutine lifecycle control
- Resume-state stability (no partial flip restoration)
- Minimal coupling between gameplay and UI
- Production-ready state management

---

##   Stack

- Unity
- C#
- PlayerPrefs (JSON serialization)
- Coroutine-based animations
- Event-driven architecture

---

This prototype emphasizes stability, clean architecture, and scalable runtime systems suitable for production-level Unity projects.