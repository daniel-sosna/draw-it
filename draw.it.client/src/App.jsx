import './App.css';
import Index from "@/pages/index/Index.jsx";
import HostScreen from "@/pages/Host/HostScreen.jsx";
import {BrowserRouter, Route, Routes} from "react-router";

function App() {
    return (
        <BrowserRouter>
            <Routes>
                <Route path="/" element={<Index />} />
                <Route path="/host/:roomId" element={<HostScreen />} />
            </Routes>
        </BrowserRouter>
    )
}

export default App;