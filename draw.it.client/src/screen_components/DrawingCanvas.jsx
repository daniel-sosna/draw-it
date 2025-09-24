import React, { useRef, useState, useEffect } from "react";

export default function DrawingCanvas() {
    const canvasRef = useRef(null);
    const [isDrawing, setIsDrawing] = useState(false);
    const [color, setColor] = useState("black"); // current color

    // Initialize canvas context
    useEffect(() => {
        const canvas = canvasRef.current;
        const ctx = canvas.getContext("2d");

        // Set drawing styles
        ctx.lineWidth = 3;
        ctx.lineCap = "round";
        ctx.strokeStyle = color; // default color

    }, []);
    
    // When the color changes
    useEffect(() => {
        const ctx = canvasRef.current.getContext("2d");
        ctx.strokeStyle = color;
    }, [color]);

    // Start drawing
    const startDrawing = (e) => {
        const ctx = canvasRef.current.getContext("2d");
        ctx.beginPath();
        ctx.moveTo(e.nativeEvent.offsetX, e.nativeEvent.offsetY);
        setIsDrawing(true);
    };

    // Draws aline as mouse moves
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
        <div>
            {/* Color palette */}
            <div style={{ marginBottom: "10px" }}>
                <button onClick={() => setColor("black")} style={{ background: "black", width: 30, height: 30, marginRight: 5 }} />
                <button onClick={() => setColor("red")} style={{ background: "red", width: 30, height: 30, marginRight: 5 }} />
                <button onClick={() => setColor("blue")} style={{ background: "blue", width: 30, height: 30, marginRight: 5 }} />
                <button onClick={() => setColor("green")} style={{ background: "green", width: 30, height: 30, marginRight: 5 }} />
                <button onClick={() => setColor("yellow")} style={{ background: "yellow", width: 30, height: 30, marginRight: 5 }} />
            </div>

            {/* Canvas */}
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
        </div>
    );
}
