import React from "react";
import DrawingCanvas from "../screen_components/DrawingCanvas";

export default function GameplayScreen() {
    return (
        <div style={styles.container}>
            <div style={styles.main}>
                <DrawingCanvas />
            </div>
        </div>
    );
}

const styles = {
    container: {
        display: "flex",
        height: "100vh",
    },
    main: {
        flex: 1,
        display: "flex",
        flexDirection: "column",
        padding: "10px",
    },
};