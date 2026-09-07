# ProductionCompanies (شركات الإنتاج)

## الغرض (Purpose)
إدارة شركات الإنتاج ويوزراتها، وربطها بالمشاريع (Projects).

## الـ Entities المرتبطة
- `ProductionCompany` (ArabicName, EnglishName, Description, ContactPhone, ContactEmail, IsSelfRegistered)

## الـ Endpoints
| Method | Route | الوصف |
|---|---|---|

> لسا ما انبنت الـ Controllers.

## قواعد البزنس (Business Rules)
- `IsSelfRegistered` بيميّز الشركة يلي سجلت حالها ذاتيًا (تسجيل ذاتي) عن يلي أنشأها الأدمن — قد يؤثر على منطق الموافقة لاحقًا (لسا غير محسوم).

## الـ Dependencies
- `Project` (كل مشروع مرتبط بشركة إنتاج واحدة)
- `User.EntityType = ProductionCompany` + `User.EntityId`

## أسئلة مفتوحة / غير محسومة
- **ContactPhone / ContactEmail**: نفس منطق Association — إضافة يستاهل تأكيد.
- هل `IsSelfRegistered = true` بيتطلب خطوة موافقة إضافية (Admin approval) قبل ما تصير الشركة فعالة؟ غير محسوم.
