import Modal from "./Modal";

export default function ScoreModal({ isOpen, onClose, scores = [], title = "Scores" }) {
    return (
        <Modal isOpen={isOpen} onClose={onClose}>
            <div className="max-w-md w-full">
                <h2 className="text-2xl font-bold mb-4">{title}</h2>
                <ul className="space-y-2">
                    {scores && scores.length > 0 ? (
                        scores.map((s, i) => (
                            <li key={i} className="flex justify-between items-center bg-white/10 p-3 rounded">
                                <span className="truncate">{s.name}</span>
                                <span className="font-semibold">{s.points}</span>
                            </li>
                        ))
                    ) : (
                        <li className="text-sm text-gray-300">No scores available</li>
                    )}
                </ul>
                <div className="mt-6 text-right">
                    <button
                        className="px-4 py-2 rounded bg-primary text-white"
                        onClick={onClose}
                    >
                        Close
                    </button>
                </div>
            </div>
        </Modal>
    );
}
