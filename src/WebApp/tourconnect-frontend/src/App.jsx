import { BrowserRouter, Routes, Route, Link } from 'react-router-dom'
import HomePage from './pages/HomePage'
import OperatorPage from './pages/OperatorPage'
import HotelPage from './pages/HotelPage'

export default function App() {
  return (
    <BrowserRouter>
      {/* Üst navigasyon çubuğu */}
      <nav className="bg-blue-600 text-white px-6 py-4 flex gap-6 items-center">
        <span className="font-bold text-lg">TourConnect</span>
        <Link to="/" className="hover:underline">Fırsatlar</Link>
        <Link to="/operator" className="hover:underline">Operatör</Link>
        <Link to="/hotel" className="hover:underline">Otel</Link>
      </nav>

      <div className="p-6">
        <Routes>
          <Route path="/" element={<HomePage />} />
          <Route path="/operator" element={<OperatorPage />} />
          <Route path="/hotel" element={<HotelPage />} />
        </Routes>
      </div>
    </BrowserRouter>
  )
}
