import React from "react";
import ReactDOM from "react-dom";
import "./Modal.css";
import colors from "@/constants/colors.js";

export default function Modal({ isOpen, onClose, children }) {
    if (!isOpen) return null; // Don't render if closed

    return ReactDOM.createPortal(
        <div className="modal-overlay" onClick={onClose}>
            <div
                className="modal-content"
                style={{ backgroundColor: colors.secondary }}
                onClick={(e) => e.stopPropagation()} // prevent closing when clicking inside
            >
                <button className="modal-close" onClick={onClose}>
                    ❌
                </button>
                {children}
            </div>
        </div>,
        document.body // render into <body>
    );
}
