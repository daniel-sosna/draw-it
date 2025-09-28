import { useEffect, useState } from 'react';
import './App.css';
import Index from "@/pages/index/Index.jsx";
import {BrowserRouter, Route, Routes} from "react-router";

function App() {
    return (
        <BrowserRouter>
            <Routes>
                <Route path="/" element={<Index />}/>
            </Routes>
        </BrowserRouter>
    )
}

export default App;