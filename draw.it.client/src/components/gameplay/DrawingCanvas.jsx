import React, {useRef, useState, useEffect, useContext} from "react";
import {FaEraser} from "react-icons/fa";
import styles from "@/components/gameplay/DrawingCanvas.module.css";
import "../../index.css";
import {GameplayHubContext} from "@/utils/GameplayHubProvider.jsx";
import WordComponent from "@/components/gameplay/WordComponent.jsx";

const App = () => {
    const canvasRef = useRef(null);
    const strokesRef = useRef([]); // [{ points:[{x,y}], color, size, eraser, canvasW, canvasH }]
    const localPathRef = useRef(null);
    const remotePathRef = useRef(null);
    const [isDrawing, setIsDrawing] = useState(false);
    const [color, setColor] = useState("black");
    const [isEraser, setIsEraser] = useState(false);
    const [isInitialized, setIsInitialized] = useState(false);
    const [brushSize, setBrushSize] = useState(5);
    const gameplayConnection = useContext(GameplayHubContext);
    const [isDrawer, setIsDrawer] = useState(false);
    
    const toNorm = ({x,y}) => {
        const r = canvasRef.current.getBoundingClientRect();
        return { x: x / r.width, y: y / r.height };
    };
    const fromNorm = ({x,y}) => {
        const r = canvasRef.current.getBoundingClientRect();
        return { x: x * r.width, y: y * r.height };
    };

    // Redraw all strokes at current canvas size
    const redrawAll = () => {
        const canvas = canvasRef.current;
        if (!canvas) return;
        const ctx = canvas.getContext("2d");
        clearCanvas();

        const rect = canvas.getBoundingClientRect();
        ctx.save();
        ctx.lineCap = "round";
        strokesRef.current.forEach(stroke => {
            // Scale line width relative to original CSS width
            const scaleW = rect.width / Math.max(1, stroke.canvasW);
            const scaledLineWidth = Math.max(1, stroke.size * scaleW);

            ctx.beginPath();
            ctx.strokeStyle = stroke.eraser ? "white" : stroke.color;
            ctx.lineWidth = scaledLineWidth;

            const pts = stroke.points;
            if (!pts || pts.length === 0) return;

            const p0 = fromNorm(pts[0]);
            ctx.moveTo(p0.x, p0.y);
            for (let i = 1; i < pts.length; i++) {
                const p = fromNorm(pts[i]);
                ctx.lineTo(p.x, p.y);
            }
            ctx.stroke();
        });
        ctx.restore();
    };

    // Receive remote draws and record them so they can be redrawn crisply
    const onReceiveDraw = (drawDto) => {
        const { point, type, color, size, eraser } = drawDto;
        const canvas = canvasRef.current;
        if (!canvas) return;
        const ctx = canvas.getContext("2d");
        const rect = canvas.getBoundingClientRect();

        if (type === "start") {
            remotePathRef.current = {
                points: [{ x: point.x, y: point.y }],
                color,
                size,
                eraser,
                canvasW: rect.width,
                canvasH: rect.height
            };
            strokesRef.current.push(remotePathRef.current);

            // start live path
            ctx.beginPath();
            const p = fromNorm(point);
            ctx.moveTo(p.x, p.y);
            ctx.lineCap = "round";
            const scaleW = rect.width / Math.max(1, remotePathRef.current.canvasW);
            ctx.lineWidth = Math.max(1, size * scaleW);
            ctx.strokeStyle = eraser ? "white" : color;
        } else if (type === "move" && remotePathRef.current) {
            remotePathRef.current.points.push({ x: point.x, y: point.y });
            const p = fromNorm(point);
            ctx.lineTo(p.x, p.y);
            ctx.stroke();
        } else if (type === "end") {
            remotePathRef.current = null;
        }
    };
    
    const onReceiveClear = () => {
        strokesRef.current = [];
        clearCanvas();
    };

    useEffect(() => {
        if(!gameplayConnection) {
            console.log("Gameplay connection not established yet");
            return;
        }
        
        gameplayConnection.on("ReceiveDraw", onReceiveDraw);
        gameplayConnection.on("ReceiveClear", onReceiveClear);
        gameplayConnection.on("SetDrawerStatus", (status) => setIsDrawer(status));
        return () => {
            gameplayConnection.off("ReceiveDraw", onReceiveDraw);
            gameplayConnection.off("ReceiveClear", onReceiveClear);
            gameplayConnection.off("SetDrawerStatus");
        };
    }, [gameplayConnection]);

    const clearCanvas = () => {
        const canvas = canvasRef.current;
        if (!canvas) return;
        const ctx = canvas.getContext("2d");
        ctx.save();
        ctx.setTransform(1,0,0,1,0,0); // draw fill in CSS space
        ctx.fillStyle = "white";
        ctx.fillRect(0, 0, canvas.width, canvas.height);
        ctx.restore();
    };
      
    // Initial canvas setup with DPI scaling
    useEffect(() => {
        const canvas = canvasRef.current;
        if (canvas && !isInitialized) {
            const ctx = canvas.getContext("2d", { alpha: false });

            const displayWidth = canvas.clientWidth;
            const displayHeight = canvas.clientHeight;

            const dpi = window.devicePixelRatio || 1;
            canvas.width  = Math.max(1, Math.round(displayWidth  * dpi));
            canvas.height = Math.max(1, Math.round(displayHeight * dpi));

            ctx.setTransform(1,0,0,1,0,0);
            ctx.scale(dpi, dpi);

            ctx.lineCap = "round";
            ctx.strokeStyle = color;

            clearCanvas();
            setIsInitialized(true);
        }
    }, [isInitialized, color]);

    // Keep brush style in sync for local live drawing
    useEffect(() => {
        if (canvasRef.current) {
            const ctx = canvasRef.current.getContext("2d");
            ctx.strokeStyle = isEraser ? "white" : color;
            ctx.lineWidth = brushSize;
        }
    }, [color, isEraser, brushSize]);

    const getCoordinates = (e) => {
        const rect = canvasRef.current.getBoundingClientRect();
        const x = e.clientX - rect.left;
        const y = e.clientY - rect.top;
        return { x, y };
    };

    const startDrawing = (e) => {
        if (!canvasRef.current || !isDrawer) return;
        const { x, y } = getCoordinates(e);
        const ctx = canvasRef.current.getContext("2d");
        const rect = canvasRef.current.getBoundingClientRect();

        ctx.beginPath();
        ctx.moveTo(x, y);
        ctx.lineCap = "round";
        ctx.strokeStyle = isEraser ? "white" : color;
        ctx.lineWidth = brushSize;
        setIsDrawing(true);

        // record stroke for crisp redraws
        const norm = toNorm({x, y});
        localPathRef.current = {
            points: [norm],
            color,
            size: brushSize,
            eraser: isEraser,
            canvasW: rect.width,
            canvasH: rect.height
        };
        strokesRef.current.push(localPathRef.current);

        gameplayConnection?.invoke("SendDraw", {
            point: norm,
            type: "start",
            color,
            size: brushSize,
            eraser: isEraser,
        });
    };

    const draw = (e) => {
        if (!isDrawing || !canvasRef.current || !isDrawer) return;
        const { x, y } = getCoordinates(e);
        const ctx = canvasRef.current.getContext("2d");
        ctx.lineTo(x, y);
        ctx.stroke();

        const norm = toNorm({x, y});
        if (localPathRef.current) {
            localPathRef.current.points.push(norm);
        }

        gameplayConnection?.invoke("SendDraw", {
            point: norm,
            type: "move",
            color,
            size: brushSize,
            eraser: isEraser,
        });
    };

    const stopDrawing = () => {
        if (!isDrawing) return;
        setIsDrawing(false);
        localPathRef.current = null;

        gameplayConnection?.invoke("SendDraw", {
            point: {x: 0, y: 0},
            type: "end",
            color,
            size: brushSize,
            eraser: isEraser,
        });
    };

    // Resize: adjust backing store and redraw all strokes (vector replay — no blur)
    useEffect(() => {
        const canvas = canvasRef.current;
        if (!canvas) return;

        const ctx = canvas.getContext("2d", { alpha: false });
        let prevDpi = window.devicePixelRatio || 1;

        const resizeToElement = () => {
            const rect = canvas.getBoundingClientRect();
            const dpi = window.devicePixelRatio || 1;

            const newW = Math.max(1, Math.round(rect.width * dpi));
            const newH = Math.max(1, Math.round(rect.height * dpi));

            if (canvas.width !== newW || canvas.height !== newH || dpi !== prevDpi) {
                canvas.width = newW;
                canvas.height = newH;

                ctx.setTransform(1, 0, 0, 1, 0, 0);
                ctx.scale(dpi, dpi);

                // white background
                ctx.save();
                ctx.fillStyle = "white";
                ctx.fillRect(0, 0, rect.width, rect.height);
                ctx.restore();

                // Vector replay -> crisp at any size
                redrawAll();

                prevDpi = dpi;
            }
        };

        const ro = new ResizeObserver(() => {
            requestAnimationFrame(resizeToElement);
        });

        ro.observe(canvas);
        resizeToElement();

        return () => ro.disconnect();
    }, []);

    return (
        <div className="flex h-full min-w-screen p-1 bg-gray-100 font-sans">
            <div className="w-screen h-[80vh] p-4 bg-gray-100 font-sans flex flex-col mr-4">

                {/* Guess the word prompt */}
                <WordComponent />
                
                {isDrawer && (
                    <div className="flex flex-col items-center mb-4 space-y-2">
                        <div className="flex flex-wrap items-center justify-center space-x-2">
                            <button
                                onClick={() => { setColor("black"); setIsEraser(false); }}
                                className={styles.colorButton}
                                style={{ backgroundColor: "black" }}
                            />
                            <button
                                onClick={() => { setColor("red"); setIsEraser(false); }}
                                className={styles.colorButton}
                                style={{ backgroundColor: "red" }}
                            />
                            <button
                                onClick={() => { setColor("blue"); setIsEraser(false); }}
                                className={styles.colorButton}
                                style={{ backgroundColor: "blue" }}
                            />
                            <button
                                onClick={() => { setColor("green"); setIsEraser(false); }}
                                className={styles.colorButton}
                                style={{ backgroundColor: "green" }}
                            />
                            <button
                                onClick={() => { setColor("yellow"); setIsEraser(false); }}
                                className={styles.colorButton}
                                style={{ backgroundColor: "yellow" }}
                            />
                            <button
                                onClick={() => setIsEraser(!isEraser)}
                                className={isEraser ? styles.toolButtonActive : styles.toolButtonInactive}
                            >
                                <FaEraser size={20} color={isEraser ? "white" : "gray"} />
                            </button>
                            <button
                                onClick={() => { gameplayConnection?.invoke("SendClear"); strokesRef.current = []; clearCanvas(); }}
                                className={styles.clearButton}
                            >
                                Clear
                            </button>
                        </div>

                        <div className="flex items-center justify-center space-x-2">
                            <span className={styles.brushLabel}>Brush Size:</span>
                            <input
                                type="range"
                                min="1"
                                max="50"
                                value={brushSize}
                                onChange={(e) => setBrushSize(e.target.valueAsNumber)}
                                className={styles.brushSlider}
                            />
                            <span className={styles.brushValueDisplay}>{brushSize}</span>
                        </div>
                    </div>
                )}

                {/* Canvas */}
                <canvas
                    ref={canvasRef}
                    className="w-full h-4/5 border-2 border-gray-500 cursor-crosshair rounded-lg bg-white"
                    onMouseDown={startDrawing}
                    onMouseMove={draw}
                    onMouseUp={stopDrawing}
                    onMouseLeave={stopDrawing}
                />

                {isDrawer && (
                    <div className="mt-4 text-center text-gray-600">
                        Current Tool: <span className="font-semibold">{isEraser ? "Eraser" : "Pen"}</span>
                        {!isEraser && (
                            <span
                                className="ml-2 w-4 h-4 rounded-full inline-block align-middle"
                                style={{ backgroundColor: color }}
                            />
                        )}
                    </div>
                )}
            </div>
        </div>
    );
};

export default App;
