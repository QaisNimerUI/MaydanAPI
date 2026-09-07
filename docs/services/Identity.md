# Identity (Users / Roles / Permissions / Groups)

## الغرض (Purpose)
إدارة اليوزرات، الأدوار، الصلاحيات والمجموعات، وحساب الصلاحيات الفعلية (Effective Permissions) لكل يوزر. هاد الـ Service هو الأساس يلي بيبنى عليه كل الـ Authorization بالنظام.

## الـ Entities المرتبطة
- `User`, `Role`, `Permission`, `Group`
- `RolePermission`, `GroupPermission`, `UserGroup`, `UserPermission` (جداول ربط)
- `EntityType` (enum: Bayt / Aseza / ProductionCompany / Association)

## الـ Endpoints
| Method | Route | الوصف |
|---|---|---|
| POST | `/api/Auth/login` | تسجيل الدخول، يرجع access token + refresh token |
| POST | `/api/Auth/refresh-token` | تجديد الـ access token |
| POST | `/api/Auth/logout` | إبطال الـ refresh token |
| POST | `/api/Auth/change-password` | تغيير كلمة المرور لليوزر الحالي |
| POST | `/api/Auth/register-{role}` | تسجيل يوزر جديد حسب الرول (بحاجة تفصيل شكل كل رول) |
| GET/POST/PUT/DELETE | `/api/Users` | CRUD لليوزرات |

> لسا ما انبنت الـ Controllers فعليًا — هاي القائمة موثّقة من القسم 5 بالـ Brief كإلزام تسمية.

## قواعد البزنس (Business Rules)
- **الصلاحيات الفعلية (Effective Permissions)** = اتحاد:
  `RolePermissions[user.RoleId] ∪ (GroupPermissions لكل Group منضم إلها اليوزر) ∪ UserPermissions[user.UserId]`
- الاتحاد بينحسب عند الـ Login (JWT claims) أو Server-side مع caching — القرار النهائي لسا مفتوح (شوف تحت).
- `User.EntityType` + `User.EntityId` بيحددوا الجهة الفعلية يلي اليوزر تابعلها (Association/ProductionCompany/...) — بدونهم ما فيه طريقة تعرف تبعية اليوزر.
- `User.PasswordHash` ينحفظ كـ hash فقط، وليس plain text. الآلية الحالية: `IPasswordHasher` + `PasswordHasher` باستخدام `PBKDF2-SHA256`.
- Seed user الحالي يستخدم password أولي `Abc@123` محفوظ كـ generated hash داخل `UserSeedConfiguration`.

## الـ Dependencies
لا شي — هاد أول Service بينبني، وكل الـ Services التانية (Associations, ProductionCompanies, Projects...) رح تعتمد عليه للـ Authorization.

## أسئلة مفتوحة / غير محسومة
- **مكان حساب الـ Effective Permissions**: JWT claims وقت الـ Login مقابل Server-side + caching — لسا مش محسوم (مذكور صراحة بالقسم 4).
- **شكل `register-{role}` endpoints**: كل رول شكله مختلف بالتسجيل، بس التفاصيل لسا ما وصلت.
- **Refresh Token storage**: وين بينخزن (DB جدول منفصل؟ Redis؟) ومدة صلاحيته بالتفصيل.
- **شو آلية إدارة الأسرار (`Jwt:Key`, `Security:CivilIdHashKey`) لبيئتي QA وStage؟** حاليًا بيئة Dev بتعتمد على `dotnet user-secrets` محليًا بس، وهاد مش حل يصلح لـ QA/Stage (بيئات مشتركة، مش جهاز مطوّر واحد). لازم يتحدد: Azure Key Vault؟ متغيرات بيئة على مستوى السيرفر/الـ pipeline؟ ومين المسؤول عن توليدهم وتدويرهم (rotation) لكل بيئة.
