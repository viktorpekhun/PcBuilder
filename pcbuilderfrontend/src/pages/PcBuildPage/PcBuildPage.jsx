import {Link, useNavigate} from "react-router-dom";
import {useEffect, useRef, useState} from "react";
import styles from './PcBuildPage.module.css'

function PcBuildPage() {
    const [selectedComponents, setSelectedComponents] = useState({
        cpu: null,
        gpu: null,
        motherboard: null,
        ram: null,
        ssd: null,
        hdd: null,
        powerSupply: null,
        cpuCooler: null,
        pcCase: null,
        fan: null
    });
    const navigate = useNavigate();
    const isInitialMount = useRef(true);

    const componentTypes = [
        { key: 'cpu', label: 'CPU' },
        { key: 'gpu', label: 'GPU' },
        { key: 'motherboard', label: 'Motherboard' },
        { key: 'ram', label: 'RAM' },
        { key: 'ssd', label: 'SSD' },
        { key: 'hdd', label: 'HDD' },
        { key: 'powerSupply', label: 'Power Supply' },
        { key: 'cpuCooler', label: 'CPU Cooler' },
        { key: 'pcCase', label: 'PC Case' },
        { key: 'fan', label: 'Fan' }
    ];

    // Load saved components from localStorage on initial render
    useEffect(() => {
        try {
            console.log("PcBuildPage mounted, checking localStorage");
            const savedComponents = localStorage.getItem('selectedComponents');
            console.log("Retrieved from localStorage:", savedComponents);

            if (savedComponents) {
                const parsedComponents = JSON.parse(savedComponents);
                console.log("Parsed components:", parsedComponents);
                setSelectedComponents(parsedComponents);
            }
        } catch (err) {
            console.error("Error loading components from localStorage:", err);
        }
    }, []);

    // Save components to localStorage when they change, but not on initial mount
    useEffect(() => {
        // Skip the first render
        if (isInitialMount.current) {
            isInitialMount.current = false;
            return;
        }

        try {
            console.log("Saving components to localStorage:", selectedComponents);
            localStorage.setItem('selectedComponents', JSON.stringify(selectedComponents));
        } catch (err) {
            console.error("Error saving to localStorage:", err);
        }
    }, [selectedComponents]);

    const removeComponent = (type) => {
        console.log("Removing component type:", type);
        setSelectedComponents(prev => ({
            ...prev,
            [type]: null
        }));
    };

    return (
        <section className={styles['build-components-page']}>
            <h1>Build Your PC</h1>
            <table className={styles['build-components-table']}>
                <thead>
                    <tr>
                        <th>Type</th>
                        <th>Component</th>
                        <th>Price</th>
                        <th>Actions</th>
                    </tr>
                </thead>
                <tbody>
                    {componentTypes.map(({ key, label }) => (
                        <tr key={key}>
                            <td>{label}</td>
                            <td>
                                {selectedComponents[key] ? (
                                    <div className={styles['selected-component']}>
                                        {selectedComponents[key].photoUrl && (
                                            <img
                                                src={selectedComponents[key].photoUrl}
                                                alt={selectedComponents[key].name}
                                                width="50"
                                            />
                                        )}
                                        <div className={styles['component-name']}>
                                            {selectedComponents[key].name}
                                        </div>
                                    </div>
                                ) : (
                                    <button>
                                        <Link to={`/components/${key}`}>Choose A {label}</Link>
                                    </button>
                                )}
                            </td>
                            <td>{selectedComponents[key]?.averagePrice || ""}</td>
                            <td>
                                {selectedComponents[key] && (
                                    <button onClick={() => removeComponent(key)}>
                                        Remove
                                    </button>
                                )}
                            </td>
                        </tr>
                    ))}
                </tbody>
            </table>
        </section>
    );
}

export default PcBuildPage;
