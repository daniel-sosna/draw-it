import React from "react";
import ReactDOM from "react-dom/client";
import { BrowserRouter, Routes, Route } from "react-router-dom";
import App from "./App.jsx";                 // šablono pradinis puslapis (neliečiam)
import RoomPage from "./pages/RoomPage.jsx"; // mūsų naujas papildomas puslapis

ReactDOM.createRoot(document.getElementById("root")).render(
  <React.StrictMode>
    <BrowserRouter>
      <Routes>
        {/* PAGRINDINIS ŠABLONO PUSLAPIS – paliekam kaip buvo */}
        <Route path="/" element={<App />} />

        {/* PAPILDOMAS PUSLAPIS – TIK PER URL */}
        <Route path="/room/:roomId" element={<RoomPage />} />
      </Routes>
    </BrowserRouter>
  </React.StrictMode>
);
