import { useState, useEffect } from 'react'
import { Link } from 'react-router-dom'

const API = import.meta.env.VITE_API_URL

const categoryLabels = {
  0: 'Tekne Turu',
  1: 'Safari',
  2: 'Dalış',
  3: 'Kültür',
  4: 'Macera',
  5: 'Yemek',
}

export default function HomePage() {
  const [deals, setDeals] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)

  useEffect(() => {
    fetch(`${API}/api/deals`)
      .then(r => r.json())
      .then(data => { setDeals(data); setLoading(false) })
      .catch(() => { setError('API\'ye bağlanılamadı'); setLoading(false) })
  }, [])

  if (loading) return <p className="text-gray-500">Yükleniyor...</p>

  if (error) return (
    <div className="p-4 bg-red-50 border border-red-300 text-red-800 rounded-lg">
      {error}
    </div>
  )

  return (
    <div>
      <h1 className="text-2xl font-bold mb-6">Aktif Son Dakika Fırsatları</h1>

      {deals.length === 0 && (
        <p className="text-gray-500">
          Şu an aktif fırsat yok.{' '}
          <Link to="/operator" className="text-blue-600 hover:underline">
            Fırsat eklemek için tıklayın →
          </Link>
        </p>
      )}

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        {deals.map(deal => (
          <div key={deal.id} className="border rounded-lg p-4 shadow-sm bg-white flex flex-col gap-3">
            {/* Başlık ve kategori */}
            <div className="flex justify-between items-start">
              <h2 className="font-semibold text-lg">{deal.tour?.title}</h2>
              <span className="text-xs bg-blue-100 text-blue-700 px-2 py-1 rounded">
                {categoryLabels[deal.tour?.category] ?? 'Tur'}
              </span>
            </div>

            {/* Fiyat */}
            <div className="flex items-center gap-2">
              <span className="line-through text-gray-400 text-sm">{deal.originalPrice} ₺</span>
              <span className="text-green-600 font-bold text-xl">{deal.discountedPrice} ₺</span>
              <span className="text-xs text-green-500">
                %{Math.round((1 - deal.discountedPrice / deal.originalPrice) * 100)} indirim
              </span>
            </div>

            {/* Detaylar */}
            <div className="text-sm text-gray-600 flex flex-col gap-1">
              <span>Kalan yer: <strong>{deal.availableSlots}</strong></span>
              <span>Son tarih: {new Date(deal.expiresAt).toLocaleString('tr-TR')}</span>
            </div>

            {/* Rezervasyon butonu — otel sayfasına yönlendirir */}
            <Link
              to="/hotel"
              className="mt-auto bg-green-600 text-white text-center rounded px-4 py-2 hover:bg-green-700"
            >
              Rezervasyon Yap
            </Link>
          </div>
        ))}
      </div>
    </div>
  )
}
