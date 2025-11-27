import React, { useState, useEffect, useContext } from "react";
import {GameplayHubContext} from "@/utils/GameplayHubProvider.jsx";


export default function TimerComponent() {
    const gameplayConnection = useContext(GameplayHubContext);
    const [secondsLeft, setSecondsLeft] = useState(0);
    const [timer, setTimer] = useState(null);
    const [serverOffset, setServerOffset] = useState(null);

    useEffect(() => {
        if (!gameplayConnection) return;

        gameplayConnection.on("ReceiveTimer", (timerString) => {
            const serverTime = new Date(timerString).getTime();
            const clientTime = Date.now();
            
            const offset = serverTime - clientTime;
            setServerOffset(offset);
            
            const timer = new Date(timerString);
            setTimer(timer);
        });
    
        return () => {
            gameplayConnection.off("ReceiveTimer");
        }
    }, [gameplayConnection]);
    
    useEffect(() => {
        if (!timer) return;
        const updateTimer = () => {
            const clientNow = Date.now();

            // Add the offset to get the "Real" Server Time
            const estimatedServerTime = clientNow + serverOffset;

            const targetTimeMs = timer.getTime();

            // Calculate difference
            const diffMs = targetTimeMs - estimatedServerTime;
            const diffSecs = Math.max(0, Math.floor(diffMs / 1000));

            setSecondsLeft(diffSecs);
        };
        updateTimer();

        // Start a 1-second countdown
        const interval = setInterval(updateTimer, 1000);

        return () => clearInterval(interval);
    }, [timer, serverOffset]);
    
    return (
        <div className="absolute top-4 right-6 bg-black z-10 px-4 py-2 rounded-lg shadow-md text-xl font-semibold text-white">
            {secondsLeft}
        </div>
    );  
}

