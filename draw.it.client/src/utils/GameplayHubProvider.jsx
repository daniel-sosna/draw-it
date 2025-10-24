import { createHubProvider } from "./HubFactory.jsx";

const { HubContext: GameplayHubContext, HubProvider: GameplayHubProvider } = createHubProvider("https://localhost:7200/gameplayHub");

export { GameplayHubContext, GameplayHubProvider };