import { createContext, useEffect, useState } from "react";
import * as signalR from "@microsoft/signalr";

export const GameplayHubContext = createContext(null);

export function GameplayHubProvider({ children }) {
    const [gameplayConnection, setGameplayConnection] = useState(null);

    useEffect(() => {
        const connection = new signalR.HubConnectionBuilder()
            .withUrl("https://localhost:7200/gameplayHub")
            .configureLogging(signalR.LogLevel.Information)
            .withAutomaticReconnect()
            .build();

        setGameplayConnection(connection);

        async function start() {
            try {
                await connection.start();
                console.log("SignalR Connected to gameplayHub.");
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
        <GameplayHubContext.Provider value={lobbyConnection}>
            {children}
        </GameplayHubContext.Provider>
    );
}