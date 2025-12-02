import React from 'react';
import { describe, it, expect, vi } from 'vitest';
import { render, fireEvent } from '@testing-library/react';

import Button from '@/components/button/Button.jsx';
import colors from '@/constants/colors';

const hexToRgb = (hex) => {
    const clean = hex.replace('#', '');
    const int = parseInt(clean, 16);
    const r = (int >> 16) & 255;
    const g = (int >> 8) & 255;
    const b = int & 255;
    return `rgb(${r}, ${g}, ${b})`;
};

describe('Button', () => {
    it('renders children text', () => {
        const { getByRole } = render(<Button>Click me</Button>);

        const button = getByRole('button', { name: /click me/i });
        expect(button.textContent).toBe('Click me');
    });

    
    it('calls onClick when clicked', async () => {
        const handleClick = vi.fn();

        const { getByRole } = render(
            <Button onClick={handleClick}>Click me</Button>
        );

        const button = getByRole('button', { name: /click me/i });

        fireEvent.click(button)

        expect(handleClick).toHaveBeenCalledTimes(1);
    });


    it('has primary background color by default', () => {
        const { getByRole } = render(<Button>Default color</Button>);

        const button = getByRole('button', { name: /default color/i });

        expect(button.style.backgroundColor).toBe(hexToRgb(colors.primary));
        expect(button.className).toContain('button');
    });

    it('changes background color on hover and resets on mouse out', () => {
        const { getByRole } = render(<Button>Hover me</Button>);

        const button = getByRole('button', { name: /hover me/i });

        fireEvent.mouseOver(button);
        expect(button.style.backgroundColor).toBe(hexToRgb(colors.primaryDark));

        fireEvent.mouseOut(button);
        expect(button.style.backgroundColor).toBe(hexToRgb(colors.primary));
    });
});
