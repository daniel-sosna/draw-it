import React from "react";
import "./Input.css";
import colors from "@/constants/colors.js";

export default function NumberInput({
    id,
    value,
    onChange,
    min,
    max,
    step = 1,
    ...rest
}) {
    return (
        <input
            id={id}
            className="input"
            type="number"
            value={value}
            onChange={onChange}
            min={min}
            max={max}
            step={step}
            style={{ borderColor: colors.secondaryDark }}
            {...rest}
        />
    );
}


