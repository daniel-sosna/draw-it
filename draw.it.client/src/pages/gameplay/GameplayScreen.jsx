import {useContext, useEffect, useState} from "react";
import DrawingCanvas from "@/components/gameplay/DrawingCanvas";
import ChatComponent from "@/components/gameplay/ChatComponent.jsx";
import { GameplayHubContext } from "@/utils/GameplayHubProvider.jsx";
import {useParams} from "react-router";

export default function GameplayScreen() {
    
    const gameplayConnection = useContext(GameplayHubContext);
    const { roomId } = useParams();
    
    const [messages, setMessages] = useState([]);

    useEffect(() => {
        if(!gameplayConnection) {
            console.log("Gameplay connection not established yet");
            return;
        }
        
        gameplayConnection.on("ReceiveMessage", (userName, message, isCorrectGuess) => {
            setMessages((prevMessages) => [...prevMessages, { user: userName, message: message, isCorrect: isCorrectGuess }]);
        })

        console.log("Gameplay connection established:", gameplayConnection);

    }, [gameplayConnection, roomId]);
    
    
    const handleSendMessage = async (message) => {
        console.log("Sending message:", message);
        try {
            await gameplayConnection.invoke("SendMessage", message);
            setMessages((prevMessages) => [...prevMessages, { user: "You", message: message, isCorrect: false }]);
        } catch (error) {
            console.log(error);
            console.log("Could not send message:", error);
        }        
    };
    
    return (
        // FIX 1: Use w-screen h-screen and overflow-hidden to contain the layout.
        <div className="flex w-screen h-[90vh] bg-secondary p-4 overflow-hidden">

            {/* Canvas Wrapper: w-3/4 and h-full remains correct */}
            <div className="w-3/4 h-full bg-gray-100 p-6 rounded-xl shadow-lg flex flex-col mr-4">
                <DrawingCanvas />
            </div>

            {/* FIX 2: Explicitly wrap ChatComponent to control its w-1/4 and h-full layout */}
            <div className="w-1/4 h-[90vh]">
                <ChatComponent
                    messages={messages}
                    onSendMessage={handleSendMessage}
                    // Pass classes to ChatComponent to handle its internal styling (like h-full)
                    className="h-full bg-gray-800 rounded-xl shadow-lg"
                />
            </div>
        </div>
    );
}