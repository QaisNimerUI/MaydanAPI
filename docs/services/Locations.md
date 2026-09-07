# Locations (Country / City)

## الغرض (Purpose)
مرجع جغرافي مبسّط (v1) — دولة ومدينة كنقطة/مرجع إداري، تستخدمهم Association كموقعها الرئيسي.

## الـ Entities المرتبطة
- `Country` (ArabicName, EnglishName)
- `City` (ArabicName, EnglishName, CountryId)

## الـ Endpoints
| Method | Route | الوصف |
|---|---|---|

> لسا ما انبنت الـ Controllers.

## قواعد البزنس (Business Rules)
لا شي معقد حاليًا — بيانات مرجعية بسيطة.

## الـ Dependencies
لا شي. `Association` بتعتمد على `City`.

## أسئلة مفتوحة / غير محسومة
- **Polygon/GIS**: هاد v1 مبسّط بس. لما توصل صيغة بيانات الـ GIS النهائية، رح يضاف عمود جغرافي منفصل (مش تعديل على `City`/`Country`).
