import React from "react";
import "./Input.css";
import colors from "@/constants/colors.js";

export default function TextInput({
    id,
    value,
    onChange,
    placeholder,
    ...rest
}) {
    return (
        <input
            id={id}
            className="input"
            type="text"
            value={value}
            onChange={onChange}
            placeholder={placeholder}
            style={{ borderColor: colors.secondaryDark }}
            {...rest}
        />
    );
}


