import React from 'react';
import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import '@testing-library/jest-dom';

import Input from '@/components/input/Input.jsx';
import colors from '@/constants/colors.js';

const hexToRgb = (hex) => {
    const clean = hex.replace('#', '');
    const int = parseInt(clean, 16);
    const r = (int >> 16) & 255;
    const g = (int >> 8) & 255;
    const b = int & 255;
    return `rgb(${r}, ${g}, ${b})`;
};

describe('Input', () => {
    it('renders a text input by default with provided value and placeholder', () => {
        render(<Input value="hello" placeholder="type here" onChange={() => {}} />);

        const input = screen.getByPlaceholderText(/type here/i);

        expect(input).toBeInTheDocument();
        expect(input).toHaveAttribute('type', 'text');
        expect(input).toHaveValue('hello');
    });

    it('calls onChange when the value changes', () => {
        const handleChange = vi.fn();
        render(<Input value="" onChange={handleChange} placeholder="change me" />);

        const input = screen.getByPlaceholderText(/change me/i);
        fireEvent.change(input, { target: { value: 'new value' } });

        expect(handleChange).toHaveBeenCalledTimes(1);
    });

    it('applies the expected border color style', () => {
        render(<Input value="" onChange={() => {}} placeholder="border test" />);

        const input = screen.getByPlaceholderText(/border test/i);
        expect(input.style.borderColor).toBe(hexToRgb(colors.secondaryDark));
    });
});


