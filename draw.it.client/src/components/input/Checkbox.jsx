import React from "react";
import "./Input.css";

export default function Checkbox({
    id,
    checked,
    onChange,
    label,
    ...rest
}) {
    return (
        <label>
            <input
                id={id}
                type="checkbox"
                checked={checked}
                onChange={onChange}
                {...rest}
            />
            {label}
        </label>
    );
}


