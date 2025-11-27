import React, { useState, useEffect, useContext } from "react";
import {GameplayHubContext} from "@/utils/GameplayHubProvider.jsx";


function TimerComponent() {
    const gameplayConnection = useContext(GameplayHubContext);
    const [secondsLeft, setSecondsLeft] = useState(0);
    const [timer, setTimer] = useState(null);

    useEffect(() => {
        if (!gameplayConnection) return;

        gameplayConnection.on("ReceiveTimer", (timerString) => {
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
            const now = new Date();
            const diffSecs = Math.max(0, Math.floor((timer - now) / 1000));
            setSecondsLeft(diffSecs);
        };
        updateTimer();

        // Start a 1-second countdown
        const interval = setInterval(updateTimer, 1000);

        return () => clearInterval(interval);
    }, [timer]);
    
    return (
        <div className="absolute top-4 right-6 bg-white px-4 py-2 rounded-lg shadow-md text-xl font-semibold">
            {secondsLeft}
        </div>
    );  
}

