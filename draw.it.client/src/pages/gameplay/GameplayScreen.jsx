import { useContext, useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router";
import DrawingCanvas from "@/components/gameplay/DrawingCanvas";
import ChatComponent from "@/components/gameplay/ChatComponent.jsx";
import { GameplayHubContext } from "@/utils/GameplayHubProvider.jsx";
import ScoreModal from "@/components/modal/ScoreModal.jsx";

export default function GameplayScreen() {
    
    const gameplayConnection = useContext(GameplayHubContext);
    const { roomId } = useParams();
    const [messages, setMessages] = useState([]);
    const [scoreModalOpen, setScoreModalOpen] = useState(false);
    const [scoreModalTitle, setScoreModalTitle] = useState("");
    const [scoreModalScores, setScoreModalScores] = useState([]);
    const [timer, setTimer] = useState(0);

    useEffect(() => {
        if(!gameplayConnection) return;
        
        gameplayConnection.on("ReceiveMessage", (userName, message, isCorrectGuess) => {
            setMessages((prevMessages) => [...prevMessages, { user: userName, message: message, isCorrect: isCorrectGuess ?? false }]);
        })

        gameplayConnection.on("ReceiveTurnStarted", () => {
            setScoreModalOpen(false);
            setMessages([]);
        });

        gameplayConnection.on("ReceiveRoundStarted", () => {
        });

        gameplayConnection.on("ReceiveRoundEnded", (scores) => {
            setScoreModalTitle("Round Results");
            setScoreModalScores(scores || []);
            setScoreModalOpen(true);
        });

        gameplayConnection.on("ReceiveGameEnded", (scores) => {
            setScoreModalTitle("Final Scores");
            setScoreModalScores(scores || []);
            setScoreModalOpen(true);
        });

        
        return () => {
            gameplayConnection.off("ReceiveMessage");
            gameplayConnection.off("ReceiveTurnStarted");
            gameplayConnection.off("ReceiveRoundStarted");
            gameplayConnection.off("ReceiveRoundEnded");
            gameplayConnection.off("ReceiveGameEnded");
        };
    }, [gameplayConnection, roomId]);
    
    
    const handleSendMessage = async (message) => {
        try {
            await gameplayConnection.invoke("SendMessage", message);
        } catch (error) {
            console.log("Could not send message:", error);
            alert("Error sending message. Please try again.");
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

            <ScoreModal
                isOpen={scoreModalOpen}
                onClose={() => setScoreModalOpen(false)}
                scores={scoreModalScores}
                title={scoreModalTitle}
            />
        </div>
    );
}