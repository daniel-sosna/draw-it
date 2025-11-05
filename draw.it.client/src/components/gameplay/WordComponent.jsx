import React, { useState, useEffect, useContext } from "react";
import {GameplayHubContext} from "@/utils/GameplayHubProvider.jsx";

export default function WordComponent() {

    const [word, setWord] = useState("");
    const gameplayConnection = useContext(GameplayHubContext);

    
    useEffect(() => {
        if (!gameplayConnection) return;

        gameplayConnection.on("ReceiveWordToDraw", (word) => {
            setWord(word);
        });
        return () => {
            gameplayConnection.off("ReceiveWordToDraw");
        };
    }, [gameplayConnection]);
    
    
    return (
        //{/* Word to draw/guess */}
        <div className="flex items-center justify-center mb-2">
            <div className="text-center">
                <div className="text-2xl font-bold text-gray-800 bg-white px-4 py-1 rounded-lg border-2 border-orange-200 shadow-sm">
                    {word || "Hidden Word"}
                </div>
            </div>
        </div>
    );
};

