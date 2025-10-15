import { useEffect, useState } from "react";
import * as signalR from "@microsoft/signalr";

let connection = null;

// This class is for connecting to the lobby hub
// We get less code duplication  this way
// However it is not implemented yet / doesn't work, don't if it is even worth it
export function getLobbyConnection() {
    if (!connection) {
        connection = new signalR.HubConnectionBuilder()
            .withUrl("https://localhost:7200/lobbyHub")
            .configureLogging(signalR.LogLevel.Information)
            .withAutomaticReconnect()
            .build();
    }
    return connection;
}

export async function startLobbyConnection(roomId) {
    const conn = getLobbyConnection();

    if (conn.state === signalR.HubConnectionState.Disconnected) {
        await conn.start();
        console.log("SignalR Connected.");
        console.log(`Room id: ${roomId}`);
        await conn.invoke("JoinRoomGroup", roomId);
    }

    return conn;
}