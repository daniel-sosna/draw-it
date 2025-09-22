import React from "react";
import colors from "../../constants/colors";
import "./Button.css"

export default function Button({ children, onClick }) {
    return (
        <button
            onClick={onClick}
            style={{ backgroundColor: colors.primary }}
            className={"button"}
            onMouseOver={(e) => (e.currentTarget.style.backgroundColor = colors.primaryDark)}
            onMouseOut={(e) => (e.currentTarget.style.backgroundColor = colors.primary)}
        >
            {children}
        </button>
    );
}
