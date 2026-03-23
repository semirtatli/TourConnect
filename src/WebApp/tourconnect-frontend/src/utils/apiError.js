// API'den gelen hata yanıtını okunabilir metne çevirir.
// İki farklı format olabilir:
//   1. FluentValidation: { errors: { "DiscountedPrice": ["..."] } }
//   2. ExceptionMiddleware: { detail: "..." }
export async function parseError(response) {
  try {
    const body = await response.json()

    // FluentValidation hataları — errors objesi içinde alan bazlı mesajlar
    if (body.errors) {
      const messages = Object.values(body.errors).flat()
      return messages.join(' • ')
    }

    // Middleware hatası — tek satır açıklama
    if (body.detail) return body.detail

    // Hiçbiri değilse title'ı göster
    return body.title ?? 'Bilinmeyen hata'
  } catch {
    return 'Sunucuya bağlanılamadı'
  }
}
