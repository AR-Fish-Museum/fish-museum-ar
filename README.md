# 🐟 AR Balık Müzesi

**AR Balık Müzesi**, Unity 6 ile geliştirilen, artırılmış gerçeklik destekli ve oyunlaştırılmış öğrenme odaklı bir mobil eğitim uygulaması prototipidir.

Proje; öğrencilerin sınıf/PIN ile giriş yaparak öğretmen tarafından oluşturulan soruları çözmesini, quiz sonucunda balık galerisine geçmesini ve seçtiği balığı artırılmış gerçeklik ortamında görüntülemesini sağlar.

---

## 🎯 Proje Amacı

Bu projenin temel amacı, öğrenciler ve müze ziyaretçileri için daha etkileşimli, görsel ve oyunlaştırılmış bir öğrenme deneyimi sunmaktır.

Kullanıcılar:

* Öğrenci veya öğretmen rolüyle giriş yapabilir.
* Öğrenciler sınıf/PIN ile quiz sistemine katılabilir.
* Öğretmenler sınıf oluşturabilir ve sınıfa özel sorular ekleyebilir.
* Öğrenciler öğretmenin hazırladığı soruları çözebilir.
* Quiz sonunda puan, doğru ve yanlış sayılarını görebilir.
* Balık Galerisi sahnesine geçerek balık seçebilir.
* Seçtiği balığı AR sahnesinde hareketli şekilde görüntüleyebilir.
* AR sahnesinde telefonu hareket ettirerek balığı takip edebilir.

---

## 🚀 Mevcut Durum

Proje şu anda çalışan bir MVP seviyesine ulaşmıştır.

Tamamlanan temel özellikler:

* Unity projesi oluşturuldu.
* Android platform ayarları yapıldı.
* AR Foundation ve ARCore XR Plugin kurulumu tamamlandı.
* XR Simulation ile editör içi AR test ortamı hazırlandı.
* AR sahnesi oluşturuldu.
* Öğrenci / Öğretmen giriş ayrımı yapıldı.
* Supabase bağlantısı kuruldu.
* Öğretmen paneli oluşturuldu.
* Öğretmen panelinden sınıf oluşturma ve soru yönetimi eklendi.
* Öğrencinin sınıf seçerek quiz çözmesi sağlandı.
* Quiz sonucu ekranı oluşturuldu.
* Balık Galerisi sahnesi oluşturuldu.
* Köpekbalığı ve Palyaço Balığı kartları eklendi.
* Balık kartlarından AR sahnesine geçiş sistemi kuruldu.
* Seçilen balığın AR sahnesinde görünmesi sağlandı.
* AR sahnesinde balıkların görünmez bir 3D alan içinde hareket etmesi sağlandı.
* AR sahnesine geri butonu eklendi.
* Balık Galerisi sahnesine geri dönüş akışı eklendi.
* Android cihazda build ve test yapıldı.
* Supabase üzerinde soru ekleme, listeleme, düzenleme ve silme sistemi çalışır hale getirildi.

---

## 🧭 Uygulama Akışı

### Öğrenci Akışı

```text
Uygulama açılır
→ Öğrenci seçilir
→ İsim Soyisim + PIN girilir
→ Sınıf seçilir
→ Quiz başlar
→ Sorular cevaplanır
→ Quiz sonucu gösterilir
→ Balık Galerisine Git butonuna basılır
→ Balık Galerisi sahnesi açılır
→ Balık seçilir
→ AR sahnesine geçilir
→ Seçilen balık AR ortamında hareket eder
```

### Öğretmen Akışı

```text
Uygulama açılır
→ Öğretmen seçilir
→ Öğretmen PIN girilir
→ Öğretmen paneli açılır
→ Sınıf adı girilir
→ Soru ve şıklar oluşturulur
→ Doğru cevap seçilir
→ Soru Supabase veritabanına kaydedilir
→ Mevcut sorular listelenebilir
→ Sorular düzenlenebilir veya silinebilir
```

---

## 🛠️ Kullanılan Teknolojiler

| Teknoloji                 | Açıklama                                  |
| ------------------------- | ----------------------------------------- |
| Unity 6.3 LTS             | Ana oyun motoru                           |
| Unity 6000.3.11f1         | Kullanılan Unity sürümü                   |
| C#                        | Script geliştirme dili                    |
| AR Foundation             | Platform bağımsız AR altyapısı            |
| ARCore XR Plugin          | Android AR desteği                        |
| XR Simulation             | Telefonsuz editör içi AR testi            |
| Universal Render Pipeline | Mobil uyumlu render altyapısı             |
| UI Toolkit                | Login, quiz ve öğretmen paneli arayüzleri |
| Unity UI                  | AR ve galeri sahnelerindeki bazı butonlar |
| TextMeshPro               | UI yazıları                               |
| Supabase                  | Veritabanı ve REST API altyapısı          |
| Android Build Support     | Android cihazlara build almak için        |

---

## 🗂️ Sahne Yapısı

Projede kullanılan temel sahneler:

| Sahne             | Görev                                                           |
| ----------------- | --------------------------------------------------------------- |
| `LoginScene`      | Öğrenci / Öğretmen giriş ekranı, quiz ekranı ve öğretmen paneli |
| `00_Bootstrap`    | Başlangıç / yönlendirme sahnesi                                 |
| `01_AR_Museum`    | AR balık görüntüleme sahnesi                                    |
| `02_Webcam_Test`  | Webcam / test sahnesi                                           |
| `03_Fish_Gallery` | Balık Galerisi sahnesi                                          |

---

## 📁 Proje Klasör Yapısı

```text
Assets/
├── _Project/
│   ├── Art/
│   ├── Editor/
│   ├── Prefabs/
│   ├── Scenes/
│   │   ├── 00_Bootstrap.unity
│   │   ├── 01_AR_Museum.unity
│   │   ├── 02_Webcam_Test.unity
│   │   ├── 03_Fish_Gallery.unity
│   │   └── LoginScene.unity
│   ├── ScriptableObjects/
│   └── Scripts/
│       ├── AR/
│       ├── Core/
│       ├── Data/
│       ├── UI/
│       └── Utilities/
├── Scripts/
│   ├── Core/
│   ├── Data/
│   └── UI/
├── UI/
│   ├── LoginScreens.uxml
│   ├── LoginScreens.uss
│   ├── MainAppScreen.uxml
│   └── MainAppScreen.uss
├── Resources/
├── Settings/
└── Underwater life deluxe/
```

---

## 🧩 Önemli Scriptler

### `SceneLoader.cs`

Sahneler arası geçişleri merkezi olarak yönetir.

Temel görevleri:

* Login sahnesine geçiş
* AR sahnesine geçiş
* Fish Gallery sahnesine geçiş
* Önceki sahne bilgisini tutma
* AR sahnesinden geri dönüş akışını destekleme

---

### `GameSession.cs`

Uygulama boyunca öğrencinin oturum bilgisini tutar.

Tuttuğu bilgiler:

* Öğrenci adı
* Sınıf ID
* Sınıf adı
* Misafir durumu
* Puan
* Doğru cevap sayısı
* Yanlış cevap sayısı
* Cevaplanan soru sayısı
* Quiz sonucu ekranına dönüş durumu

---

### `LoginScreensController.cs`

LoginScene içindeki giriş akışını yönetir.

Desteklenen akışlar:

* Öğrenci girişi
* Öğretmen girişi
* PIN kontrolü
* Sınıf listeleme
* Öğrenci oturumu başlatma
* Quiz ekranına geçiş
* Öğretmen panelini açma

---

### `TeacherPanelController.cs`

Öğretmen panelindeki soru yönetimini sağlar.

Temel görevleri:

* Sınıf adı ile sınıf arama
* Sınıf yoksa yeni sınıf oluşturma
* Soru ekleme
* Soru listeleme
* Soru düzenleme
* Soru silme / pasifleştirme
* Supabase ile veri alışverişi

---

### `SupabaseClient.cs`

Unity ile Supabase REST API arasında bağlantı kurar.

Desteklenen işlemler:

* `GET`
* `POST`
* `PATCH`

Özellikler:

* Supabase URL ve anon key yönetimi
* Android build için fallback URL/key desteği
* Authorization header yönetimi
* Hata durumunda response body loglama
* Schema ve RLS hatalarını debug etmeye uygun yapı

---

### `MainAppScreenController.cs`

Quiz ekranını ve quiz sonucu ekranını yönetir.

Temel görevleri:

* Sınıfa ait soruları Supabase’den çekme
* Soruları sırayla gösterme
* Cevap kontrolü
* Puan hesaplama
* Doğru / yanlış sayısını takip etme
* Quiz sonucu ekranı oluşturma
* Balık Galerisine geçiş butonu oluşturma
* Quiz’e baştan başlama
* Sıralama ekranını gösterme

---

### `QuestionData.cs`

Supabase’den gelen soru verisini temsil eder.

İçerdiği alanlar:

* Soru ID
* Sınıf ID
* Soru metni
* Şıklar
* Doğru cevap
* Aktiflik durumu
* Oluşturulma tarihi

---

### `ClassData.cs`

Supabase’deki sınıf verisini temsil eder.

İçerdiği alanlar:

* Sınıf ID
* Sınıf adı
* Öğrenci PIN
* Öğretmen PIN
* Arşiv durumu
* Oluşturulma tarihi

---

### `SelectedFishSession.cs`

Balık Galerisi veya quiz sonucu üzerinden seçilen balığı AR sahnesine taşır.

Tuttuğu bilgiler:

* Seçilen balık ID’si
* Seçilen balık adı
* Seçilen balık prefabı

---

### `FishGalleryCardButton.cs`

Balık Galerisi sahnesindeki kartların tıklanabilir olmasını sağlar.

Temel görevleri:

* Kart tıklanınca balık seçimini kaydetme
* `SelectedFishSession` içine seçilen balığı yazma
* AR sahnesine geçiş yapma

---

### `RewardFishSpawner.cs`

AR sahnesinde seçilen balığı oluşturur.

Temel özellikleri:

* `SelectedFishSession` üzerinden seçilen balığı alır.
* Seçilen balık yoksa fallback prefab kullanır.
* Balığı `RewardFish` layer’ına alır.
* Overlay Camera sistemi ile balığın görünür olmasını sağlar.
* Balığı görünmez bir 3D akvaryum alanı içine yerleştirir.
* Balık türüne göre scale ve rotation offset uygular.

---

### `RewardFishSwimmer.cs`

AR sahnesindeki balığın görünmez akvaryum alanı içinde hareket etmesini sağlar.

Temel görevleri:

* Balık için rastgele hedef noktalar seçme
* Balığı hedefe doğru hareket ettirme
* Balığın yönünü gittiği yöne çevirme
* Hareket hızını doğal aralıkta değiştirme

---

### `ARBackButtonController.cs`

AR sahnesinde geri butonu oluşturur.

Temel görevleri:

* AR sahnesine ekran üstü geri butonu ekleme
* Geri basıldığında Balık Galerisi sahnesine dönme
* Gerekirse seçilen balık bilgisini temizleme

---

### `FishGalleryBackButtonController.cs`

Balık Galerisi sahnesindeki geri butonunu yönetir.

Temel görevleri:

* Balık Galerisi’nden quiz sonucu ekranına dönüş isteği oluşturma
* LoginScene’e dönüp MainApp ekranının sonuç görünümünü açmasını sağlama

---

## 🐠 Balık Sistemi

Projede şu anda iki balık desteklenmektedir:

| Balık          | ID                 | Prefab                     |
| -------------- | ------------------ | -------------------------- |
| Köpekbalığı    | `reward_shark`     | `Great_white_shark_prefab` |
| Palyaço Balığı | `reward_clownfish` | `Clownfish_prefab`         |

Balıklar `SelectedFishSession` üzerinden seçilir ve AR sahnesinde `RewardFishSpawner` tarafından oluşturulur.

Her balık için ayrı ayarlar yapılabilir:

* Scale
* Rotation offset
* Prefab
* Kart görseli
* Balık adı
* Açıklama

---

## 🐟 Balık Galerisi

Balık Galerisi sahnesi, öğrencinin quiz sonrası balık seçmesini sağlar.

Mevcut özellikler:

* Sualtı temalı arka plan
* Köpekbalığı kartı
* Palyaço Balığı kartı
* Kartlara tıklayınca AR sahnesine geçiş
* Geri butonu ile quiz sonuç ekranına dönüş

Akış:

```text
Quiz sonucu
→ Balık Galerisine Git
→ Balık seç
→ AR sahnesi
```

---

## 🌊 AR Sahnesi

AR sahnesinde seçilen balık artırılmış gerçeklik ortamında gösterilir.

Mevcut özellikler:

* AR kamera desteği
* ARCore / AR Foundation altyapısı
* Overlay Camera render çözümü
* `RewardFish` layer sistemi
* Balıkların kamera child’ı olmadan dünya içinde konumlanması
* Görünmez 3D akvaryum alanı
* Balıkların bu alan içinde rastgele hedeflere yüzmesi
* Telefon hareket ettirilince balığın ekrandan çıkabilmesi
* Kamera tekrar balığa yöneltilince balığın yeniden görünmesi
* AR sahnesinden Balık Galerisi’ne geri dönüş butonu

---

## 🧠 Quiz Sistemi

Quiz sistemi Supabase’den sınıfa özel soruları çeker.

Öğrenci:

* İsim Soyisim girer.
* PIN girer.
* Sınıf seçer.
* Soruları çözer.
* Her cevap sonrası doğru/yanlış kontrolü yapılır.
* Quiz sonunda sonuç ekranını görür.

Sonuç ekranında:

* Toplam puan
* Doğru sayısı
* Yanlış sayısı
* Cevaplanan soru sayısı
* Balık Galerisine Git butonu
* Quiz’e Baştan Başla butonu
* Sıralamayı Gör butonu

bulunur.

---

## 👨‍🏫 Öğretmen Paneli

Öğretmen paneli, öğretmenin sınıfa özel sorular oluşturmasını sağlar.

Özellikler:

* Öğretmen PIN ile giriş
* Sınıf adı girme
* Yeni sınıf oluşturma
* Mevcut sınıfı bulma
* Soru metni girme
* A/B/C/D şıkları girme
* Doğru cevabı seçme
* Soruyu kaydetme
* Sınıfa ait soruları listeleme
* Soruları düzenleme
* Soruları silme / pasifleştirme

Varsayılan değerler:

```text
Öğretmen PIN: 1234
Öğrenci PIN: 1111
```

---

## 🗄️ Supabase Veritabanı

Projede Firebase yerine Supabase kullanılmaktadır.

Kullanılan temel tablolar:

### `classes`

Sınıf bilgilerini tutar.

Temel kolonlar:

```text
id
class_name
student_pin
teacher_pin
is_archived
created_at
expires_at
```

### `questions`

Sınıfa ait soruları tutar.

Temel kolonlar:

```text
id
class_id
question_text
options
correct_option
is_active
created_at
```

`options` alanı JSONB formatındadır.

Örnek:

```json
{
  "a": "Cevap A",
  "b": "Cevap B",
  "c": "Cevap C",
  "d": "Cevap D"
}
```

### `analytics_answers`

Öğrenci cevap analizleri için planlanan/ kullanılan tablodur.

Temel amaç:

* Öğrenci cevaplarını kaydetmek
* Hangi soruya hangi cevabın verildiğini tutmak
* İleride öğretmen panelinde analiz göstermek

---

## 🔐 Supabase Güvenlik Notu

Bu proje şu anda MVP / prototip seviyesindedir.

Unity tarafında Supabase publishable/anon key kullanılmaktadır. Bu nedenle Supabase güvenliği RLS politikaları üzerinden yönetilmelidir.

MVP sürecinde öğretmen panelinin çalışması için `classes` ve `questions` tablolarında `anon` rolüne belirli `select`, `insert` ve `update` izinleri verilmiştir.

Gerçek üretim ortamında önerilen yapı:

* Supabase Auth kullanmak
* Öğretmen hesabı oluşturmak
* Soru ekleme/düzenleme işlemlerini yetkili kullanıcıya bağlamak
* Gerekirse Supabase Edge Functions kullanmak
* Client tarafında öğretmen PIN’i tutmamak

---

## 🧪 Test Durumu

Proje hem Unity Editor içinde hem de gerçek Android cihazlarda test edilmiştir.

Test edilen başlıca akışlar:

```text
Öğrenci girişi
Sınıf seçimi
Quiz çözme
Quiz sonucu görme
Balık Galerisine geçme
Köpekbalığı seçme
Palyaço Balığı seçme
AR sahnesinde balık görüntüleme
AR sahnesinde balığın hareket etmesi
AR sahnesinden geri dönme
Balık Galerisi’nden sonuç ekranına dönme
Öğretmen girişi
Soru ekleme
Soru listeleme
Soru düzenleme
Soru silme
Android build alma
```

Android tarafında test edilen konular:
<<<<<<< HEAD

* Kamera izni
* AR sahnesi açılışı
* Balık görünürlüğü
* Balık materyalleri
* UI butonları
* Supabase bağlantısı
* Öğrenci/öğretmen akışları

---

## 🎬 Proje Tanıtım Videosu

AR Balık Müzesi projesinin öğrenci girişi, öğretmen paneli, quiz sistemi, balık galerisi ve AR sahnesi akışı aşağıdaki tanıtım videosunda gösterilmektedir.

[![AR Balık Müzesi Tanıtım Videosu](docs/Ekran görüntüsü 2026-06-25 172623.png)](https://www.youtube.com/watch?v=oxRk6NoB1nU)
=======

* Kamera izni
* AR sahnesi açılışı
* Balık görünürlüğü
* Balık materyalleri
* UI butonları
* Supabase bağlantısı
* Öğrenci/öğretmen akışları
>>>>>>> 918ce9e4c61ad20c5280edf9e1602430d28c237b

---

## 📱 Hedef Platform

İlk hedef platform:

```text
Android / ARCore
```

İlerleyen aşamalarda:

```text
iOS / ARKit
```

desteği eklenebilir.

Şu anda geliştirme ve test odağı Android üzerindedir.

---

## ⚙️ Kurulum

Projeyi klonla:

```bash
git clone https://github.com/BBaglars/fish_museum_automation.git
```

Proje klasörüne gir:

```bash
cd fish_museum_automation
```

Unity Hub üzerinden projeyi aç:

```text
Unity Version: 6000.3.11f1
```

Gerekli paketler Unity tarafından `Packages/manifest.json` üzerinden yüklenecektir.

---

## ▶️ Unity İçinde Çalıştırma

Unity içinde başlangıç için şu sahne açılabilir:

```text
Assets/_Project/Scenes/LoginScene.unity
```

Play tuşuna basıldığında uygulama giriş ekranından başlar.

Test edilecek ana akış:

```text
LoginScene
→ Öğrenci
→ Sınıf seçimi
→ Quiz
→ Quiz sonucu
→ Balık Galerisi
→ AR sahnesi
```

---

## 📦 Android Build Alma

Unity içinde:

```text
File
→ Build Profiles / Build Settings
→ Android
→ Build And Run
```

Gerekli cihaz ayarları:

* Developer Options açık olmalı.
* USB Debugging açık olmalı.
* USB bağlantı modu File Transfer olmalı.
* Telefonda kamera izni verilmelidir.
* ARCore destekli cihaz önerilir.

---

## 🧰 Teknik Notlar

* Gerçek ARCore testi için Android cihaz gereklidir.
* Laptop kamerası ARCore yerine geçmez.
* Telefonsuz geliştirme için XR Simulation kullanılabilir.
* Android build için Supabase URL/key fallback alanları Inspector’dan ayarlanabilir.
* Supabase secret/service role key client içinde kullanılmamalıdır.
* Unity `Library`, `Temp`, `Obj`, `Build` gibi klasörler repoya dahil edilmemelidir.
* Büyük model ve texture dosyaları gerekirse Git LFS ile yönetilebilir.
* Shader/material uyumsuzluklarında URP materyal düzeltmesi gerekebilir.
* AR sahnesinde balıkların görünmesi için `RewardFish` layer sistemi kullanılmaktadır.

---

## 🗺️ Geliştirme Yol Haritası

### Faz 1 — AR Temel Deneyim

* [x] Unity projesi oluşturma
* [x] Android platform ayarı
* [x] AR Foundation kurulumu
* [x] ARCore ayarı
* [x] XR Simulation kurulumu
* [x] AR sahnesi oluşturma
* [x] Plane Detection ekleme
* [x] Raycast ile yüzeye obje yerleştirme
* [x] TestFish prefabı oluşturma
* [x] Gerçek Android cihazda test

### Faz 2 — Giriş ve Rol Sistemi

* [x] Öğrenci / Öğretmen rol seçimi
* [x] Öğrenci isim + PIN girişi
* [x] Öğretmen PIN girişi
* [x] Sınıf seçimi
* [x] Oturum bilgilerinin `GameSession` içinde tutulması

### Faz 3 — Supabase Entegrasyonu

* [x] Supabase bağlantısı
* [x] Android fallback URL/key sistemi
* [x] `classes` tablosundan sınıf çekme
* [x] `questions` tablosundan soru çekme
* [x] Yeni sınıf oluşturma
* [x] Yeni soru ekleme
* [x] Soru güncelleme
* [x] Soru silme / pasifleştirme
* [x] Supabase RLS policy düzenlemeleri

### Faz 4 — Öğretmen Paneli

* [x] Öğretmen paneli UI
* [x] Sınıf adı girme
* [x] Soru metni girme
* [x] A/B/C/D şıkları
* [x] Doğru cevap seçimi
* [x] Soruyu kaydetme
* [x] Soruları listeleme
* [x] Soruları düzenleme
* [x] Soruları silme

### Faz 5 — Quiz Sistemi

* [x] Soru veri modeli oluşturma
* [x] Supabase’den sınıfa özel soru çekme
* [x] Quiz paneli hazırlama
* [x] Çoktan seçmeli cevap sistemi
* [x] Doğru / yanlış cevap kontrolü
* [x] Puan hesaplama
* [x] Quiz sonucu ekranı
* [x] Quiz’den Balık Galerisi’ne geçiş

### Faz 6 — Balık Galerisi

* [x] Balık Galerisi sahnesi
* [x] Sualtı temalı arka plan
* [x] Köpekbalığı kartı
* [x] Palyaço Balığı kartı
* [x] Karttan AR sahnesine geçiş
* [x] Galeriden quiz sonucuna geri dönüş

### Faz 7 — AR Ödül Balığı Sistemi

* [x] Seçilen balığı AR sahnesine taşıma
* [x] `SelectedFishSession` sistemi
* [x] `RewardFishSpawner` sistemi
* [x] Overlay Camera çözümü
* [x] `RewardFish` layer sistemi
* [x] Köpekbalığı ve Palyaço Balığı scale ayarları
* [x] Balık materyal / shader düzeltmeleri
* [x] Görünmez 3D akvaryum alanı
* [x] Balıkların alan içinde hareket etmesi
* [x] Balık türüne göre rotation offset
* [x] AR sahnesinden geri dönüş butonu

### Faz 8 — Mobil Test ve Build

* [x] Android build ayarları
* [x] USB debugging ile cihaz bağlantısı
* [x] Gerçek cihazda build testleri
* [x] Kamera izni testi
* [x] Supabase bağlantı testi
* [x] AR sahnesi görünürlük testi
* [x] UI buton testleri

### Faz 9 — Planlanan Geliştirmeler

* [ ] Yeni balık türleri ekleme
* [ ] Balık bilgi kartları
* [ ] Balığa dokununca tepki animasyonu
* [ ] Balık kaybolduğunda yönlendirme mesajı
* [ ] Öğretmen paneli tasarım iyileştirmesi
* [ ] Soru silmeden önce onay ekranı
* [ ] Öğrenci cevap analizleri
* [ ] Sınıf bazlı skor tablosu
* [ ] Daha gelişmiş admin/öğretmen yetkilendirmesi
* [ ] Offline mod
* [ ] 2D fallback modu
* [ ] iOS / ARKit desteği

---

## 🧱 Bilinen Sınırlamalar

* Öğretmen PIN sistemi şu anda prototip seviyesindedir.
* Supabase işlemleri client üzerinden yapılmaktadır.
* Gerçek üretim ortamı için daha güçlü authentication sistemi gerekir.
* AR deneyimi cihazın ARCore desteğine bağlıdır.
* Bazı Android cihazlarda ARCore desteği sınırlı olabilir.
* Balık sayısı şu an iki model ile sınırlıdır.
* Balık bilgi kartları henüz tamamlanmamıştır.
* Detaylı öğrenci analiz paneli henüz tamamlanmamıştır.

---

## 👥 Geliştiriciler

* Berkay BAĞLARS — 245541020
* Aziz BOLAT — 225541123
* Yunus GÜÇLÜ — 225541089
* Mert Kaan KİNDAR — 225541039

---

## 📄 Lisans

Bu proje geliştirme aşamasındadır.

Lisans bilgisi ilerleyen sürümlerde netleştirilecektir.
