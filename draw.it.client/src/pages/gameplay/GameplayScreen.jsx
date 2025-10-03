import React, { useState, useState } from "react";
import DrawingCanvas from "@/components/gameplay/DrawingCanvas";
import ChatComponent from "@/components/gameplay/ChatComponent.jsx";
import * as signalR from "@microsoft/signalr";

export default function GameplayScreen() {
    const [messages, setMessages] = useState([
        { user: "Laimis", text: "Bananas" },
        { user: "Titas", text: "Lol" },
    ]);

    const handleSendMessage = (text) => {
        setMessages((prevMessages) => [...prevMessages, { user: "You", text }]);
        // TODO: send message to backend 
    };
    return (
        // FIX 1: Use w-screen h-screen and overflow-hidden to contain the layout.
        <div className="flex w-screen h-[90vh] bg-secondary p-4 overflow-hidden">

            {/* Canvas Wrapper: w-3/4 and h-full remains correct */}
            <div className="w-3/4 h-full bg-white p-6 rounded-xl shadow-lg flex flex-col mr-4">
                <DrawingCanvas />
            </div>

            {/* FIX 2: Explicitly wrap ChatComponent to control its w-1/4 and h-full layout */}
            <div className="w-1/4 h-[90vh]">
                <ChatComponent
                    messages={messages}
                    onSendMessage={handleSendMessage}
                    // Pass classes to ChatComponent to handle its internal styling (like h-full)
                    className="h-full bg-gray-800 rounded-xl shadow-lg"
                />
            </div>
        </div>
    );
}