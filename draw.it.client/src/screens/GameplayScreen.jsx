import React, { useState } from "react";
import DrawingCanvas from "../screen_components/DrawingCanvas"; // import your canvas component

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
        // TODO: send message to backend via SignalR
    };

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
                    <button onClick={sendMessage} style={styles.button}>
                        Send
                    </button>
                </div>
            </div>
        </div>
    );
}

// === Styles ===
const styles = {
    container: {
        display: "flex",
        height: "65vh",
        background: "#f0f0f0",
        padding: "10px",
        boxSizing: "border-box",
    },
    canvasContainer: {
        flex: 3,
        marginRight: "10px",
    },
    chatContainer: {
        flex: 1,
        display: "flex",
        flexDirection: "column",
        border: "1px solid #ccc",
        borderRadius: "5px",
        background: "#fff",
        padding: "5px",
        height: "100%",
    },
    messages: {
        flex: 1,
        overflowY: "auto",
        marginBottom: "5px",
    },
    inputRow: {
        display: "flex",
        padding: "5px"
    },
    input: {
        flex: 1,
        padding: "5px",
    },
    button: {
        marginLeft: "5px",
        padding: "5px 10px",
    },
};
