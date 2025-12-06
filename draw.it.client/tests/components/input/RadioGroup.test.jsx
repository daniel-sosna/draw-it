import React from 'react';
import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import '@testing-library/jest-dom';

import RadioGroup from '@/components/input/RadioGroup.jsx';

const OPTIONS = [
    { id: 1, name: 'Animals' },
    { id: 2, name: 'Vehicles' },
    { id: 3, name: 'Games' },
];

describe('RadioGroup', () => {
    it('renders a radiogroup with all options', () => {
        render(
            <RadioGroup
                name="categoryId"
                options={OPTIONS}
                value="2"
                onChange={() => {}}
            />
        );

        const group = screen.getByRole('radiogroup');
        expect(group).toBeInTheDocument();

        OPTIONS.forEach((opt) => {
            expect(screen.getByLabelText(opt.name)).toBeInTheDocument();
        });
    });

    it('marks the selected option as checked based on the value prop', () => {
        render(
            <RadioGroup
                name="categoryId"
                options={OPTIONS}
                value="2"
                onChange={() => {}}
            />
        );

        const selected = screen.getByLabelText('Vehicles');
        const notSelected = screen.getByLabelText('Animals');

        expect(selected).toBeChecked();
        expect(notSelected).not.toBeChecked();
    });

    it('calls onChange when a different option is selected', () => {
        const handleChange = vi.fn();
        render(
            <RadioGroup
                name="categoryId"
                options={OPTIONS}
                value="1"
                onChange={handleChange}
            />
        );

        const vehiclesOption = screen.getByLabelText('Vehicles');
        fireEvent.click(vehiclesOption);

        expect(handleChange).toHaveBeenCalledTimes(1);
    });
});


