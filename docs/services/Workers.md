# Workers

## الغرض (Purpose)
إدارة العمال المرتبطين بالجمعيات، وتوليد الـ QR code الخاص فيهم.

## الـ Entities المرتبطة
- `Worker` (FirstName, MiddleName, LastName, CivilId, CivilIdHash, DateOfBirth, PhoneNumber, AssociationId, QrCode, IsActive)

## الـ Endpoints
| Method | Route | الوصف |
|---|---|---|

> لسا ما انبنت الـ Controllers.

## قواعد البزنس (Business Rules)
- **CivilId**: حقل واحد موحّد (يلغي `civilainId`/`civilianId`/`civilainIdNumber` القديمة)، required، **مشفّر عند التخزين بشكل عشوائي (non-deterministic) وبدون unique constraint عليه مباشرة**.
- **CivilIdHash (Blind Index)**: `HMAC-SHA256(CivilId)` بمفتاح سري على مستوى السيرفر (`ICivilIdHasher` بـ `Maydan.Application`، تطبيقه الفعلي `HmacCivilIdHasher` بـ `Maydan.Infrastructure`، المفتاح من `Security:CivilIdHashKey` عبر user-secrets/env var — نفس منطق `Jwt:Key`). **هاد العمود هو يلي عليه الـ unique index والبحث** (`IWorkerRepository.GetByCivilIdHashAsync`)، مش `CivilId` نفسه.
- **QrCode**: بيتولّد تلقائيًا عند إنشاء العامل، unique.
- `AssociationId` required — كل عامل مرتبط بجمعية واحدة بأي وقت (افتراض، شوف تحت).

## الـ Dependencies
- `Association` (required)

## أسئلة مفتوحة / غير محسومة
- **هل رقم الهوية الوطنية (CivilId) الخاص بعامل تم حذفه (Soft Delete) قابل لإعادة الاستخدام لعامل جديد؟** الـ Soft Delete (`IsDeleted`) بيخلي سجل العامل القديم موجود بالجدول (بس مستبعد من الاستعلامات عبر الـ query filter)، والـ unique index على `CivilIdHash` رح يمنع تسجيل نفس رقم الهوية من جديد طالما السجل القديم موجود — حتى لو العامل "محذوف" منطقيًا. لازم قرار من البزنس: هل هاي الحالة مقصودة (منع إعادة الاستخدام نهائيًا)، أو لازم آلية استثناء (مثلاً استرجاع السجل القديم بدل إنشاء جديد، أو hard-delete بعد فترة احتفاظ معيّنة)؟
- **تشفير CivilId الفعلي (AES أو ما يعادله) لسا ما انبنى** — العمود مصمم ومهيّأ (`nvarchar(256)`, بدون unique) والـ blind index (`CivilIdHash`) شغّال، بس القيمة المخزّنة بـ `CivilId` نفسها لسا plaintext لحد ما ينبنى الـ encryption layer (قرار تقني منفصل عن الـ blind index، يحتاج تصميم إدارة مفاتيح).
- **AssociationId required**: افتراض مبني على BRD (11.1) "كل عامل مرتبط بجمعية واحدة بأي وقت" — يستاهل تأكيد صريح إذا التسجيل ممكن يصير بدون جمعية أول.
- **صيغة QrCode**: Guid أو كود قصير؟ القرار يرجع لفريق الباك، لسا غير محسوم.
- **متعمّد الاستبعاد**: `DailyWageAmount` مو خاصية بالعامل — هي خاصية بربط العامل بمشروع معيّن (`ProjectWorker`)، ومؤجلة مع منظومة Service Request.
