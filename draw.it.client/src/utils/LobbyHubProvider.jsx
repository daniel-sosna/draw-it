import { createHubProvider } from "./HubFactory.jsx";

const { HubContext: LobbyHubContext, HubProvider: LobbyHubProvider } = createHubProvider("https://localhost:7200/lobbyHub");

export { LobbyHubContext, LobbyHubProvider };