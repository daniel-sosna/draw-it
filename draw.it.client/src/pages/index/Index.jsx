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
        if (localStorage.getItem("userId")) return true;

        const response = await api.post("api/v1/User", { 
            name: name
        });

        if (response.status === 201) {
            console.log(response)
            localStorage.setItem("userId", response.data.id);
            localStorage.setItem("userName", response.data.name);
            return true;
        }
        return false;
    }

    const joinRoomAndNavigate = async (name) => {
        const userReady = await createUser(name);
        if (!userReady) {
            alert("");
            return;
        }

        const roomResponse = await api.post("api/v1/Room/join", { roomCode: roomCodeInputText });

        if (roomResponse.status === 200) {
            navigate(`/room/${roomCodeInputText}`);
        } else {
            alert("");
        }
    }

    const createRoomAndNavigate = async () => {
        const userReady = await createUser();
        if (!userReady) {
            alert("");
            return;
        }

        const roomResponse = await api.post("api/v1/Room/generate-id");

        if (roomResponse.status === 200) {
            const roomId = roomResponse.data.roomId;

            navigate(`/host/${roomId}`);

        } else {
            alert("");
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
                        <Button onClick={() => joinRoomAndNavigate(nameInputText)}>Join</Button>
                    </div>
                </Modal>
            </div>
        </div>
    )
}

export default Index;