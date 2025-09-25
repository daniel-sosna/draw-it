import React, { useRef, useState, useEffect } from "react";

// The main App component
const App = () => {
    const canvasRef = useRef(null);
    const [isDrawing, setIsDrawing] = useState(false);
    const [color, setColor] = useState("black");
    const [isEraser, setIsEraser] = useState(false);
    const [isInitialized, setIsInitialized] = useState(false);
    const [brushSize, setBrushSize] = useState(5);
    const canvasWidth = 1000;  
    const canvasHeight = 600;  
    const clearCanvas = () => {
        if (!canvasRef.current) return;
        const canvas = canvasRef.current;
        const ctx = canvas.getContext("2d");
        ctx.fillStyle = "white";
        ctx.fillRect(0, 0, canvas.width, canvas.height);
    };

    // Set up canvas when the component mounts
    useEffect(() => {
        const canvas = canvasRef.current;
        if (canvas && !isInitialized) {
            const ctx = canvas.getContext("2d");

            // Adjust canvas dimensions for high-DPI screens
            const dpi = window.devicePixelRatio || 1;

            canvas.width = canvasWidth * dpi;   // internal pixels
            canvas.height = canvasHeight * dpi; // internal pixels
            ctx.scale(dpi, dpi);                // scale context

            ctx.lineCap = "round";
            ctx.strokeStyle = color;

            clearCanvas(); // Fill the canvas with a white background on load
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

    // Inline SVG for the eraser icon
    const EraserIcon = () => (
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 576 512" className="w-5 h-5 fill-current">
            <path d="M402.6 83.2l90.2 90.2c12.9 12.9 12.9 33.8 0 46.7l-24.9 24.9c-7.6 7.6-17.7 11.7-28.3 11.7H390.9L161.8 455.8c-10.4 10.4-21.7 16.5-34.1 19.3-11.7 2.7-23.9 1.4-35.2-3.6S78.8 459.7 70 451l-36-36c-8.8-8.8-12.7-20.1-10-31.4s8.9-23.7 19.5-34.2c2.8-2.8 5.6-5.6 8.3-8.3L34.6 221.7c-7.6-7.6-11.7-17.7-11.7-28.3V128.5c0-10.6 4.1-20.7 11.7-28.3l24.9-24.9c12.9-12.9 33.8-12.9 46.7 0l90.2 90.2c-1.8 1.8-3.5 3.6-5.1 5.4-1.5 1.5-3 3-4.4 4.5-1.4 1.4-2.8 2.8-4.1 4.1-1.3 1.3-2.6 2.6-3.8 3.8-1.2 1.2-2.3 2.3-3.4 3.4-1.1 1.1-2.1 2.2-3.1 3.2-1 1-2 2-2.9 3-1 1-1.9 2-2.8 3s-1.7 1.8-2.5 2.7-1.6 1.7-2.3 2.5-1.5 1.6-2.2 2.3-1.4 1.5-2 2.1c-.6 .6-1.1 1.1-1.6 1.6s-.9 .9-1.2 1.2c-.3 .3-.5 .5-.6 .6s-.2 .2-.2 .2z"/>
        </svg>
    );

    return (
        <div className="flex flex-col items-center justify-center p-4 min-h-screen bg-gray-100 font-sans">
            <div className="w-1200px h-800px bg-white p-6 rounded-xl shadow-lg">
                {/* Color Palette and Tools */}
                <div className="flex flex-wrap items-center justify-center space-x-2 mb-4">
                    <button
                        onClick={() => { setColor("black"); setIsEraser(false); }}
                        className="w-8 h-8 rounded-full border-2 border-transparent hover:border-black transition-all duration-200 focus:outline-none"
                        style={{ backgroundColor: "black" }}
                    ></button>
                    <button
                        onClick={() => { setColor("red"); setIsEraser(false); }}
                        className="w-8 h-8 rounded-full border-2 border-transparent hover:border-black transition-all duration-200 focus:outline-none"
                        style={{ backgroundColor: "red" }}
                    ></button>
                    <button
                        onClick={() => { setColor("blue"); setIsEraser(false); }}
                        className="w-8 h-8 rounded-full border-2 border-transparent hover:border-black transition-all duration-200 focus:outline-none"
                        style={{ backgroundColor: "blue" }}
                    ></button>
                    <button
                        onClick={() => { setColor("green"); setIsEraser(false); }}
                        className="w-8 h-8 rounded-full border-2 border-transparent hover:border-black transition-all duration-200 focus:outline-none"
                        style={{ backgroundColor: "green" }}
                    ></button>
                    <button
                        onClick={() => { setColor("yellow"); setIsEraser(false); }}
                        className="w-8 h-8 rounded-full border-2 border-transparent hover:border-black transition-all duration-200 focus:outline-none"
                        style={{ backgroundColor: "yellow" }}
                    ></button>

                    {/* Eraser Button */}
                    <button
                        onClick={() => setIsEraser(!isEraser)}
                        className={`w-10 h-10 rounded-lg flex items-center justify-center ml-4 transition-colors duration-200 ${
                            isEraser ? "bg-black text-white" : "bg-white text-black border border-gray-300"
                        }`}
                    >
                        <EraserIcon />
                    </button>

                    {/* Clear Button */}
                    <button
                        onClick={clearCanvas}
                        className="ml-4 px-4 py-2 rounded-lg bg-gray-200 text-gray-800 font-medium hover:bg-gray-300 transition-colors duration-200"
                    >
                        Clear
                    </button>
                </div>

                {/* Brush Size Slider */}
                <div className="flex items-center justify-center mb-4 space-x-2">
                    <span className="text-gray-600">Brush Size:</span>
                    <input
                        type="range"
                        min="1"
                        max="50"
                        value={brushSize}
                        onChange={(e) => setBrushSize(e.target.value)}
                        className="w-32 accent-gray-500"
                    />
                    <span className="text-gray-600 w-6 text-right">{brushSize}</span>
                </div>

                {/* Canvas */}
                <canvas
                    ref={canvasRef}
                    // className="w-full h-1/2  border-2 border-gray-400 rounded-lg cursor-crosshair bg-white shadow-inner"
                    style={{
                        width: `${canvasWidth}px`,
                        height: `${canvasHeight}px`,
                        border: "2px solid gray",
                        cursor: "crosshair",
                        borderRadius: "8px",
                        backgroundColor: "white",
                    }}
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
