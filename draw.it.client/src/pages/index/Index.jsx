import "./Index.css"
import { useState } from "react";
import { useNavigate } from "react-router";
import api from "@/utils/api.js";
import colors from "@/constants/colors.js";
import Input from "@/components/input/Input.jsx"
import Button from "@/components/button/button.jsx";
import Modal from "@/components/modal/Modal.jsx";

function Index() {
    const [nameInputText, setNameInputText] = useState("");
    const [roomCodeInputText, setRoomCodeInputText] = useState("");
    const [modalOpen, setModalOpen] = useState(false);

    const navigate = useNavigate();

    // Try to get current user. If not authenticated (401) then create a new user by calling join
    const createUserIfNotSignedIn = async (name) => {
        let userData = null;

        // Check if user exists
        try {
            const meResponse = await api.get("auth/me");
            if (meResponse.status === 200) {
                userData = meResponse.data;
            }
        } catch (err) {}

        if (userData) {
            try {
                await api.post("user/new-name", { name: name });

                return userData;
            } catch (err) {
                console.error("Error updating user name:", err);
                throw err;
            }
        }

        // Create user
        try {
            const joinResponse = await api.post("auth/join", {
                name: name
            });
            
            if (joinResponse.status === 201) {
                return joinResponse.data;
            }
        } catch (err) {
            console.error("Error creating user:", err);
            alert(err.response?.data?.error || "Could not sign in. Please try again.");
            throw err;
        }
    }

    // Join room and navigate to it
    const joinRoomAndNavigate = async (name, roomId) => {
        await createUserIfNotSignedIn(name);

        try {
            const roomResponse = await api.post(`room/${roomId}/join`);

            if (roomResponse.status === 204) {
                navigate(`/room/${roomId}`);
            }
        } catch (err) {
            console.error("Error joining room:", err);
            alert(err.response?.data?.error || "Could not join room. Please check the room code and try again.");
        }
    }

    // Create room and navigate to it
    const createRoomAndNavigate = async (name) => {
        await createUserIfNotSignedIn(name);

        try {
            const roomResponse = await api.post("room");

            if (roomResponse.status === 201) {
                const roomId = roomResponse.data.roomId;
                navigate(`/host/${roomId}`);
            }
        } catch (err) {
            console.error("Error creating room:", err);
            alert(err.response?.data?.error || "Could not create room. Please try again.");
        }
    }
    
    return (
        <div className="index-container">
            <h1 id="app-title">
                Draw <span className="highlight" style={{ backgroundColor: colors.primary, color: colors.secondaryDark }}>.it</span>
            </h1>

            <div className="action-container">
                <Input value={nameInputText} 
                       onChange={(e) => setNameInputText(e.target.value)} 
                       placeholder="Enter name"
                />

                <div className="action-button-container">
                    <Button onClick={() => nameInputText.trim() ? setModalOpen(!modalOpen) : alert("Name is required")}>Join Room</Button>
                    <Button onClick={() => nameInputText.trim() ? createRoomAndNavigate(nameInputText) : alert("Name is required")}>Create Room</Button>
                </div>

                <Modal isOpen={modalOpen} onClose={() => setModalOpen(false)}>
                    <div className="modal-container">
                        <h1>Enter room code</h1>
                        <Input value={roomCodeInputText} placeholder="12..." onChange={(e) => setRoomCodeInputText(e.target.value)}/>
                        <Button onClick={() => roomCodeInputText.trim() ? joinRoomAndNavigate(nameInputText, roomCodeInputText) : alert("Room code is required")}>Join</Button>
                    </div>
                </Modal>
            </div>
        </div>
    )
}

export default Index;