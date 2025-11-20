import { afterEach } from 'vitest';
import { cleanup } from '@testing-library/react';

// Clean up the DOM after each test to avoid test interference
afterEach(() => {
    cleanup();
});