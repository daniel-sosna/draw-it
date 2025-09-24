import { useEffect, useState } from 'react';
import './App.css';
import GameplayScreen from "./screens/GameplayScreen.jsx"; // remove later

function App() {
    const [drawItems, setDrawItems] = useState();

    useEffect(() => {
        populateDrawItemsData();
    }, []);

    const contents = drawItems === undefined
        ? <p><em>Loading... Please refresh once the ASP.NET backend has started.</em></p>
        : <table className="table table-striped" aria-labelledby="tableLabel">
            <thead>
                <tr>
                    <th>Category</th>
                    <th>Example</th> 
                </tr>
            </thead>
            <tbody>
                {drawItems.map(item =>
                    <tr key={item.category}>
                        <td>{item.category}</td>
                        <td>{item.example}</td> 
                    </tr>
                )}
            </tbody>
        </table>;

    return <GameplayScreen/>; 
    /*(
            <div>
                <h1 id="tableLabel">Draw.it</h1>
                <p>Showing a random thing from each category.</p>
                {contents}
            </div>
        );*/
 
    async function populateDrawItemsData() {
        const response = await fetch('drawitem');
        if (response.ok) {
            const data = await response.json();
            setDrawItems(data);
        }
    }
}

export default App;