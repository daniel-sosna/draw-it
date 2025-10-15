import { useEffect, useState } from "react";
import * as signalR from "@microsoft/signalr";

let connection = null;

// This class is for connecting to the lobby hub
// We get less code duplication  this way
// However it is not implemented yet/doesnt work
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

export async function startLobbyConnection(userId, roomId) {
    const conn = getLobbyConnection();

    if (conn.state === signalR.HubConnectionState.Disconnected) {
        await conn.start();
        console.log("SignalR Connected.");
        console.log(`Room id: ${roomId}`);
        await conn.invoke("JoinRoomGroup", userId, roomId);
    }

    return conn;
}