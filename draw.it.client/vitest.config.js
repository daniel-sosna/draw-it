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
            reporter: ['lcov', 'text', 'html'],        
            reportsDirectory: './coverage',
            include: ['src/**/*.{js,jsx}'],
            exclude: [
                'tests/**',
                '**/*.test.*',
                '**/*.spec.*',
            ],
        },
    },
});
