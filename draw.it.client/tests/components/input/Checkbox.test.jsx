import React from 'react';
import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import '@testing-library/jest-dom';

import Checkbox from '@/components/input/Checkbox.jsx';

describe('Checkbox', () => {
    it('renders a checkbox with label text', () => {
        render(
            <Checkbox
                id="remember"
                checked={false}
                onChange={() => {}}
                label="Remember me"
            />
        );

        const checkbox = screen.getByLabelText(/remember me/i);

        expect(checkbox).toBeInTheDocument();
        expect(checkbox).toHaveAttribute('type', 'checkbox');
        expect(checkbox).not.toBeChecked();
    });

    it('uses checked prop and calls onChange when toggled', () => {
        const handleChange = vi.fn();
        render(
            <Checkbox
                id="remember"
                checked={true}
                onChange={handleChange}
                label="Remember me"
            />
        );

        const checkbox = screen.getByLabelText(/remember me/i);
        expect(checkbox).toBeChecked();

        fireEvent.click(checkbox);

        expect(handleChange).toHaveBeenCalledTimes(1);
    });
});


