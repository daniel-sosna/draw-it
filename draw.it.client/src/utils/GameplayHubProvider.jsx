import { createHubProvider } from "./HubFactory.jsx";
import serverBaseUrl from '@/constants/urls.js';

const { HubContext: GameplayHubContext, HubProvider: GameplayHubProvider } = createHubProvider(`${serverBaseUrl}/gameplayHub`);

export { GameplayHubContext, GameplayHubProvider };