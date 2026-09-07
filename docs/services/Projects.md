# Projects

## الغرض (Purpose)
إدارة المشاريع المرتبطة بشركات الإنتاج.

## الـ Entities المرتبطة
- `Project` (Name, Description, StartDate, EndDate, ProductionCompanyId)

## الـ Endpoints
| Method | Route | الوصف |
|---|---|---|

> لسا ما انبنت الـ Controllers.

## قواعد البزنس (Business Rules)
لا شي معقد بالنسخة الحالية — الجدول مقصود يكون بسيط لحد ما ينحسم Service Request.

## الـ Dependencies
- `ProductionCompany` (required)

## أسئلة مفتوحة / غير محسومة
- **متعمّد الاستبعاد من هاد الجدول حاليًا**: ربط المشروع بالجمعية (Association)، والمواقع (ProjectLocation)، وأي حالة موافقة/Status — كل هاد مؤجل لحد ما ينحسم موضوع Service Request والتوجيه الجغرافي التلقائي. **لا تُبنى** `ProjectAssociation` أو `ProjectAssociationLocation` قبل ما ينحسم هاد الموضوع.
- **ProjectWorker** (تعيين عامل لمشروع + أجره اليومي `DailyWageAmount`) مؤجل بنفس السبب — راجع `Workers.md`.
