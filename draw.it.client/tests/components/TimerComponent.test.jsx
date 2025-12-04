import React from 'react';
import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { render, screen, act } from '@testing-library/react';
import '@testing-library/jest-dom';
import TimerComponent from '@/components/gameplay/TimerComponent.jsx';

// Create mock connection
const mockGameplayConnection = {
    on: vi.fn(),
    off: vi.fn(),
    invoke: vi.fn()
};

// Mock the context
vi.mock('@/utils/GameplayHubProvider.jsx', () => ({
    GameplayHubContext: React.createContext(null)
}));

// Mock react-router
vi.mock('react-router', () => ({
    useParams: vi.fn(() => ({})),
    useNavigate: vi.fn()
}));

// Import the actual context after mocking
import { GameplayHubContext } from '@/utils/GameplayHubProvider.jsx';

describe('TimerComponent', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        vi.useFakeTimers();
        // Reset Date.now to a fixed value for consistent testing
        const fixedDate = new Date('2024-01-01T10:00:00.000Z');
        vi.setSystemTime(fixedDate);
    });

    afterEach(() => {
        vi.useRealTimers();
    });

    const renderWithContext = (connection = mockGameplayConnection) => {
        return render(
            <GameplayHubContext.Provider value={connection}>
                <TimerComponent />
            </GameplayHubContext.Provider>
        );
    };

    it('renders initial timer display as 00:00', () => {
        renderWithContext();
        expect(screen.getByText('00:00')).toBeInTheDocument();
    });

    it('sets up ReceiveTimer event listener on mount', () => {
        renderWithContext();

        expect(mockGameplayConnection.on).toHaveBeenCalledWith(
            'ReceiveTimer',
            expect.any(Function)
        );
    });

    it('cleans up ReceiveTimer event listener on unmount', () => {
        const { unmount } = renderWithContext();

        unmount();

        expect(mockGameplayConnection.off).toHaveBeenCalledWith('ReceiveTimer');
    });

    describe('when ReceiveTimer event is triggered', () => {
        it('calculates server offset and starts countdown', () => {
            renderWithContext();

            // Get the callback function registered with on()
            const timerCallback = mockGameplayConnection.on.mock.calls.find(
                call => call[0] === 'ReceiveTimer'
            )?.[1];

            expect(timerCallback).toBeDefined();

            // Simulate server sending timer data (10 seconds from now)
            const serverTime = new Date('2024-01-01T10:00:00.000Z');
            const durationSeconds = 10;

            act(() => {
                timerCallback(serverTime.toISOString(), durationSeconds);
            });

            // Should update display initially
            expect(screen.getByText('00:10')).toBeInTheDocument();
        });

        it('formats minutes and seconds correctly', () => {
            renderWithContext();

            const timerCallback = mockGameplayConnection.on.mock.calls.find(
                call => call[0] === 'ReceiveTimer'
            )?.[1];

            expect(timerCallback).toBeDefined();

            // Test with 1 minute 5 seconds
            const serverTime = new Date('2024-01-01T10:00:00.000Z');
            const durationSeconds = 65; // 1 minute 5 seconds

            act(() => {
                timerCallback(serverTime.toISOString(), durationSeconds);
            });

            expect(screen.getByText('01:05')).toBeInTheDocument();
        });

        it('resets hasCalledEndRef when new timer starts', () => {
            renderWithContext();

            const timerCallback = mockGameplayConnection.on.mock.calls.find(
                call => call[0] === 'ReceiveTimer'
            )?.[1];

            expect(timerCallback).toBeDefined();

            const serverTime = new Date('2024-01-01T10:00:00.000Z');
            const durationSeconds = 10;

            act(() => {
                timerCallback(serverTime.toISOString(), durationSeconds);
            });

            // hasCalledEndRef should be reset to false when timer starts
            // We can verify this by checking that invoke is not called immediately
            expect(mockGameplayConnection.invoke).not.toHaveBeenCalled();
        });
    });

    describe('countdown functionality', () => {
        it('updates timer every second', () => {
            renderWithContext();

            const timerCallback = mockGameplayConnection.on.mock.calls.find(
                call => call[0] === 'ReceiveTimer'
            )?.[1];

            expect(timerCallback).toBeDefined();

            const serverTime = new Date('2024-01-01T10:00:00.000Z');
            const durationSeconds = 3;

            act(() => {
                timerCallback(serverTime.toISOString(), durationSeconds);
            });

            // Initial state: 00:03
            expect(screen.getByText('00:03')).toBeInTheDocument();

            // After 1 second: 00:02
            act(() => {
                vi.advanceTimersByTime(1000);
            });
            expect(screen.getByText('00:02')).toBeInTheDocument();

            // After 2 seconds: 00:01
            act(() => {
                vi.advanceTimersByTime(1000);
            });
            expect(screen.getByText('00:01')).toBeInTheDocument();

            // After 3 seconds: 00:00 and TimerEnded invoked
            act(() => {
                vi.advanceTimersByTime(1000);
            });
            expect(screen.getByText('00:00')).toBeInTheDocument();
            expect(mockGameplayConnection.invoke).toHaveBeenCalledWith('TimerEnded');
        });

        it('calls TimerEnded only once when timer reaches zero', () => {
            renderWithContext();

            const timerCallback = mockGameplayConnection.on.mock.calls.find(
                call => call[0] === 'ReceiveTimer'
            )?.[1];

            expect(timerCallback).toBeDefined();

            const serverTime = new Date('2024-01-01T10:00:00.000Z');
            const durationSeconds = 1;

            act(() => {
                timerCallback(serverTime.toISOString(), durationSeconds);
            });

            // Advance past timer end
            act(() => {
                vi.advanceTimersByTime(2000); // More than 1 second
            });

            // Should only call TimerEnded once
            expect(mockGameplayConnection.invoke).toHaveBeenCalledTimes(1);
            expect(mockGameplayConnection.invoke).toHaveBeenCalledWith('TimerEnded');

            // Advance more time - should not call again
            act(() => {
                vi.advanceTimersByTime(1000);
            });
            expect(mockGameplayConnection.invoke).toHaveBeenCalledTimes(1);
        });

        it('does not go below 00:00', () => {
            renderWithContext();

            const timerCallback = mockGameplayConnection.on.mock.calls.find(
                call => call[0] === 'ReceiveTimer'
            )?.[1];

            expect(timerCallback).toBeDefined();

            const serverTime = new Date('2024-01-01T10:00:00.000Z');
            const durationSeconds = 1;

            act(() => {
                timerCallback(serverTime.toISOString(), durationSeconds);
            });

            // Advance well past the timer end
            act(() => {
                vi.advanceTimersByTime(5000); // 5 seconds
            });

            // Should still show 00:00
            expect(screen.getByText('00:00')).toBeInTheDocument();
        });
    });

    describe('server offset calculation', () => {
        it('handles positive server offset (server ahead of client)', () => {
            renderWithContext();

            const timerCallback = mockGameplayConnection.on.mock.calls.find(
                call => call[0] === 'ReceiveTimer'
            )?.[1];

            expect(timerCallback).toBeDefined();

            // Server is 2 seconds ahead of client
            const serverTime = new Date('2024-01-01T10:00:02.000Z'); // Server time
            const durationSeconds = 5;

            act(() => {
                timerCallback(serverTime.toISOString(), durationSeconds);
            });

            // With server 2 seconds ahead, should calculate offset
            expect(screen.queryByText('00:00')).not.toBeInTheDocument();
        });

        it('handles negative server offset (client ahead of server)', () => {
            renderWithContext();

            const timerCallback = mockGameplayConnection.on.mock.calls.find(
                call => call[0] === 'ReceiveTimer'
            )?.[1];

            expect(timerCallback).toBeDefined();

            // Server is 1 second behind client
            const serverTime = new Date('2024-01-01T09:59:59.000Z'); // Server time (1s behind)
            const durationSeconds = 3;

            act(() => {
                timerCallback(serverTime.toISOString(), durationSeconds);
            });

            // Should handle negative offset without errors
            expect(screen.queryByText('00:00')).not.toBeInTheDocument();
        });
    });

 
});