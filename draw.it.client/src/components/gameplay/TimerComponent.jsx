import React, { useState, useEffect, useContext } from "react";
import {GameplayHubContext} from "@/utils/GameplayHubProvider.jsx";


export default function TimerComponent() {
    const gameplayConnection = useContext(GameplayHubContext);
    const [secondsLeft, setSecondsLeft] = useState(0);
    const [initialDurationMs, setInitialDurationMs] = useState(null);
    const [serverOffset, setServerOffset] = useState(null);
    const hasCalledEndRef = useRef(false);
    useEffect(() => {
        if (!gameplayConnection) return;
        
        gameplayConnection.on("ReceiveTimer", (serverTimeString, durationSeconds) => {

            hasCalledEndRef.current = false;
            
            // Calculate and set offset of the servers time and clients
            const serverTime = new Date(serverTimeString).getTime();
            const clientTime = Date.now();
            const offset = serverTime - clientTime; // Offset in ms
            setServerOffset(offset);
            
            const durationMs = durationSeconds * 1000;

            // Server times deadline
            const deadlineMs = serverTime + durationMs;

            setInitialDurationMs(deadlineMs);
        });
        
        return () => {
            gameplayConnection.off("ReceiveTimer");
        }
    }, [gameplayConnection]);

    useEffect(() => {
        if (!initialDurationMs || serverOffset === null) return;

        const updateTimer = () => {
            const clientNow = Date.now();

            // Apply the offset to get the Server Time
            const estimatedServerTime = clientNow + serverOffset;

            // Calculate remaining time against the server-anchored deadline
            const diffMs = initialDurationMs - estimatedServerTime;
            const diffSecs = Math.max(0, Math.floor(diffMs / 1000));

            setSecondsLeft(diffSecs);
            
            if (diffSecs <= 0) {
                clearInterval(interval);
                setSecondsLeft(0);  
                if (!hasCalledEndRef.current){
                    gameplayConnection.invoke("TimerEnded");
                    hasCalledEndRef.current = true;
                } 
            }
        };

        updateTimer();

        // Start a 1-second countdown
        const interval = setInterval(updateTimer, 1000);

        return () => {
            clearInterval(interval);
        } 

    }, [initialDurationMs, serverOffset, gameplayConnection]);

    return (
        <div className="absolute top-4 right-6 bg-black z-10 px-4 py-2 rounded-lg shadow-md text-xl font-semibold text-white">
            {secondsLeft}
        </div>
    );
}