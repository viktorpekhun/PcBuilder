import './App.css'
import {Routes, Route} from "react-router-dom";
import Layout from "./components/Layout.jsx";
import PcBuildPage from "./pages/PcBuildPage/PcBuildPage.jsx";
import ComponentsPage from "./pages/ComponentsPage/ComponentsPage.jsx";
import ComponentPage from "./pages/ComponentPage/ComponentPage.jsx";
import Navbar from "./components/Navbar/Navbar.jsx";
import Footer from "./components/Footer/Footer.jsx";

function App() {

    return(
        <>
            <Navbar/>
            <div>
                <Routes>
                    <Route path="/" element={<Layout />}>
                        <Route index element={<PcBuildPage />} />
                        <Route path="components/:type" element={<ComponentsPage />} />
                        <Route path="components/:type/:id" element={<ComponentPage />} />
                    </Route>
                </Routes>
            </div>
            <Footer/>
        </>
    );
}

export default App
