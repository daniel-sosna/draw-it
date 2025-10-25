import { createHubProvider } from "./HubFactory.jsx";
import serverBaseUrl from '@/constants/urls.js';

const { HubContext: LobbyHubContext, HubProvider: LobbyHubProvider } = createHubProvider(`${serverBaseUrl}/lobbyHub`);

export { LobbyHubContext, LobbyHubProvider };