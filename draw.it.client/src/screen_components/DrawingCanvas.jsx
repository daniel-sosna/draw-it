import React, { useRef, useState, useEffect } from "react";

export default function DrawingCanvas() {
    const canvasRef = useRef(null);
    const [isDrawing, setIsDrawing] = useState(false);

    // Initialize canvas context
    useEffect(() => {
        const canvas = canvasRef.current;
        const ctx = canvas.getContext("2d");

        // Set drawing styles
        ctx.lineWidth = 3;
        ctx.lineCap = "round";
        ctx.strokeStyle = "black";
    }, []);

    // Start drawing
    const startDrawing = (e) => {
        const ctx = canvasRef.current.getContext("2d");
        ctx.beginPath();
        ctx.moveTo(e.nativeEvent.offsetX, e.nativeEvent.offsetY);
        setIsDrawing(true);
    };

    // Draw line as mouse moves
    const draw = (e) => {
        if (!isDrawing) return;
        const ctx = canvasRef.current.getContext("2d");
        ctx.lineTo(e.nativeEvent.offsetX, e.nativeEvent.offsetY);
        ctx.stroke();
    };

    // Stop drawing
    const stopDrawing = () => {
        setIsDrawing(false);
    };

    return (
        <canvas
            ref={canvasRef}
            width={800}
            height={500}
            style={{
                border: "2px solid black",
                background: "white",
                cursor: "crosshair",
            }}
            onMouseDown={startDrawing}
            onMouseMove={draw}
            onMouseUp={stopDrawing}
            onMouseLeave={stopDrawing}
        />
    );
}
