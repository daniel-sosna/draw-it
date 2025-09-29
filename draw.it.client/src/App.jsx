import './App.css';
import Index from "@/pages/index/Index.jsx";
import RoomPage from "@/pages/room_page/RoomPage.jsx";
import {BrowserRouter, Route, Routes} from "react-router";

function App() {
    return (
        <BrowserRouter>
            <Routes>
                <Route path="/" element={<Index />}/>
                <Route path="/room/:roomId" element={<RoomPage />} />
            </Routes>
        </BrowserRouter>
    )
}

export default App;