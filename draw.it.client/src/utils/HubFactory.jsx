import { createContext, useEffect, useState } from "react";
import { useNavigate } from 'react-router';
import * as signalR from "@microsoft/signalr";

// Factory that returns a Context and Provider for a given hub URL.
// Keeps the SignalR setup in one place so multiple hub providers can share behavior.
export function createHubProvider(hubUrl) {
    const HubContext = createContext(null);

    function HubProvider({ children }) {
        const [connection, setConnection] = useState(null);
        const navigate = useNavigate();

        // Helper to stop a SignalR connection safely. Returns a Promise that
        // resolves when the stop completed or immediately if there's nothing to stop.
        function safeStop(conn) {
            if (!conn) return Promise.resolve();
            try {
                if (conn.state !== signalR.HubConnectionState.Disconnected) {
                    return conn.stop().catch(err => console.error("Error stopping connection:", err));
                }
            } catch (err) {
                console.error("Failed to stop connection:", err);
            }
            return Promise.resolve();
        }

        useEffect(() => {
            const connection = new signalR.HubConnectionBuilder()
                .withUrl(hubUrl)
                .configureLogging(signalR.LogLevel.Information)
                .withAutomaticReconnect()
                .build();

            setConnection(connection);

            async function start() {
                try {
                    await connection.start();
                    console.log(`SignalR Connected to ${hubUrl}.`);
                } catch (err) {
                    console.log(err);
                }
            }

            connection.onreconnected((connectionId) => {
                console.log("Reconnected successfully!");
            });

            connection.on("ReceiveConnectionAborted", (message) => {
                console.error("Connection aborted by server:", message);
                safeStop(connection);
                alert(`Connection aborted: ${message}`);
                navigate("/");
            });

            start();

            return () => {
                safeStop(connection); // Cleanup on unmount
            };
        }, []); // Ensure it runs once

        return (
            <HubContext.Provider value={connection}>
                {children}
            </HubContext.Provider>
        );
    }

    return { HubContext, HubProvider };
}
