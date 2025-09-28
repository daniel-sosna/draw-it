import React, { useState } from "react";
// Assuming Button component path is correct
import Button from "@/components/button/button.jsx";

export default function ChatComponent({ messages, onSendMessage }) {
    const [input, setInput] = useState("");

    const sendMessage = () => {
        if (!input.trim()) return;
        onSendMessage(input); // Call the function passed from the parent
        setInput("");
    };

    return (
        <div className="flex-1 flex flex-col border border-gray-300 rounded bg-white mt-5 p-1.5 h-[85%]">

            <div className="flex-1 overflow-y-auto mb-1 text-black">
                {messages.map((m, i) => (
                    <p key={i} className="text-sm p-0.5">
                        <b className="font-semibold">{m.user}:</b> {m.text}
                    </p>
                ))}
            </div>

            {/* Input Row - simple flex container */}
            <div className="flex">
                <input
                    type="text"
                    value={input}
                    onChange={(e) => setInput(e.target.value)}
                    onKeyDown={(e) => e.key === "Enter" && sendMessage()}
                    placeholder="Type your guess..."
                    // Input element takes up space (flex-1) and has padding
                    className="flex-1 flex-shrink-0 p-1.5 border border-gray-400 rounded-l focus:outline-none focus:ring-2 focus:ring-blue-500"
                />

                <Button
                    onClick={sendMessage}
                    // Added a default style to the button to visually match the input row
                    className="px-3 py-1.5 bg-blue-500 text-white font-medium rounded-r hover:bg-blue-600 transition-colors"
                >
                    Send
                </Button>
            </div>
        </div>
    );
}
