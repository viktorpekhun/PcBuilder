import './App.css'
import {Routes, Route} from "react-router-dom";
import Layout from "./components/Layout.jsx";
import Components from "./components/Components.jsx";

function App() {

    return(
        <Routes>
            <Route path="/" element={<Layout />}>
                <Route index element={<Components type={'Cpu'}/>} />
            </Route>
        </Routes>
    );
}

export default App
