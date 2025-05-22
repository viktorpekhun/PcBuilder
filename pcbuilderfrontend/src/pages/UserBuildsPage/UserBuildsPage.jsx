import { useState, useEffect } from 'react';
import {Link, useNavigate} from 'react-router-dom';
import styles from './UserBuildsPage.module.css';
import useAuth from '../../hooks/useAuth';
import useAxiosPrivate from '../../hooks/useAxiosPrivate';
import DeleteModal from "../../components/DeleteModal/DeleteModal.jsx";

const USER_BUILDS = '/api/pcBuild/user-builds';
const USER_BUILD = '/api/pcBuild';
function UserBuildsPage() {
    const [builds, setBuilds] = useState([]);
    const [selectedBuild, setSelectedBuild] = useState(null);
    const [selectedBuildId, setSelectedBuildId] = useState(null);
    const [loading, setLoading] = useState(true);
    const navigate = useNavigate();
    const [error, setError] = useState(null);
    const [deleteStatus, setDeleteStatus] = useState({ loading: false, error: null });
    const { auth } = useAuth();
    const axiosPrivate = useAxiosPrivate();
    const [deleteModal, setDeleteModal] = useState({
        isOpen: false,
        buildId: null,
        buildName: ''
    });

    const handleDeleteBuild = async () => {
        if (!deleteModal.buildId) return;

        try {
            setDeleteStatus({ loading: true, error: null });
            // Send delete request
            await axiosPrivate.delete(`${USER_BUILD}/${deleteModal.buildId}`);

            // Update builds list
            setBuilds(prevBuilds => prevBuilds.filter(build => build.id !== deleteModal.buildId));

            // If the deleted build was selected, select the first remaining build or null
            if (selectedBuildId === deleteModal.buildId) {
                const remainingBuilds = builds.filter(build => build.id !== deleteModal.buildId);
                if (remainingBuilds.length > 0) {
                    setSelectedBuildId(remainingBuilds[0].id);
                } else {
                    setSelectedBuildId(null);
                    setSelectedBuild(null);
                }
            }

            setDeleteStatus({ loading: false, error: null });
            // Close the modal
            setDeleteModal({ isOpen: false, buildId: null, buildName: '' });
        } catch (err) {
            console.error('Error deleting build:', err);
            setDeleteStatus({
                loading: false,
                error: err.response?.data?.message || 'Failed to delete build. Please try again.'
            });
        }
    };

    const handleEditBuild = () => {
        if (!selectedBuild) return;
        console.log("Edit build - transformed components:", selectedBuild);
        // Transform the selected build data to the format expected by PcBuildPage
        const transformedComponents = {
            // Single components
            cpu: selectedBuild.cpu ? {
                componentId: selectedBuild.cpu.id,
                price: selectedBuild.cpu.price,
                storeName: selectedBuild.cpu.storeName,
                storeLogoUrl: selectedBuild.cpu.storeLogoUrl || selectedBuild.cpu.imageUrl,
                productOfferUrl: selectedBuild.cpu.productOfferUrl,
                offerId: selectedBuild.cpu.offerId
            } : null,

            gpu: selectedBuild.gpu ? {
                componentId: selectedBuild.gpu.id,
                price: selectedBuild.gpu.price,
                storeName: selectedBuild.gpu.storeName,
                storeLogoUrl: selectedBuild.gpu.storeLogoUrl || selectedBuild.gpu.imageUrl,
                productOfferUrl: selectedBuild.gpu.productOfferUrl,
                offerId: selectedBuild.gpu.offerId
            } : null,

            motherboard: selectedBuild.motherboard ? {
                componentId: selectedBuild.motherboard.id,
                price: selectedBuild.motherboard.price,
                storeName: selectedBuild.motherboard.storeName,
                storeLogoUrl: selectedBuild.motherboard.storeLogoUrl || selectedBuild.motherboard.imageUrl,
                productOfferUrl: selectedBuild.motherboard.productOfferUrl,
                offerId: selectedBuild.motherboard.offerId
            } : null,

            powerSupply: selectedBuild.powerSupply ? {
                componentId: selectedBuild.powerSupply.id,
                price: selectedBuild.powerSupply.price,
                storeName: selectedBuild.powerSupply.storeName,
                storeLogoUrl: selectedBuild.powerSupply.storeLogoUrl || selectedBuild.powerSupply.imageUrl,
                productOfferUrl: selectedBuild.powerSupply.productOfferUrl,
                offerId: selectedBuild.powerSupply.offerId
            } : null,

            cpuCooler: selectedBuild.cpuCooler ? {
                componentId: selectedBuild.cpuCooler.id,
                price: selectedBuild.cpuCooler.price,
                storeName: selectedBuild.cpuCooler.storeName,
                storeLogoUrl: selectedBuild.cpuCooler.storeLogoUrl || selectedBuild.cpuCooler.imageUrl,
                productOfferUrl: selectedBuild.cpuCooler.productOfferUrl,
                offerId: selectedBuild.cpuCooler.offerId
            } : null,

            pcCase: selectedBuild.pcCase ? {
                componentId: selectedBuild.pcCase.id,
                price: selectedBuild.pcCase.price,
                storeName: selectedBuild.pcCase.storeName,
                storeLogoUrl: selectedBuild.pcCase.storeLogoUrl || selectedBuild.pcCase.imageUrl,
                productOfferUrl: selectedBuild.pcCase.productOfferUrl,
                offerId: selectedBuild.pcCase.offerId
            } : null,

            // Multi-component arrays
            rams: selectedBuild.rams ? selectedBuild.rams.map(ram => ({
                componentId: ram.id,  // Use id instead of componentId
                quantity: ram.quantity || 1,
                price: ram.price || (ram.totalPrice / (ram.quantity || 1)),
                storeName: ram.storeName,
                storeLogoUrl: ram.storeLogoUrl || ram.imageUrl,
                productOfferUrl: ram.productOfferUrl,
                offerId: ram.offerId
            })) : [],

            ssds: selectedBuild.ssds ? selectedBuild.ssds.map(ssd => ({
                componentId: ssd.id,  // Use id instead of componentId
                quantity: ssd.quantity || 1,
                price: ssd.price || (ssd.totalPrice / (ssd.quantity || 1)),
                storeName: ssd.storeName,
                storeLogoUrl: ssd.storeLogoUrl || ssd.imageUrl,
                productOfferUrl: ssd.productOfferUrl,
                offerId: ssd.offerId
            })) : [],

            hdds: selectedBuild.hdds ? selectedBuild.hdds.map(hdd => ({
                componentId: hdd.id,  // Use id instead of componentId
                quantity: hdd.quantity || 1,
                price: hdd.price || (hdd.totalPrice / (hdd.quantity || 1)),
                storeName: hdd.storeName,
                storeLogoUrl: hdd.storeLogoUrl || hdd.imageUrl,
                productOfferUrl: hdd.productOfferUrl,
                offerId: hdd.offerId
            })) : [],

            fans: selectedBuild.fans ? selectedBuild.fans.map(fan => ({
                componentId: fan.id,  // Use id instead of componentId
                quantity: fan.quantity || 1,
                price: fan.price || (fan.totalPrice / (fan.quantity || 1)),
                storeName: fan.storeName,
                storeLogoUrl: fan.storeLogoUrl || fan.imageUrl,
                productOfferUrl: fan.productOfferUrl,
                offerId: fan.offerId
            })) : []
        };

        console.log("Edit build - transformed components:", transformedComponents);

        // Store the data in localStorage for PcBuildPage to use
        localStorage.setItem('selectedComponents', JSON.stringify(transformedComponents));

        // Store build information for potential save/update
        localStorage.setItem('editingBuild', JSON.stringify({
            id: selectedBuild.id,
            name: selectedBuild.name,
            description: selectedBuild.description
        }));

        // Navigate to the PC Builder page
        navigate('/');
    };

    const openDeleteModal = (buildId) => {
        const buildToDelete = builds.find(build => build.id === buildId);
        if (buildToDelete) {
            setDeleteModal({
                isOpen: true,
                buildId: buildId,
                buildName: buildToDelete.name
            });
        }
    };

    const handleCreateNewBuild = () => {
        // Clear any existing build data from localStorage
        localStorage.removeItem('selectedComponents');
        localStorage.removeItem('editingBuild');

        // Navigate to the PC Builder page
        navigate('/');
    };

    // Fetch all user builds
    useEffect(() => {
        const fetchUserBuilds = async () => {
            try {
                setLoading(true);
                const response = await axiosPrivate.get(USER_BUILDS);
                const data = response.data;
                setBuilds(data);

                // Select first build by default if available
                if (data.length > 0) {
                    setSelectedBuildId(data[0].id);
                }
                setError(null);
            } catch (err) {
                setError('Failed to load your builds. Please try again later.');
                console.error('Error fetching user builds:', err);
            } finally {
                setLoading(false);
            }
        };

        if (auth?.accessToken) {
            fetchUserBuilds();
        } else {
            setError('You need to be logged in to view your builds.');
            setLoading(false);
        }
    }, [auth?.accessToken]);

    // Fetch selected build details
    useEffect(() => {
        if (!selectedBuildId) return;

        const fetchSelectedBuild = async () => {
            try {
                const response = await axiosPrivate.get(`${USER_BUILD}/${selectedBuildId}`);
                const data = response.data;
                setSelectedBuild(data);
            } catch (err) {
                console.error(`Error fetching build ${selectedBuildId}:`, err);
                // Don't set main error since we still have the builds list
            }
        };
        fetchSelectedBuild();
    }, [selectedBuildId]);

    // Handle build selection
    const handleSelectBuild = (buildId) => {
        setSelectedBuildId(buildId);
        console.log('Selected Build: ', selectedBuild)
    };

    if (loading) {
        return <div className={styles['loading']}>Loading your builds...</div>;
    }

    if (error) {
        return (
            <div className={styles['error-container']}>
                <div className={styles['error']}>{error}</div>
                {!auth?.accessToken && (
                    <div className={styles['login-prompt']}>
                        <Link to="/login" className={styles['login-link']}>Log in</Link> to see your saved builds.
                    </div>
                )}
            </div>
        );
    }

    if (builds.length === 0) {
        return (
            <div className={styles['no-builds']}>
                <h2>You don't have any saved builds yet</h2>
                <p>Go to the <Link to="/">PC Builder</Link> to create your first build!</p>
            </div>
        );
    }

    return (
        <div className={styles['user-builds-page']}>
            {/* Add the modal */}
            <DeleteModal
                isOpen={deleteModal.isOpen}
                buildName={deleteModal.buildName}
                isDeleting={deleteStatus.loading}
                onCancel={() => setDeleteModal({ isOpen: false, buildId: null, buildName: '' })}
                onConfirm={handleDeleteBuild}
            />

            <h1>Мої Збірки</h1>
            <div className={styles['builds-container']}>
                {/* Left sidebar - builds list */}
                <div className={styles['builds-list']}>
                    <h2>Saved Builds</h2>
                    <ul>
                        {builds.map(build => (
                            <li
                                key={build.id}
                                className={`${styles['build-item']} ${selectedBuildId === build.id ? styles['selected'] : ''}`}
                                onClick={() => handleSelectBuild(build.id)}
                            >
                                <div className={styles['build-name']}>{build.name}</div>
                                <div className={styles['build-date']}>
                                    {new Date(build.updatedAt).toLocaleDateString()}
                                </div>
                                <div className={styles['build-price']}>{build.price} UAH</div>
                            </li>
                        ))}
                    </ul>
                    <button
                        onClick={handleCreateNewBuild}
                        className={styles['new-build-button']}
                    >
                        Create New Build
                    </button>
                </div>

                {/* Right content - selected build details */}
                <div className={styles['build-details']}>
                    {selectedBuild ? (
                        <>
                            <div className={styles['build-header']}>
                                <h2>{selectedBuild.name}</h2>
                                <div className={styles['build-actions']}>
                                    <button
                                        className={styles['edit-button']}
                                        onClick={handleEditBuild}
                                    >
                                        Edit Build
                                    </button>
                                    <button
                                        className={styles['delete-button']}
                                        onClick={() => openDeleteModal(selectedBuildId)}
                                    >
                                        Delete Build
                                    </button>
                                </div>
                            </div>
                            {deleteStatus.error && (
                                <div className={styles['error-message']}>
                                    {deleteStatus.error}
                                </div>
                            )}
                            <div className={styles['build-meta']}>
                                <div>Created: {new Date(selectedBuild.createdAt).toLocaleDateString()}</div>
                                {selectedBuild.updatedAt && (
                                    <div>Updated: {new Date(selectedBuild.updatedAt).toLocaleDateString()}</div>
                                )}
                                <div className={styles['build-total']}>
                                    Total Price: {selectedBuild.price} UAH
                                </div>
                            </div>

                            {selectedBuild.description && (
                                <div className={styles['build-description']}>
                                    <h3>Description</h3>
                                    <p>{selectedBuild.description}</p>
                                </div>
                            )}

                            <div className={styles['build-components']}>
                                <h3>Components</h3>
                                <table className={styles['components-table']}>
                                    <thead>
                                    <tr>
                                        <th>Type</th>
                                        <th>Component</th>
                                        <th>Store</th>
                                        <th>Price</th>
                                    </tr>
                                    </thead>
                                    <tbody>
                                    {/* Render single components */}
                                    {selectedBuild.cpu && (
                                        <tr>
                                            <td>CPU</td>
                                            <td className={styles['component-cell']}>
                                                <div className={styles['component-info']}>
                                                    {selectedBuild.cpu.imageUrl && (
                                                        <img
                                                            src={selectedBuild.cpu.imageUrl}
                                                            alt={selectedBuild.cpu.name}
                                                        />
                                                    )}
                                                    <div className={styles['component-name']}>
                                                        {selectedBuild.cpu.name}
                                                    </div>
                                                </div>
                                            </td>
                                            <td>
                                                {selectedBuild.cpu ? (
                                                    <div className={styles['store-info']}>
                                                        <span>{selectedBuild.cpu.storeName}</span>
                                                    </div>
                                                ) : (
                                                    <span>Average Price</span>
                                                )}
                                            </td>
                                            <td>
                                                {selectedBuild.cpu.price}
                                            </td>
                                        </tr>
                                    )}

                                    {/* Add similar rows for other single components */}
                                    {selectedBuild.gpu && (
                                        <tr>
                                            <td>GPU</td>
                                            <td className={styles['component-cell']}>
                                                {/* Similar structure as CPU */}
                                            </td>
                                            <td>{/* Store info */}</td>
                                            <td>{/* Price info */}</td>
                                        </tr>
                                    )}

                                    {/* Add rows for motherboard, powerSupply, cpuCooler, pcCase */}

                                    {/* Render multi-components (RAM, SSD, HDD, Fan) */}
                                    {selectedBuild.rams?.length > 0 && selectedBuild.rams.map((ram, index) => (
                                        <tr key={`ram-${index}`}>
                                            <td>{index === 0 ? 'RAM' : ''}</td>
                                            <td className={styles['component-cell']}>
                                                <div className={styles['component-info']}>
                                                    {ram.imageUrl && (
                                                        <img
                                                            src={ram.imageUrl}
                                                            alt={ram.name}
                                                        />
                                                    )}
                                                    <div>
                                                        <div className={styles['component-name']}>
                                                            {ram.name}
                                                        </div>
                                                        <div className={styles['quantity']}>
                                                            Quantity: {ram.quantity}
                                                        </div>
                                                    </div>
                                                </div>
                                            </td>
                                            <td>
                                                <div className={styles['store-info']}>
                                                    <span>{ram.storeName}</span>
                                                </div>
                                            </td>
                                            <td>
                                                {ram.totalPrice}
                                            </td>
                                        </tr>
                                    ))}

                                    {/* Add similar sections for SSD, HDD, and Fan multi-components */}

                                    </tbody>
                                </table>
                            </div>
                        </>
                    ) : (
                        <div className={styles['no-build-selected']}>
                            Select a build from the list to view details
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
}

export default UserBuildsPage;