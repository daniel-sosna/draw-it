import React from 'react';
import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import '@testing-library/jest-dom';

import TextInput from '@/components/input/TextInput.jsx';
import colors from '@/constants/colors.js';

const hexToRgb = (hex) => {
    const clean = hex.replace('#', '');
    const int = parseInt(clean, 16);
    const r = (int >> 16) & 255;
    const g = (int >> 8) & 255;
    const b = int & 255;
    return `rgb(${r}, ${g}, ${b})`;
};

describe('TextInput', () => {
    it('renders a text input with id, value and placeholder', () => {
        render(
            <TextInput
                id="room-name"
                value="My Room"
                placeholder="Room name"
                onChange={() => {}}
            />
        );

        const input = screen.getByPlaceholderText(/room name/i);

        expect(input).toBeInTheDocument();
        expect(input).toHaveAttribute('id', 'room-name');
        expect(input).toHaveAttribute('type', 'text');
        expect(input).toHaveValue('My Room');
    });

    it('calls onChange when user types', () => {
        const handleChange = vi.fn();
        render(
            <TextInput
                id="room-name"
                value=""
                placeholder="Room name"
                onChange={handleChange}
            />
        );

        const input = screen.getByPlaceholderText(/room name/i);
        fireEvent.change(input, { target: { value: 'New Name' } });

        expect(handleChange).toHaveBeenCalledTimes(1);
    });

    it('applies the expected border color style', () => {
        render(
            <TextInput
                id="room-name"
                value=""
                placeholder="Room name"
                onChange={() => {}}
            />
        );

        const input = screen.getByPlaceholderText(/room name/i);
        expect(input.style.borderColor).toBe(hexToRgb(colors.secondaryDark));
    });
});


