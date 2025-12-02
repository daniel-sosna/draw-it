import React from "react";
import "./Input.css";
import colors from "@/constants/colors.js";

// Utility to clamp and snap a numeric value based on provided rules.
const clampAndSnap = (val, { min, max, step }) => {
    let v = Number(val);
    if (Number.isNaN(v)) v = min;
    v = Math.min(max, Math.max(min, v));
    const base = min ?? 0;
    if (step && step > 0) {
        v = Math.round((v - base) / step) * step + base;
    }
    return v;
};

export default function NumberInput({
    id,
    value,
    onChange,
    min,
    max,
    step = 1,
    clamped = false,
    ...rest
}) {
    const handleChange = (event) => {
        if (!onChange) return;

        if (!clamped) {
            onChange(event);
            return;
        }

        const newValue = clampAndSnap(event.target.value, { min, max, step });
        onChange(newValue);
    };

    return (
        <input
            id={id}
            className="input"
            type="number"
            value={value}
            onChange={handleChange}
            min={min}
            max={max}
            step={step}
            style={{ borderColor: colors.secondaryDark }}
            {...rest}
        />
    );
}

