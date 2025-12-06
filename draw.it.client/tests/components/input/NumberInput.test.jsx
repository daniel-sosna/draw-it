import React from 'react';
import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import '@testing-library/jest-dom';

import NumberInput from '@/components/input/NumberInput.jsx';
import colors from '@/constants/colors.js';
import { hexToRgb } from '../../utils/colors';

describe('NumberInput', () => {
    it('renders a number input with min, max and step props', () => {
        render(
            <NumberInput
                id="rounds"
                value={3}
                min={1}
                max={10}
                step={1}
                onChange={() => {}}
            />
        );

        const input = screen.getByDisplayValue('3');

        expect(input).toBeInTheDocument();
        expect(input).toHaveAttribute('type', 'number');
        expect(input).toHaveAttribute('id', 'rounds');
        expect(input).toHaveAttribute('min', '1');
        expect(input).toHaveAttribute('max', '10');
        expect(input).toHaveAttribute('step', '1');
    });

    it('calls onChange when the value changes', () => {
        const handleChange = vi.fn();
        render(
            <NumberInput
                id="rounds"
                value={2}
                min={1}
                max={10}
                step={1}
                onChange={handleChange}
            />
        );

        const input = screen.getByDisplayValue('2');
        fireEvent.change(input, { target: { value: '4' } });

        expect(handleChange).toHaveBeenCalledTimes(1);
    });

    it('applies the expected border color style', () => {
        render(
            <NumberInput
                id="rounds"
                value={2}
                min={1}
                max={10}
                step={1}
                onChange={() => {}}
            />
        );

        const input = screen.getByDisplayValue('2');
        expect(input.style.borderColor).toBe(hexToRgb(colors.secondaryDark));
    });
});


