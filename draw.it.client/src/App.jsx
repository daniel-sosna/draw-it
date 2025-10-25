import "./App.css";
import Index from "@/pages/index/Index.jsx";
import RoomPage from "@/pages/room/RoomPage.jsx";
import GameplayScreen from "@/pages/gameplay/GameplayScreen.jsx";
import HostScreen from "@/pages/host/HostScreen.jsx";
import { BrowserRouter, Routes, Route, Outlet } from "react-router";
import { LobbyHubProvider } from "@/utils/LobbyHubProvider.jsx";
import { GameplayHubProvider } from "@/utils/GameplayHubProvider.jsx";


function LobbyLayout() {
    return (
        <LobbyHubProvider>
            <Outlet /> {/* nested routes render here */}
        </LobbyHubProvider>
    );
}

function GameplayLayout() {
    return (
        <GameplayHubProvider>
            <Outlet />
        </GameplayHubProvider>
    );
}

function App() {
    return (
        <BrowserRouter>
            <Routes>
                <Route path="/" element={<Index />} />
                <Route element={<LobbyLayout />}>
                    <Route path="/room/:roomId" element={<RoomPage />} />
                    <Route path="/host/:roomId" element={<HostScreen />} />
                    <Route element={<GameplayLayout />}>
                        <Route path="/gameplay/:roomId" element={<GameplayScreen />} />
                    </Route>
                </Route>
            </Routes>
        </BrowserRouter>
    )
}

export default App;
