import { useState, useEffect } from 'react'
import { parseError } from '../utils/apiError'

const API = import.meta.env.VITE_API_URL

const categories = [
  { value: 0, label: 'Tekne Turu' },
  { value: 1, label: 'Safari' },
  { value: 2, label: 'Dalış' },
  { value: 3, label: 'Kültür' },
  { value: 4, label: 'Macera' },
  { value: 5, label: 'Yemek' },
]

export default function OperatorPage() {
  const [operators, setOperators] = useState([])
  const [tours, setTours] = useState([])
  const [message, setMessage] = useState(null)

  function refresh() {
    fetch(`${API}/api/operators`).then(r => r.json()).then(setOperators)
    fetch(`${API}/api/tours`).then(r => r.json()).then(setTours)
  }

  useEffect(() => { refresh() }, [])

  async function submit(url, body, successMsg) {
    const r = await fetch(`${API}${url}`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(body),
    })
    if (r.ok) {
      setMessage({ type: 'success', text: successMsg })
      refresh()
    } else {
      setMessage({ type: 'error', text: await parseError(r) })
    }
  }

  return (
    <div className="max-w-xl mx-auto flex flex-col gap-8">
      <h1 className="text-2xl font-bold">Operatör Paneli</h1>

      {message && (
        <div className={`p-4 rounded-lg border flex justify-between items-start ${
          message.type === 'success'
            ? 'bg-green-50 border-green-300 text-green-800'
            : 'bg-red-50 border-red-300 text-red-800'
        }`}>
          <span>{message.text}</span>
          <button onClick={() => setMessage(null)} className="ml-4 font-bold text-lg leading-none">×</button>
        </div>
      )}

      {/* 1. Operatör oluştur */}
      <section className="border rounded-lg p-4">
        <h2 className="font-semibold mb-4">1. Operatör Oluştur</h2>
        <form onSubmit={e => {
          e.preventDefault()
          const f = e.target
          submit('/api/operators', { name: f.name.value, phone: f.phone.value, location: f.location.value }, 'Operatör oluşturuldu!')
          f.reset()
        }} className="flex flex-col gap-3">
          <input name="name" placeholder="Ad" required className="border rounded px-3 py-2" />
          <input name="phone" placeholder="Telefon" required className="border rounded px-3 py-2" />
          <input name="location" placeholder="Konum" required className="border rounded px-3 py-2" />
          <button className="bg-blue-600 text-white rounded px-4 py-2 hover:bg-blue-700">Kaydet</button>
        </form>
      </section>

      {/* 2. Tur oluştur */}
      <section className="border rounded-lg p-4">
        <h2 className="font-semibold mb-4">2. Tur Oluştur</h2>
        <form onSubmit={e => {
          e.preventDefault()
          const f = e.target
          submit('/api/tours', {
            operatorId: f.operatorId.value,
            title: f.title.value,
            description: f.description.value,
            category: parseInt(f.category.value),
            durationInHours: parseInt(f.duration.value),
            basePrice: parseFloat(f.basePrice.value),
          }, 'Tur oluşturuldu!')
          f.reset()
        }} className="flex flex-col gap-3">
          <select name="operatorId" required className="border rounded px-3 py-2">
            <option value="">Operatör seçin...</option>
            {operators.map(op => (
              <option key={op.id} value={op.id}>{op.name}</option>
            ))}
          </select>
          <input name="title" placeholder="Tur adı" required className="border rounded px-3 py-2" />
          <input name="description" placeholder="Açıklama" required className="border rounded px-3 py-2" />
          <select name="category" required className="border rounded px-3 py-2">
            {categories.map(c => <option key={c.value} value={c.value}>{c.label}</option>)}
          </select>
          <input name="duration" type="number" min="1" placeholder="Süre (saat)" required className="border rounded px-3 py-2" />
          <input name="basePrice" type="number" step="0.01" placeholder="Baz fiyat (₺)" required className="border rounded px-3 py-2" />
          <button className="bg-blue-600 text-white rounded px-4 py-2 hover:bg-blue-700">Tur Oluştur</button>
        </form>
      </section>

      {/* 3. Fırsat yayınla */}
      <section className="border rounded-lg p-4">
        <h2 className="font-semibold mb-4">3. Fırsat Yayınla</h2>
        <form onSubmit={e => {
          e.preventDefault()
          const f = e.target
          submit('/api/deals', {
            tourId: f.tourId.value,
            availableSlots: parseInt(f.slots.value),
            originalPrice: parseFloat(f.originalPrice.value),
            discountedPrice: parseFloat(f.discountedPrice.value),
            expiresAt: new Date(f.expiresAt.value).toISOString(),
          }, 'Fırsat yayınlandı!')
          f.reset()
        }} className="flex flex-col gap-3">
          <select name="tourId" required className="border rounded px-3 py-2">
            <option value="">Tur seçin...</option>
            {tours.map(t => (
              <option key={t.id} value={t.id}>{t.title}</option>
            ))}
          </select>
          <input name="slots" type="number" min="1" placeholder="Kalan yer sayısı" required className="border rounded px-3 py-2" />
          <input name="originalPrice" type="number" step="0.01" placeholder="Normal fiyat (₺)" required className="border rounded px-3 py-2" />
          <input name="discountedPrice" type="number" step="0.01" placeholder="İndirimli fiyat (₺)" required className="border rounded px-3 py-2" />
          <input name="expiresAt" type="datetime-local" required className="border rounded px-3 py-2" />
          <button className="bg-green-600 text-white rounded px-4 py-2 hover:bg-green-700">Fırsatı Yayınla</button>
        </form>
      </section>

      {/* Özet listeler */}
      <section>
        <h2 className="font-semibold mb-2">Operatörler ({operators.length})</h2>
        {operators.map(op => (
          <div key={op.id} className="border rounded px-3 py-2 mb-2 text-sm text-gray-700">
            <strong>{op.name}</strong> — {op.location}
          </div>
        ))}
      </section>

      <section>
        <h2 className="font-semibold mb-2">Turlar ({tours.length})</h2>
        {tours.map(t => (
          <div key={t.id} className="border rounded px-3 py-2 mb-2 text-sm text-gray-700">
            <strong>{t.title}</strong> — {t.basePrice}₺
          </div>
        ))}
      </section>
    </div>
  )
}
