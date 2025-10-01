import './App.css';
import Index from "@/pages/index/Index.jsx";
import GameplayScreen from "@/pages/gameplay/GameplayScreen.jsx";
import HostScreen from "@/pages/Host/HostScreen.jsx";
import {BrowserRouter, Route, Routes} from "react-router";

function App() {
    return (
        <BrowserRouter>
            <Routes>
                <Route path="/" element={<Index />} />
                <Route path="/host/:roomId" element={<HostScreen />} />
                <Route path="/gameplay" element={<GameplayScreen />}/>
            </Routes>
        </BrowserRouter>
    )
}

export default App;