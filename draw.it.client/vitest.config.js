import { defineConfig } from 'vitest/config';
import react from '@vitejs/plugin-react';

export default defineConfig({
    plugins: [react()],
    resolve: {
        alias: {
            '@': '/src',
        },
    },
    test: {
        globals: true,
        environment: 'jsdom',
        setupFiles: ['./tests/setupTests.js'],
        coverage: {
            provider: 'v8',                  
            reporter: ['text', 'html'],        
            reportsDirectory: './coverage',
            include: ['src/**/*.{js,jsx,ts,tsx}'],
            exclude: [
                'tests/**',
                '**/*.test.*',
                '**/*.spec.*',
            ],
        },
    },
});
