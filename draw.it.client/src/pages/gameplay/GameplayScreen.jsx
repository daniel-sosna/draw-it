import {useContext, useEffect, useState} from "react";
import DrawingCanvas from "@/components/gameplay/DrawingCanvas";
import ChatComponent from "@/components/gameplay/ChatComponent.jsx";
import PlayerList from "@/components/gameplay/PlayerList.jsx";
import { GameplayHubContext } from "@/utils/GameplayHubProvider.jsx";
import { LobbyHubContext } from "@/utils/LobbyHubProvider.jsx";
import {useParams} from "react-router";

export default function GameplayScreen() {
    
    const gameplayConnection = useContext(GameplayHubContext);
    const lobbyConnection = useContext(LobbyHubContext);
    const { roomId } = useParams();
    
    const [messages, setMessages] = useState([]);
    const [players, setPlayers] = useState([]);

    useEffect(() => {
        if(!gameplayConnection) {
            console.log("Gameplay connection not established yet");
            return;
        }
        
        gameplayConnection.on("ReceiveMessage", (userName, message) => {
            setMessages((prevMessages) => [...prevMessages, { user: userName, message: message }]);
        })

        console.log("Gameplay connection established:", gameplayConnection);

    }, [gameplayConnection, roomId]);

    // Subscribe to lobby player list updates and fetch initial list
    useEffect(() => {
        if (!lobbyConnection) return;

        const onReceivePlayers = (players) => setPlayers(players);
        lobbyConnection.on("ReceivePlayerList", onReceivePlayers);

        // Ask server to send current list for this room
        try {
            lobbyConnection.invoke("SendPlayerListUpdate", roomId);
        } catch (err) {
            console.warn("Failed to request player list:", err);
        }

        return () => {
            lobbyConnection.off("ReceivePlayerList", onReceivePlayers);
        };
    }, [lobbyConnection, roomId]);
    
    
    const handleSendMessage = async (message) => {
        console.log("Sending message:", message);
        try {
            await gameplayConnection.invoke("SendMessage", message);
            setMessages((prevMessages) => [...prevMessages, { user: "You", message: message }]);
        } catch (error) {
            console.log(error);
            console.log("Could not send message:", error);
        }        
    };
    
    return (
        // FIX 1: Use w-screen h-screen and overflow-hidden to contain the layout.
        <div className="flex w-screen h-[90vh] bg-secondary p-4 overflow-hidden">

            {/* Canvas Wrapper: w-3/4 and h-full remains correct */}
            <div className="w-3/4 h-full bg-white p-6 rounded-xl shadow-lg flex flex-col mr-4">
                <DrawingCanvas />
            </div>

            {/* Sidebar: players (top) + chat (bottom) */}
            <div className="w-1/4 h-[90vh] flex flex-col">
                <div className="flex-none h-[30vh] mb-4">
                    <PlayerList players={players} />
                </div>
                <div className="flex-1 min-h-0">
                    <ChatComponent
                        messages={messages}
                        onSendMessage={handleSendMessage}
                        className="h-full"
                    />
                </div>
            </div>
        </div>
    );
}