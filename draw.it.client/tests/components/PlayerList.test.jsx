import React from 'react';
import { describe, it, expect} from 'vitest';
import { render, screen} from '@testing-library/react';
import '@testing-library/jest-dom';

import PlayerStatusList from '@/components/gameplay/PlayerStatusList.jsx';

const mockPlayers = [
    { name: 'Charlie', score: 3, isDrawer: false, hasGuessed: false }, 
    { name: 'Alice', score: 1, isDrawer: false, hasGuessed: true },   
    { name: 'Bob', score: 2, isDrawer: true, hasGuessed: false },     
];

describe('PlayerStatusList', () => {

    it('renders the "Leaderboard" title', () => {
        render(<PlayerStatusList players={[]} />);
        expect(screen.getByText('Leaderboard')).toBeInTheDocument();
    });

    it('renders players and sorts them by score descending', () => {
        render(<PlayerStatusList players={mockPlayers} />);

        const scoreTexts = screen.getAllByText(/pts/i).map(el => el.textContent);

        expect(scoreTexts[0]).toContain('3 pts');
        expect(scoreTexts[1]).toContain('2 pts');
        expect(scoreTexts[2]).toContain('1 pts');
    });

    it('displays the correct name and score for each player', () => {
        render(<PlayerStatusList players={mockPlayers} />);

        expect(screen.getByText('Charlie')).toBeInTheDocument();
        expect(screen.getByText('Alice')).toBeInTheDocument();
        expect(screen.getByText('Bob')).toBeInTheDocument();
    });

    it('correctly marks the current drawer with bold/color classes', () => {
        render(<PlayerStatusList players={mockPlayers} />);

        const bobRow = screen.getByText('Bob').closest('.flex.justify-between');
        
        expect(screen.getByText('✏️')).toBeInTheDocument();
        
        expect(bobRow).toHaveClass('bg-indigo-600');
        expect(bobRow).toHaveClass('font-extrabold');
    });

    it('correctly marks a player who has guessed the word', () => {
        render(<PlayerStatusList players={mockPlayers} />);

        const aliceRow = screen.getByText('Alice').closest('.flex.justify-between');

        expect(screen.getByText('✔️')).toBeInTheDocument();

        expect(aliceRow).toHaveClass('text-lime-400');
    });
});
