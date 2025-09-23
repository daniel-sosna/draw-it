import React from "react";
import "./Input.css";
import colors from "@/constants/colors.js";

export default function Input({ type = "text", value, onChange, placeholder }) {
    return (
        <input
            className="input"
            type={type}
            value={value}
            onChange={onChange}
            placeholder={placeholder}
            style={{ borderColor: colors.gray}}
        />
    );
}
