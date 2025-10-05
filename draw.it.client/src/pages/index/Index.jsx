import "./Index.css"
import Input from "@/components/input/Input.jsx"
import {useState} from "react";
import Button from "@/components/button/button.jsx";
import colors from "@/constants/colors.js";
import Modal from "@/components/modal/Modal.jsx";
import api from "@/utils/api.js";
import { useNavigate } from "react-router";

function Index() {
    const [nameInputText, setNameInputText] = useState("");
    const [roomCodeInputText, setRoomCodeInputText] = useState("");
    const [modalOpen, setModalOpen] = useState(false);

    const navigate = useNavigate();

    const createUser = async (name) => {
        localStorage.removeItem("userId");
        localStorage.removeItem("userName");

        const response = await api.post("api/v1/User", {
            name: name
        });

        if (response.status === 201) {
            console.log(response)
            localStorage.setItem("userId", response.data.id);
            localStorage.setItem("userName", response.data.name);
            return true;
        }
        alert(`Klaida sukuriant vartotoją. Statusas: ${response.status}`);
        return false;
    }

    const joinRoom = async (roomId, userId, isHost) => {
        return api.post(`api/v1/Room/${roomId}/join`, {
            UserId: parseInt(userId, 10), 
            IsHost: isHost, 
        });
    }

    const createRoomAndNavigate = async (name) => {
        const userReady = await createUser(name);
        if (!userReady) {
            alert("Nepavyko gauti vartotojo ID. Bandykite dar kartą.");
            return;
        }
        
        const userId = localStorage.getItem("userId");
        
        try {
            const createResponse = await api.post("api/v1/Room/create", {
                UserId: parseInt(userId, 10),
               });
                
            if (createResponse.status === 200) {
                const roomId = createResponse.data.id;
                navigate(`/host/${roomId}`); 
            } else {
                alert(`Klaida prisijungiant prie kambario: ${createResponse.data?.error || 'Nežinoma klaida'}`);
            }
        } catch (error) {
            console.error("Klaida kuriant/prisijungiant:", error.response?.data?.error || error);
            alert(`Klaida kuriant/prisijungiant prie kambario: ${error.response?.data?.error || 'Nežinoma klaida'}`);
        }

    }
    const joinRoomAndNavigate = async (name, roomCode) => {
        if (!roomCode) {
            alert("Įveskite kambario kodą!");
            return;
        }

        const userReady = await createUser(name);

        if (!userReady) {
            alert("Nepavyko gauti vartotojo ID. Bandykite dar kartą.");
            return;
        }

        const userId = localStorage.getItem("userId");

        try {
            const response = await api.post(`api/v1/Room/${roomCode}/join`, {
                UserId: parseInt(userId, 10),
            });
            if (response.status === 200) {
                navigate(`/room/${roomCode}`);
            } else {
                alert(`Klaida prisijungiant prie kambario: ${response.data?.error || "Nenumatyta klaida"}`);
            }
        } catch (error) {
            console.error("Prisijungimo prie kambario klaida:", error);
            alert(`Klaida prisijungiant prie kambario: ${error.response?.data?.error || error.message}`);
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
                    <Button onClick={() => nameInputText ? setModalOpen(!modalOpen) : alert("Įveskite vardą")}>Join Room</Button>
                    <Button onClick={() => nameInputText ? createRoomAndNavigate(nameInputText) : alert("Įveskite vardą")}>Create Room</Button>
                </div>

                <Modal isOpen={modalOpen} onClose={() => setModalOpen(false)}>
                    <div className="modal-container">
                        <h1>Enter room code</h1>
                        <Input value={roomCodeInputText} placeholder="12..." onChange={(e) => setRoomCodeInputText(e.target.value)}/>
                        <Button onClick={() => joinRoomAndNavigate(nameInputText, roomCodeInputText)}>Join</Button>
                    </div>
                </Modal>
            </div>
        </div>
    )
}

export default Index;