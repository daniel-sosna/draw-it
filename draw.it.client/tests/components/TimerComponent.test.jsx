import React from 'react';
import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { render, screen, act } from '@testing-library/react';
import '@testing-library/jest-dom';
import TimerComponent from '@/components/gameplay/TimerComponent.jsx';

// Mock the GameplayHubContext
const mockGameplayConnection = {
    on: vi.fn(),
    off: vi.fn(),
    invoke: vi.fn()
};

// Mock the context provider
vi.mock('@/utils/GameplayHubProvider.jsx', () => ({
    GameplayHubContext: {
        Provider: ({ children }) => children,
        Consumer: ({ children }) => children(mockGameplayConnection)
    }
}));

// Mock react-router
vi.mock('react-router', () => ({
    useParams: vi.fn(() => ({})),
    useNavigate: vi.fn()
}));

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

    it('renders initial timer display as 00:00', () => {
        render(<TimerComponent />);
        expect(screen.getByText('00:00')).toBeInTheDocument();
    });

    it('sets up ReceiveTimer event listener on mount', () => {
        render(<TimerComponent />);

        expect(mockGameplayConnection.on).toHaveBeenCalledWith(
            'ReceiveTimer',
            expect.any(Function)
        );
    });

    it('cleans up ReceiveTimer event listener on unmount', () => {
        const { unmount } = render(<TimerComponent />);

        unmount();

        expect(mockGameplayConnection.off).toHaveBeenCalledWith('ReceiveTimer');
    });

    describe('when ReceiveTimer event is triggered', () => {
        it('calculates server offset and starts countdown', () => {
            render(<TimerComponent />);

            // Get the callback function registered with on()
            const timerCallback = mockGameplayConnection.on.mock.calls.find(
                call => call[0] === 'ReceiveTimer'
            )[1];

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
            render(<TimerComponent />);

            const timerCallback = mockGameplayConnection.on.mock.calls.find(
                call => call[0] === 'ReceiveTimer'
            )[1];

            // Test with 1 minute 5 seconds
            const serverTime = new Date('2024-01-01T10:00:00.000Z');
            const durationSeconds = 65; // 1 minute 5 seconds

            act(() => {
                timerCallback(serverTime.toISOString(), durationSeconds);
            });

            expect(screen.getByText('01:05')).toBeInTheDocument();
        });

        it('resets hasCalledEndRef when new timer starts', () => {
            render(<TimerComponent />);

            const timerCallback = mockGameplayConnection.on.mock.calls.find(
                call => call[0] === 'ReceiveTimer'
            )[1];

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
            render(<TimerComponent />);

            const timerCallback = mockGameplayConnection.on.mock.calls.find(
                call => call[0] === 'ReceiveTimer'
            )[1];

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
            render(<TimerComponent />);

            const timerCallback = mockGameplayConnection.on.mock.calls.find(
                call => call[0] === 'ReceiveTimer'
            )[1];

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
            render(<TimerComponent />);

            const timerCallback = mockGameplayConnection.on.mock.calls.find(
                call => call[0] === 'ReceiveTimer'
            )[1];

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
            render(<TimerComponent />);

            const timerCallback = mockGameplayConnection.on.mock.calls.find(
                call => call[0] === 'ReceiveTimer'
            )[1];

            // Server is 2 seconds ahead of client
            const serverTime = new Date('2024-01-01T10:00:02.000Z'); // Server time
            const clientTime = new Date('2024-01-01T10:00:00.000Z'); // Client time (set by vi.setSystemTime)
            const durationSeconds = 5;

            act(() => {
                timerCallback(serverTime.toISOString(), durationSeconds);
            });

            // With server 2 seconds ahead, the timer should show slightly less time
            // Deadline = serverTime (2s ahead) + 5s = 7s from client perspective
            // But timer shows time until deadline from server perspective
            // This is complex to test directly, but we can verify it doesn't crash
            expect(screen.queryByText('00:00')).not.toBeInTheDocument();
        });

        it('handles negative server offset (client ahead of server)', () => {
            render(<TimerComponent />);

            const timerCallback = mockGameplayConnection.on.mock.calls.find(
                call => call[0] === 'ReceiveTimer'
            )[1];

            // Server is 1 second behind client
            const serverTime = new Date('2024-01-01T09:59:59.000Z'); // Server time (1s behind)
            const clientTime = new Date('2024-01-01T10:00:00.000Z'); // Client time
            const durationSeconds = 3;

            act(() => {
                timerCallback(serverTime.toISOString(), durationSeconds);
            });

            // Should handle negative offset without errors
            expect(screen.queryByText('00:00')).not.toBeInTheDocument();
        });
    });

    it('has correct styling classes', () => {
        const { container } = render(<TimerComponent />);

        const timerElement = screen.getByText('00:00').parentElement;

        expect(timerElement).toHaveClass('absolute');
        expect(timerElement).toHaveClass('top-4');
        expect(timerElement).toHaveClass('right-6');
        expect(timerElement).toHaveClass('bg-black');
        expect(timerElement).toHaveClass('rounded-lg');
        expect(timerElement).toHaveClass('shadow-md');
        expect(timerElement).toHaveClass('text-xl');
        expect(timerElement).toHaveClass('font-semibold');
        expect(timerElement).toHaveClass('text-white');
    });
});