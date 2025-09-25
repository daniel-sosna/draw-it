# 🎨 Draw\.it

**Draw\.it** is a real-time multiplayer drawing and guessing game built with **C# (.NET)** on the backend and **React** on the frontend.
Players can create or join rooms, take turns drawing a word they’re assigned, and compete by guessing what others are drawing.
It’s designed as a fun entertainment platform for groups of friends, students, or colleagues.

## 🚀 MVP Plan

We are developing **Draw\.it** in three main stages:

### 🥚 Alpha (Initial Prototype)

- [ ] **Word list** – basic pool of words for the game  
- [ ] **Room creation & joining** – players can create rooms and invite others  
- [ ] **Chat system** – real-time messaging between players  
- [ ] **Drawing board** – canvas where players can draw  

### 🐣 Beta (Core Gameplay)

- [ ] **Turn-based drawing** – each player gets a turn to draw while others guess  
- [ ] **Round results** – points awarded after each round  
- [ ] **Game results** – scoreboard with overall winner  
- [ ] **Categories** – words grouped by theme (e.g., animals, objects, actions)  

### 🐥 Final (Full Release)

- [ ] **Customizations** – player avatars, brush settings, colors, themes  
- [ ] **Voting system** – players can vote for the best drawing  
- [ ] **Extra features** – improved UI/UX, animations, sound effects, more categories  

## 🛠️ Development Workflow

1. Create a new branch
1. Make some changes
1. Check your code with linters:  
    1. For frontend:
        ```bash
        npm run lint
        npm run lint:fix
        ```  
   1. For backend (from `Draw.it.Server` directory):
       ```bash
       dotnet format --verify-no-changes --severity info
       dotnet format --severity info
       ```
      or just: (is enough for CI lint)
       ```bash
       dotnet format --verify-no-changes
       dotnet format
       ```
1. Commit changes and push to the remote
1. Open a Pull Request
1. Wait until all CI tests are passed and your PR get approved. Only after that you will be able to merge into `main`.

## 👥 Team

Team **null** – Vilnius University, Software Engineering course project.
