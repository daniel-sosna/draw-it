import React, { useState } from "react";
import styles from "@/components/gameplay/ChatComponent.module.css";
export default function ChatComponent({ messages, onSendMessage, className = "" }) {
    const [input, setInput] = useState("");

    const sendMessage = () => {
        if (!input.trim()) return;
        onSendMessage(input); // Call the function passed from the parent
        setInput("");
    };

    return (
        <div className={`flex-1 flex flex-col border border-gray-300 rounded bg-white p-1.5 ${className}`}>

            <div className="flex-1 overflow-y-auto mb-1 text-black">
                {messages.map((m, i) => (
                    <p key={i} className="text-sm p-0.5">
                        <b className="font-semibold">{m.user}:</b> {m.message}
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
                    className={styles.inputField}
                />

                <button
                    onClick={sendMessage}
                    className={styles.sendButton}
                >
                    Send
                </button>
            </div>
        </div>
    );
}
