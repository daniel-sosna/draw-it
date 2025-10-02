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

First of all, check the [📚 Coding Conventions](https://github.com/daniel-sosna/draw-it/discussions/categories/coding-conventions) category in the Discussions section to follow best practices. Then:

1. **Create a new branch**
1. **Make some changes**
1. **Check your code locally with linters**:
    - For frontend (from `draw.it.client` directory):
      ```bash
      npm run lint
      npm run lint:fix
      ```  
    - For backend (from `Draw.it.Server` directory):
      ```bash
      dotnet format --severity info --verify-no-changes
      dotnet format --severity info
      ```
      or just: *(is enough for CI linting)*
      ```bash
      dotnet format --verify-no-changes
      dotnet format
      ```
1. **Commit changes and push to the remote**
1. **Create a Pull Request**
    - On the GitHub website, create a new PR for your branch.
    - Wait until all CI tests are passed.
    - Request feedback from the team and collaborate to improve your changes.
1. **Merge the PR into main.**
    - When you are ready to merge, ensure that the PR is up to date with the `main` branch and there are no conflicts.
        - Otherwise, follow the [guide](#update-your-pr-and-resolve-conflicts) below to update your PR.
        - Once again, wait until all CI tests are passed and your PR gets approved.
    - Press *"Merge pull request"* to merge all your commits into the `main` branch.

### Update your PR and resolve conflicts
1. **Update your local repository and merge the main**
    - Fetch the latest changes:
      ```bash
      git fetch origin
      ```
    - Merge the `main` branch into your PR branch:
      ```bash
      git switch <your-pr-branch-name>
      git merge origin/main
      ```
    - *If there are merge conflicts, proceed to the next step
1. **Resolve any merge conflicts**
    - Open the conflicting files and make the necessary changes *(the easiest way is to use your IDE's built-in tools)*.
    - Run linter tests locally to ensure that your PR will pass CI tests.
    - Commit the changes.
1. **Push the updated PR branch to the remote**

## 👥 Team

Team **null** – Vilnius University, Software Engineering course project.
