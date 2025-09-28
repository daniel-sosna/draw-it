import React, { useRef, useState, useEffect } from "react";
import {FaEraser} from "react-icons/fa";
import styles from "@/components/gameplay/DrawingCanvas.module.css";
import "../../index.css";

// The main App component
const App = () => {
    const canvasRef = useRef(null);
    const [isDrawing, setIsDrawing] = useState(false);
    const [color, setColor] = useState("black");
    const [isEraser, setIsEraser] = useState(false);
    const [isInitialized, setIsInitialized] = useState(false);
    const [brushSize, setBrushSize] = useState(5);
    
    const clearCanvas = () => {
        if (!canvasRef.current) return;
        const canvas = canvasRef.current;
        const ctx = canvas.getContext("2d");
        const displayWidth = canvas.clientWidth;
        const displayHeight = canvas.clientHeight;
        ctx.fillStyle = "white";
        ctx.fillRect(0, 0, displayWidth, displayHeight);
    };

    // Set up canvas when the component mounts
    useEffect(() => {
        const canvas = canvasRef.current;
        if (canvas && !isInitialized) {
            const ctx = canvas.getContext("2d");

            const displayWidth = canvas.clientWidth;
            const displayHeight = canvas.clientHeight;

            // Adjust canvas dimensions for high-DPI screens
            const dpi = window.devicePixelRatio || 1;

            // FIX: Set internal pixel size based on display size
            canvas.width = displayWidth * dpi;   // internal pixels
            canvas.height = displayHeight * dpi; // internal pixels

            ctx.scale(dpi, dpi);                // scale context

            ctx.lineCap = "round";
            ctx.strokeStyle = color;

            // Clear canvas needs to use the display dimensions now
            clearCanvas();
            setIsInitialized(true);
        }
    }, [isInitialized, color]);

    // Update brush color and size when state changes
    useEffect(() => {
        if (canvasRef.current) {
            const ctx = canvasRef.current.getContext("2d");
            ctx.strokeStyle = isEraser ? "white" : color;
            ctx.lineWidth = isEraser ? 20 : brushSize; // Thicker line for eraser
        }
    }, [color, isEraser, brushSize]);

    // Get coordinates relative to the canvas
    const getCoordinates = (e) => {
        const rect = canvasRef.current.getBoundingClientRect();
        const x = e.clientX - rect.left;
        const y = e.clientY - rect.top;
        return { x, y };
    };

    const startDrawing = (e) => {
        if (!canvasRef.current) return;
        const { x, y } = getCoordinates(e);
        const ctx = canvasRef.current.getContext("2d");
        ctx.beginPath();
        ctx.moveTo(x, y);
        setIsDrawing(true);
    };

    const draw = (e) => {
        if (!isDrawing || !canvasRef.current) return;
        const { x, y } = getCoordinates(e);
        const ctx = canvasRef.current.getContext("2d");
        ctx.lineTo(x, y);
        ctx.stroke();
    };

    const stopDrawing = () => {
        setIsDrawing(false);
    };
    

    return (
        <div className="flex h-full min-w-screen p-4 bg-gray-100 font-sans">
            <div className="w-screen h-[80vh] p-4 bg-gray-100 font-sans flex flex-col mr-4">
                {/* Color Palette and Tools */}
                <div className="flex flex-wrap items-center justify-center space-x-2 mb-4">
                    <button
                        onClick={() => { setColor("black"); setIsEraser(false); }}
                        className={styles.colorButton}
                        style={{ backgroundColor: "black" }}
                    ></button>
                    <button
                        onClick={() => { setColor("red"); setIsEraser(false); }}
                        className={styles.colorButton}
                        style={{ backgroundColor: "red" }}
                    ></button>
                    <button
                        onClick={() => { setColor("blue"); setIsEraser(false); }}
                        className={styles.colorButton}
                        style={{ backgroundColor: "blue" }}
                    ></button>
                    <button
                        onClick={() => { setColor("green"); setIsEraser(false); }}
                        className={styles.colorButton}
                        style={{ backgroundColor: "green" }}
                    ></button>
                    <button
                        onClick={() => { setColor("yellow"); setIsEraser(false); }}
                        className={styles.colorButton}
                        style={{ backgroundColor: "yellow" }}
                    ></button>

                    {/* Eraser Button */}
                    <button
                        onClick={() => setIsEraser(!isEraser)}
                        className={
                            isEraser
                                ? styles.toolButtonActive // Applies the 'active' styles
                                : styles.toolButtonInactive // Applies the 'inactive' styles
                        }
                    >
                        <FaEraser size={20} color={isEraser ? "white" : "gray"} />
                    </button>

                    {/* Clear Button */}
                    <button
                        onClick={clearCanvas}
                        className={styles.clearButton}>
                        Clear
                    </button>
                </div>

                {/* Brush Size Slider */}
                <div className="flex items-center justify-center mb-4 space-x-2">

                    <span className={styles.brushLabel}>Brush Size:</span>

                    <input
                        type="range"
                        min="1"
                        max="50"
                        value={brushSize}
                        onChange={(e) => setBrushSize(e.target.value)}
                        className={styles.brushSlider}
                    />

                    <span className={styles.brushValueDisplay}>{brushSize}</span>
                </div>

                {/* Canvas */}
                <canvas
                    ref={canvasRef}
                    className="w-full h-4/5 border-2 border-gray-500 cursor-crosshair rounded-lg bg-black"
                    onMouseDown={startDrawing}
                    onMouseMove={draw}
                    onMouseUp={stopDrawing}
                    onMouseLeave={stopDrawing}
                />

                {/* Display current color/tool */}
                <div className="mt-4 text-center text-gray-600">
                    Current Tool: <span className="font-semibold">{isEraser ? "Eraser" : "Pen"}</span>
                    {!isEraser && (
                        <span
                            className="ml-2 w-4 h-4 rounded-full inline-block align-middle"
                            style={{ backgroundColor: color }}
                        ></span>
                    )}
                </div>
            </div>
        </div>
    );
};

export default App;
