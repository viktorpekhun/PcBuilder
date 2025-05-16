import './App.css'
import {Routes, Route} from "react-router-dom";
import Layout from "./components/Layout.jsx";
import PcBuildPage from "./pages/PcBuildPage/PcBuildPage.jsx";
import ComponentsPage from "./pages/ComponentsPage/ComponentsPage.jsx";

function App() {

    return(
        <Routes>
            <Route path="/" element={<Layout />}>
                <Route index element={<PcBuildPage />} />
                <Route path="components/:type" element={<ComponentsPage />} />
            </Route>
        </Routes>
    );
}

export default App
