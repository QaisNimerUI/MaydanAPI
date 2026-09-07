# Associations (الجمعيات)

## الغرض (Purpose)
إدارة الجمعيات ويوزراتها. كل جمعية مرتبطة بمدينة (v1 مبسّط) ولها عمالها الخاصين.

## الـ Entities المرتبطة
- `Association` (ArabicName, EnglishName, CityId, LocationOnGoogleMaps, Latitude, Longitude, ContactPhone, ContactEmail)

## الـ Endpoints
| Method | Route | الوصف |
|---|---|---|

> لسا ما انبنت الـ Controllers.

## قواعد البزنس (Business Rules)
- **WorkersCount ليس عمود مخزّن** — لازم يترجع كـ `COUNT(Workers WHERE AssociationId = ...)` بالـ DTO وقت الطلب، مش عمود بالجدول.
- `Latitude`/`Longitude` نقطة مرجعية فقط — مش بديل عن حدود التغطية (Polygon) لما توصل.

## الـ Dependencies
- `City` (الموقع v1 المبسّط)
- `Worker` (كل عامل مرتبط بجمعية واحدة)
- `User.EntityType = Association` + `User.EntityId` (تبعية يوزرات الجمعية — من الموديل الأساسي بالبريف)

## أسئلة مفتوحة / غير محسومة
- **ContactPhone / ContactEmail**: مو موجودين بموديل الفرونت الحالي — إضافة بناءً على BRD (FR-2.1) "contact details"، يستاهل تأكيد.
- حدود التغطية الجغرافية (Polygon/GIS) لسا مؤجلة.
