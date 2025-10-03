import { useEffect, useState } from "react";
import * as signalR from "@microsoft/signalr";

export function useGameHub(room, user) {
    const [messages, setMessages] = useState([]);
    const [connection, setConnection] = useState(null);

    useEffect(() => {
        const conn = new signalR.HubConnectionBuilder()
            .withUrl("https://localhost:61528/gameHub")
            .withAutomaticReconnect()
            .build();

        conn.start()
            .then(() => conn.invoke("JoinRoom", room, user))
            .catch(err => console.error("SignalR Connection Error: ", err));

        conn.on("ReceiveMessage", (sender, message) => {
            setMessages(prev => [...prev, { user: sender, text: message }]);
        });

        conn.on("UserJoined", (_, __, newUser) => {
            setMessages(prev => [...prev, { user: "System", text: `${newUser} joined the room` }]);
        });

        conn.on("UserLeft", (_, __, leftUser) => {
            setMessages(prev => [...prev, { user: "System", text: `${leftUser} left the room` }]);
        });

        setConnection(conn);

        return () => {
            if (conn) {
                conn.invoke("LeaveRoom", room, user).catch(() => {});
                conn.stop();
            }
        };
    }, [room, user]);

    const sendMessage = (message) => {
        if (connection && connection.state === signalR.HubConnectionState.Connected) {
            connection.invoke("SendMessage", room, user, message)
                .catch(err => console.error("SendMessage Error: ", err));
        }
    };

    return { messages, sendMessage };
}