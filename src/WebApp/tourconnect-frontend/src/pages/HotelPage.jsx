import { useState, useEffect } from 'react'
import { parseError } from '../utils/apiError'

const API = import.meta.env.VITE_API_URL

export default function HotelPage() {
  const [deals, setDeals] = useState([])
  const [partners, setPartners] = useState([])
  const [message, setMessage] = useState(null)

  function refreshDeals() {
    fetch(`${API}/api/deals`).then(r => r.json()).then(setDeals)
  }

  useEffect(() => {
    refreshDeals()
    fetch(`${API}/api/partners`).then(r => r.json()).then(setPartners)
  }, [])

  async function makeReservation(e) {
    e.preventDefault()
    const f = e.target
    const r = await fetch(`${API}/api/reservations`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        dealId: f.dealId.value,
        partnerId: f.partnerId.value,
        guestName: f.guestName.value,
        guestCount: parseInt(f.guestCount.value),
      }),
    })
    if (r.ok) {
      setMessage({ type: 'success', text: 'Rezervasyon onaylandı!' })
      refreshDeals()
      f.reset()
    } else {
      setMessage({ type: 'error', text: await parseError(r) })
    }
  }

  async function createPartner(e) {
    e.preventDefault()
    const f = e.target
    const r = await fetch(`${API}/api/partners`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ name: f.name.value, contactEmail: f.email.value, location: f.location.value }),
    })
    if (r.ok) {
      setMessage({ type: 'success', text: 'Otel kaydedildi!' })
      fetch(`${API}/api/partners`).then(r => r.json()).then(setPartners)
      f.reset()
    } else {
      setMessage({ type: 'error', text: await parseError(r) })
    }
  }

  return (
    <div className="max-w-xl mx-auto flex flex-col gap-8">
      <h1 className="text-2xl font-bold">Otel Paneli</h1>

      {/* Bildirim — her zaman görünür, kapatma butonu var */}
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

      {/* Otel kaydı */}
      <section className="border rounded-lg p-4">
        <h2 className="font-semibold mb-4">Otel Kaydı</h2>
        <form onSubmit={createPartner} className="flex flex-col gap-3">
          <input name="name" placeholder="Otel adı" required className="border rounded px-3 py-2" />
          <input name="email" type="email" placeholder="E-posta" required className="border rounded px-3 py-2" />
          <input name="location" placeholder="Konum" required className="border rounded px-3 py-2" />
          <button className="bg-blue-600 text-white rounded px-4 py-2 hover:bg-blue-700">Kaydet</button>
        </form>
      </section>

      {/* Rezervasyon formu */}
      <section className="border rounded-lg p-4">
        <h2 className="font-semibold mb-4">Rezervasyon Yap</h2>
        <form onSubmit={makeReservation} className="flex flex-col gap-3">
          <select name="dealId" required className="border rounded px-3 py-2">
            <option value="">Fırsat seçin...</option>
            {deals.map(d => (
              <option key={d.id} value={d.id}>
                {d.tour?.title} — {d.discountedPrice}₺ ({d.availableSlots} yer kaldı)
              </option>
            ))}
          </select>

          <select name="partnerId" required className="border rounded px-3 py-2">
            <option value="">Oteli seçin...</option>
            {partners.map(p => (
              <option key={p.id} value={p.id}>{p.name}</option>
            ))}
          </select>

          <input name="guestName" placeholder="Misafir adı" required className="border rounded px-3 py-2" />
          <input name="guestCount" type="number" min="1" placeholder="Kişi sayısı" required className="border rounded px-3 py-2" />
          <button className="bg-green-600 text-white rounded px-4 py-2 hover:bg-green-700">Rezervasyon Yap</button>
        </form>
      </section>
    </div>
  )
}
