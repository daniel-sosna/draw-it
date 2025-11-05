import React, { useState } from "react";
export default function WordComponent() {

    const [canvasText, setCanvasText] = useState("");

    return (
        //{/* Word to draw/guess */}
        <div className="flex items-center justify-center mb-2">
            <div className="text-center">
                <div className="text-2xl font-bold text-gray-800 bg-white px-4 py-1 rounded-lg border-2 border-orange-200 shadow-sm">
                    {canvasText || "Waiting for word..."}
                </div>
            </div>
        </div>
    );
};

