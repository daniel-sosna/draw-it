import React, { useState, useEffect, useContext } from "react";
import {GameplayHubContext} from "@/utils/GameplayHubProvider.jsx";


function TimerComponent() {
    const gameplayConnection = useContext(GameplayHubContext);
    const [timer, setTimer] = useState(0);

    useEffect(() => {
        if (!gameplayConnection) return;

        gameplayConnection.on("ReceiveTimer", (timer) => {
            const date = new Date(timer);
            
        });
    
        return () => {
            gameplayConnection.off("ReceiveTimer");
        }
    }, [gameplayConnection]);
    
    const han
    return (
        <div className="absolute top-4 right-6 bg-white px-4 py-2 rounded-lg shadow-md text-xl font-semibold">
            {timer}
        </div>
    );  
}

