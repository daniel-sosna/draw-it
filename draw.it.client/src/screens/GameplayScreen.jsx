import React, { useState } from "react";
import DrawingCanvas from "../screen_components/DrawingCanvas";
import colors from "@/constants/colors.js"; // import your canvas component
import Button from "@/components/button/button.jsx";

export default function GameplayScreen() {
    const [messages, setMessages] = useState([
        { user: "Laimis", text: "Bananas" },
        { user: "Titas", text: "Lol" },
    ]);
    const [input, setInput] = useState("");

    const sendMessage = () => {
        if (!input.trim()) return;
        setMessages([...messages, { user: "You", text: input }]);
        setInput("");
        // TODO: send message to backend 
    };

    // TODO: move the chat to a different file
    
    return (
        <div style={styles.container}>
            {/* Canvas on the left */}
            <div style={styles.canvasContainer}>
                <DrawingCanvas />
            </div>

            {/* Chat on the right */}
            <div style={styles.chatContainer}>
                <div style={styles.messages}>
                    {messages.map((m, i) => (
                        <p key={i}>
                            <b>{m.user}:</b> {m.text}
                        </p>
                    ))}
                </div>
                <div style={styles.inputRow}>
                    <input
                        type="text"
                        value={input}
                        onChange={(e) => setInput(e.target.value)}
                        onKeyDown={(e) => e.key === "Enter" && sendMessage()}
                        placeholder="Type your guess..."
                        style={styles.input}
                    />
                    <Button onClick={sendMessage}>
                        Send
                    </Button>
                </div>
            </div>
        </div>
    );
}


// === Styles ===
const styles = {
    container: {
        display: "flex",
        position: "absolute",
        top: 0,
        left: 0,
        right: 0,
        bottom: 0,
        background: colors.secondary,
        margin: 0,
        padding: 10,
        boxSizing: "border-box",
    },
    canvasContainer: {
        flex: 3,
        marginRight: "10px",
        height: "100%", // fill space
    },
    chatContainer: {
        flex: 1,
        display: "flex",
        flexDirection: "column",
        border: "1px solid #ccc",
        overflowY: "auto",  // scrollable
        borderRadius: "5px",
        background: "#fff",
        marginTop: "20px",
        padding: "5px",
        height: "85%", // fill space
    },
    messages: {
        flex: 1,
        overflowY: "auto",
        marginBottom: "3px",
        color: "black",
        
    },
    inputRow: {
        display: "flex",
    },
    input: {
        flex: 1,
        flexShrink: 0,
        padding: "5px",
    },
};