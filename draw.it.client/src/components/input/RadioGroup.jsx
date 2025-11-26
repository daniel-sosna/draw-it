import React from "react";

export default function RadioGroup({
    name,
    options,
    value,
    onChange,
    renderLabel, // optional custom label renderer
    className,
}) {
    return (
        <div role="radiogroup" className={className}>
            {options.map((opt) => {
                const id = `${name}-${opt.id}`;
                return (
                    <label key={opt.id} htmlFor={id} className="radio-label">
                        <input
                            id={id}
                            type="radio"
                            name={name}
                            value={String(opt.id)}
                            checked={String(value) === String(opt.id)}
                            onChange={onChange}
                            className="category-radio"
                        />
                        {renderLabel ? renderLabel(opt) : opt.name}
                    </label>
                );
            })}
        </div>
    );
}


