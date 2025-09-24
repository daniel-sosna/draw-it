import "./Index.css"
import Input from "@/components/input/Input.jsx"
import {useState} from "react";
import Button from "@/components/button/button.jsx";
import colors from "@/constants/colors.js";
import Modal from "@/components/modal/Modal.jsx";
import api from "@/utils/api.js";

function Index() {
    const [nameInputText, setNameInputText] = useState("");
    const [roomCodeInputText, setRoomCodeInputText] = useState("");
    const [modalOpen, setModalOpen] = useState(false);
    
    const generateUserId = async () => {
        const response = await api.post("api/v1/User/generate-id");
        
        if (response.status === 200) {
            localStorage.setItem("userId", response.data.id);
            alert(`Your id is ${response.data.id}`)
        }
    }
    
    return (
        <>
            <h1 id="app-title">
                Draw <span className="highlight" style={{ backgroundColor: colors.primary, color: colors.secondaryDark }}>.it</span>
            </h1>
            
            <div className="action-container">
                <Input value={nameInputText} 
                       onChange={(e) => setNameInputText(e.target.value)} 
                       placeholder="Enter name"
                />
                
                <div className="action-button-container">
                    <Button onClick={() => setModalOpen(!modalOpen)}>Join Room</Button>
                    <Button onClick={() => generateUserId()}>Create Room</Button>
                </div>

                <Modal isOpen={modalOpen} onClose={() => setModalOpen(false)}>
                    <div className="modal-container">
                        <h1>Enter room code</h1>
                        <Input value={roomCodeInputText} placeholder="12..." onChange={(e) => setRoomCodeInputText(e.target.value)}/>
                        <Button onClick={() => generateUserId()}>Join</Button>
                    </div>
                </Modal>
            </div>
        </>
    )
}

export default Index;