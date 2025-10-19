import { createContext, useEffect, useState } from "react";
import * as signalR from "@microsoft/signalr";

export const LobbyHubContext = createContext(null);

export function LobbyHubProvider({ children }) {
    const [lobbyConnection, setLobbyConnection] = useState(null);

    useEffect(() => {
        const connection = new signalR.HubConnectionBuilder()
            .withUrl("https://localhost:7200/lobbyHub")
            .configureLogging(signalR.LogLevel.Information)
            .withAutomaticReconnect()
            .build();

        setLobbyConnection(connection);

        async function start() {
            try {
                await connection.start();
                console.log("SignalR Connected.");
            } catch (err) {
                console.log(err);
            }
        };

        connection.onreconnected(connectionId => {
            console.log("Reconnected successfully!");
        });

        start();

        return () => {
            connection.stop(); // Cleanup on unmount
        };
    }, []); // Ensures it runs once

    return (
        <LobbyHubContext.Provider value={lobbyConnection}>
            {children}
        </LobbyHubContext.Provider>
    );
}