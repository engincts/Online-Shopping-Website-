
Grup Üyeleri:
Engin Çetintaş-170422026
Tankut Arca Can-171422009

# 🛍️ Online Shopping Website

Bu proje, **ASP.NET Core MVC** ile geliştirilmiş modern bir e-ticaret web uygulamasıdır. Kullanıcılar ürünleri görüntüleyebilir, sepete ekleyebilir, adres seçerek sipariş verebilir. Ayrıca sipariş iptali, iade talebi ve favorilere ekleme gibi gelişmiş özellikler içerir. Admin paneli ile ürün ve sipariş yönetimi kolayca yapılabilir.

---

## 🚀 Özellikler

- 🛒 Ürün listeleme ve detay sayfası
- ➕ Sepete ürün ekleme / çıkarma
- ❤️ Favorilere (wishlist) ürün ekleme
- 👤 Kullanıcı kayıt / giriş (authentication)
- 📍 Adres yönetimi ve siparişte adres seçimi
- ✅ Sipariş oluşturma ve PDF fatura oluşturma
- ❌ Sipariş iptali, 🔁 iade isteği gönderme
- 📦 Sipariş durumuna göre stok azaltma/arttırma
- ⚙️ Admin paneli ile ürün / sipariş yönetimi
- 📊 Dashboard’da toplam gelir ve aylık grafik

---

## 🔧 Kullanılan Teknolojiler

- ASP.NET Core MVC (.NET 6+)
- Entity Framework Core (Code First)
- MS SQL Server
- **QuestPDF** – PDF fatura oluşturma
- Bootstrap 5, jQuery, FontAwesome
- HTML, CSS, Razor View

---

Kurulum

1. Repozitoyu klonlayın:

git clone https://github.com/engincts/Online-Shopping-Website-.git

2.Visual Studio ile açın.

3.appsettings.json dosyasındaki veritabanı bağlantısını kendi bilgilerinize göre güncelleyin:

"ConnectionStrings": {
  "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=OnlineShopping;Trusted_Connection=True;TrustServerCertificate=True;"
}

4.Migration oluştur ve veritabanını ayağa kaldır:

Add-Migration InitialCreate
Update-Database

5.Gerekirse seed dataları ekleyin, ardından projeyi çalıştırın 


🛠 Admin Girişi
Admin kullanıcıya IsAdmin = true atanmalıdır.

Admin panelde:

Ürün ekleme/güncelleme

Sipariş durumu güncelleme

İade / iptal takibi

Stok takibi yapılabilir

🔐 Geliştirici Notları
MVC mimarisiyle katmanlı yapı uygulanmıştır.

Razor View üzerinden her kullanıcıya göre farklı arayüz sunulur.

Mail gönderimi, PDF işlemleri async olarak optimize edilmiştir.

Öne çıkan ürünler carousel ile gösterilir.

📄 Lisans
Bu proje MIT Lisansı ile açık kaynak olarak sunulmuştur. Detaylar için LICENSE dosyasına göz atın.
