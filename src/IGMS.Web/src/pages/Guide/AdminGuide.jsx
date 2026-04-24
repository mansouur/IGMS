import { useState } from 'react'

// ── مكونات مساعدة ────────────────────────────────────────────────────────────

function Section({ id, title, icon, children, defaultOpen = false }) {
  const [open, setOpen] = useState(defaultOpen)
  return (
    <div id={id} className="bg-white rounded-xl border border-gray-200 overflow-hidden">
      <button
        onClick={() => setOpen(o => !o)}
        className="w-full flex items-center justify-between px-5 py-4 text-start hover:bg-gray-50 transition-colors">
        <div className="flex items-center gap-3">
          <span className="text-xl">{icon}</span>
          <span className="font-bold text-gray-800">{title}</span>
        </div>
        <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"
          className={`text-gray-400 transition-transform ${open ? 'rotate-180' : ''}`}>
          <polyline points="6 9 12 15 18 9"/>
        </svg>
      </button>
      {open && <div className="px-5 pb-5 space-y-4 border-t border-gray-100">{children}</div>}
    </div>
  )
}

function Steps({ items }) {
  return (
    <ol className="space-y-3 mt-3">
      {items.map((item, i) => (
        <li key={i} className="flex gap-3">
          <span className="flex-shrink-0 w-6 h-6 rounded-full bg-blue-600 text-white text-xs font-bold flex items-center justify-center mt-0.5">
            {i + 1}
          </span>
          <div>
            <p className="text-sm font-medium text-gray-800">{item.title}</p>
            {item.detail && <p className="text-xs text-gray-500 mt-0.5">{item.detail}</p>}
            {item.path && (
              <p className="text-xs text-blue-600 mt-0.5 font-mono bg-blue-50 px-2 py-0.5 rounded inline-block">
                {item.path}
              </p>
            )}
          </div>
        </li>
      ))}
    </ol>
  )
}

function Impact({ items }) {
  const colors = { high: 'bg-emerald-50 border-emerald-200 text-emerald-800', medium: 'bg-amber-50 border-amber-200 text-amber-800', info: 'bg-blue-50 border-blue-200 text-blue-800' }
  return (
    <div className="space-y-2 mt-3">
      {items.map((item, i) => (
        <div key={i} className={`border rounded-lg px-3 py-2 text-xs flex items-start gap-2 ${colors[item.level ?? 'info']}`}>
          <span className="mt-0.5 flex-shrink-0">{item.level === 'high' ? '✅' : item.level === 'medium' ? '⚠️' : 'ℹ️'}</span>
          <span>{item.text}</span>
        </div>
      ))}
    </div>
  )
}

function Scenario({ title, context, steps, result }) {
  return (
    <div className="border border-blue-100 rounded-xl p-4 bg-blue-50/50 mt-3">
      <p className="text-xs text-blue-500 font-semibold mb-1">سيناريو عملي</p>
      <p className="text-sm font-bold text-gray-800 mb-1">{title}</p>
      {context && <p className="text-xs text-gray-500 mb-3">{context}</p>}
      <Steps items={steps} />
      {result && (
        <div className="mt-3 bg-emerald-50 border border-emerald-200 rounded-lg px-3 py-2 text-xs text-emerald-800">
          <span className="font-bold">النتيجة: </span>{result}
        </div>
      )}
    </div>
  )
}

function Note({ children, type = 'info' }) {
  const cfg = {
    info:    { cls: 'bg-blue-50 border-blue-200 text-blue-800',   icon: 'ℹ️' },
    warning: { cls: 'bg-amber-50 border-amber-200 text-amber-800',icon: '⚠️' },
    tip:     { cls: 'bg-emerald-50 border-emerald-200 text-emerald-800', icon: '💡' },
  }[type]
  return (
    <div className={`border rounded-lg px-3 py-2 text-xs flex gap-2 mt-2 ${cfg.cls}`}>
      <span>{cfg.icon}</span>
      <span>{children}</span>
    </div>
  )
}

// ── جدول المحتويات ─────────────────────────────────────────────────────────

const TOC = [
  { id: 'overview',    label: 'نظرة عامة على النظام'       },
  { id: 'first-steps', label: 'الخطوات الأولى للإعداد'     },
  { id: 'policies',    label: 'السياسات والامتثال'          },
  { id: 'risks',       label: 'إدارة المخاطر'               },
  { id: 'raci',        label: 'مصفوفة RACI'                 },
  { id: 'kpi',         label: 'مؤشرات الأداء KPI'           },
  { id: 'tasks',       label: 'المهام والمتابعة'             },
  { id: 'controls',    label: 'اختبارات الضوابط'            },
  { id: 'assessments', label: 'الاستبيانات والتقييم'        },
  { id: 'incidents',   label: 'إدارة الحوادث'               },
  { id: 'vendors',     label: 'إدارة الموردين'              },
  { id: 'meetings',    label: 'الاجتماعات'                  },
  { id: 'performance', label: 'تقييم الأداء HPMS'           },
  { id: 'pdpl',        label: 'حماية البيانات PDPL'         },
  { id: 'workflows',   label: 'سير العمل والاعتمادات'       },
  { id: 'roles',       label: 'الأدوار والصلاحيات'          },
  { id: 'reports',     label: 'التقارير والتحليلات'         },
]

// ── الصفحة الرئيسية ─────────────────────────────────────────────────────────

export default function AdminGuide() {
  return (
    <div className="max-w-5xl">

      {/* Header */}
      <div className="mb-6">
        <div className="flex items-center gap-2 mb-1">
          <span className="text-xs bg-emerald-100 text-emerald-700 px-2 py-0.5 rounded-full font-medium">للمسؤولين فقط</span>
        </div>
        <h1 className="text-xl font-bold text-gray-800">دليل استخدام نظام IGMS</h1>
        <p className="text-sm text-gray-500 mt-1">
          دليل شامل لكيفية استخدام وحدات النظام، والسيناريوهات العملية، وأثر كل إجراء
        </p>
      </div>

      <div className="flex gap-6">

        {/* جدول المحتويات — يسار */}
        <div className="hidden lg:block w-52 flex-shrink-0">
          <div className="sticky top-4 bg-white rounded-xl border border-gray-200 p-4">
            <p className="text-xs font-bold text-gray-500 uppercase mb-3">المحتويات</p>
            <ul className="space-y-1">
              {TOC.map(t => (
                <li key={t.id}>
                  <a href={`#${t.id}`}
                    className="block text-xs text-gray-600 hover:text-blue-600 hover:bg-blue-50 px-2 py-1 rounded-lg transition-colors">
                    {t.label}
                  </a>
                </li>
              ))}
            </ul>
          </div>
        </div>

        {/* المحتوى الرئيسي */}
        <div className="flex-1 space-y-4">

          {/* ═══════════════════════════════════════════════════════════ نظرة عامة */}
          <Section id="overview" icon="🏛️" title="نظرة عامة على النظام" defaultOpen>
            <p className="text-sm text-gray-700 leading-relaxed mt-3">
              نظام IGMS (نظام الحوكمة المؤسسية) منصة متكاملة تساعد الجهات الحكومية على إدارة:
            </p>
            <div className="grid grid-cols-2 md:grid-cols-3 gap-2 mt-3">
              {[
                ['📋', 'السياسات والإجراءات'],
                ['⚠️', 'المخاطر المؤسسية'],
                ['✅', 'الضوابط والامتثال'],
                ['📊', 'مؤشرات الأداء'],
                ['🚨', 'الحوادث الأمنية'],
                ['🤝', 'إدارة الموردين'],
                ['🗓️', 'اجتماعات اللجان'],
                ['👤', 'تقييمات الأداء'],
                ['🛡️', 'حماية البيانات PDPL'],
              ].map(([icon, label]) => (
                <div key={label} className="flex items-center gap-2 bg-gray-50 rounded-lg px-3 py-2">
                  <span className="text-base">{icon}</span>
                  <span className="text-xs text-gray-700">{label}</span>
                </div>
              ))}
            </div>
            <Note type="tip">
              كل الوحدات مترابطة — مثلاً: المخاطرة تُنشئ مهمة، والسياسة ترتبط بإطار الامتثال، والحادثة تُولّد تقريراً تلقائياً.
            </Note>
          </Section>

          {/* ═══════════════════════════════════════════════ الخطوات الأولى */}
          <Section id="first-steps" icon="🚀" title="الخطوات الأولى للإعداد">
            <p className="text-sm text-gray-600 mt-3">قبل البدء باستخدام النظام، تأكد من إعداد البنية التحتية:</p>
            <Steps items={[
              { title: 'إنشاء الأقسام', detail: 'أنشئ هيكل الأقسام التنظيمية للجهة', path: 'الأقسام ← إضافة قسم' },
              { title: 'إضافة المستخدمين', detail: 'سجّل جميع موظفي النظام مع تحديد أقسامهم', path: 'المستخدمون ← مستخدم جديد' },
              { title: 'تعريف الأدوار والصلاحيات', detail: 'حدد ما يستطيع كل دور رؤيته أو تعديله', path: 'الأدوار والصلاحيات' },
              { title: 'ربط المستخدمين بالأدوار', detail: 'افتح ملف كل مستخدم وعيّن له دوره', path: 'المستخدمون ← تعديل ← الدور' },
              { title: 'إعداد سير العمل (اختياري)', detail: 'عرّف مراحل الاعتماد للسياسات والمخاطر', path: 'سير العمل ← تعريف جديد' },
            ]} />
            <Note type="info">
              بعد إعداد الأقسام والمستخدمين يمكنك البدء بأي وحدة — النظام لا يفرض ترتيباً محدداً.
            </Note>
          </Section>

          {/* ═══════════════════════════════════════════════════════ السياسات */}
          <Section id="policies" icon="📋" title="السياسات والامتثال">
            <p className="text-sm text-gray-600 mt-3">
              السياسة هي وثيقة رسمية تُلزم الجهة بإجراء معين. دورة حياتها: <strong>مسودة ← معتمدة ← مؤرشفة</strong>
            </p>

            <Scenario
              title="الوزارة تلقت سياسة جديدة للالتزام بها"
              context="مثال: صدر قرار وزاري يلزم بتطبيق سياسة أمن المعلومات"
              steps={[
                { title: 'أضف السياسة', detail: 'اكتب عنوانها ومحتواها وحدد تاريخ النفاذ والانتهاء', path: 'السياسات ← إضافة سياسة' },
                { title: 'حدد القسم المسؤول', detail: 'اختر القسم المكلّف بالتطبيق' },
                { title: 'أرفق الوثيقة الرسمية', detail: 'ارفع ملف PDF قرار الوزارة في قسم المرفقات' },
                { title: 'أرسلها للاعتماد', detail: 'اضغط "إرسال للاعتماد" لتبدأ دورة سير العمل' },
                { title: 'اربطها بإطار الامتثال', detail: 'افتح السياسة ← اربطها بضوابط ISO/NESA ذات الصلة', path: 'مكتبة الامتثال' },
                { title: 'أنشئ استبياناً للقياس', detail: 'ابنِ استبياناً لقياس مدى التزام الأقسام بالسياسة', path: 'الاستبيانات ← جديد' },
                { title: 'أنشئ مخاطر عدم الامتثال', detail: 'إذا وُجدت مخاطر من عدم التطبيق سجّلها في وحدة المخاطر' },
              ]}
              result="السياسة معتمدة ومربوطة بالامتثال، والأقسام تستقبل طلب التقييم، والمخاطر موثّقة."
            />

            <Impact items={[
              { level: 'high', text: 'بعد الاعتماد تظهر السياسة في لوحة التحكم وتُرسَل إشعارات للموظفين المرتبطين' },
              { level: 'high', text: 'يمكن تفعيل "الإقرار بالاطلاع" ليُسجَّل كل موظف اطّلع على السياسة' },
              { level: 'medium', text: 'عند اقتراب تاريخ الانتهاء يُرسل النظام تنبيهاً تلقائياً للمراجعة' },
            ]} />
          </Section>

          {/* ═══════════════════════════════════════════════════════ المخاطر */}
          <Section id="risks" icon="⚠️" title="إدارة المخاطر">
            <p className="text-sm text-gray-600 mt-3">
              المخاطرة = احتمالية × تأثير. النظام يحسب درجة الخطر تلقائياً ويصنّفها: منخفضة / متوسطة / عالية / حرجة.
            </p>

            <Scenario
              title="اكتشاف مخاطرة جديدة"
              context="مثال: اكتُشف ضعف في صلاحيات الوصول لقاعدة البيانات"
              steps={[
                { title: 'سجّل المخاطرة', detail: 'اكتب الوصف والتأثير المتوقع والقسم المرتبط', path: 'المخاطر ← إضافة مخاطرة' },
                { title: 'حدد الاحتمالية والتأثير', detail: 'من 1 إلى 5 — النظام يحسب درجة الخطر تلقائياً' },
                { title: 'عيّن المسؤول', detail: 'حدد من سيتابع المعالجة' },
                { title: 'أنشئ مهمة معالجة', detail: 'اضغط "إنشاء مهمة" من داخل المخاطرة لربطها مباشرة', path: 'داخل المخاطرة ← إنشاء مهمة' },
                { title: 'اربطها بمؤشر أداء', detail: 'إذا أثّرت على مؤشر معين اربطها به لتتبع التأثير' },
                { title: 'راجع خريطة المخاطر', detail: 'تأكد من موقع المخاطرة على الـ Heatmap', path: 'المخاطر ← عرض الخريطة' },
              ]}
              result="المخاطرة موثّقة ومربوطة بمهمة معالجة، ومسؤول المتابعة يتلقى إشعاراً فورياً."
            />

            <Impact items={[
              { level: 'high', text: 'المخاطر الحرجة تظهر باللون الأحمر في لوحة التنفيذي' },
              { level: 'high', text: 'ربط المخاطرة بمؤشر أداء يُحدّث تلقائياً تقرير العلاقة بين الأداء والمخاطر' },
              { level: 'medium', text: 'يمكن ربط المخاطرة بحادثة موجودة لتتبع مصدر المشكلة' },
            ]} />
          </Section>

          {/* ═══════════════════════════════════════════════════════════ RACI */}
          <Section id="raci" icon="📊" title="مصفوفة RACI">
            <p className="text-sm text-gray-600 mt-3">
              RACI تُوضّح من يفعل ماذا: <strong>مسؤول تنفيذاً (R) — محاسَب (A) — يُستشار (C) — يُبلَّغ (I)</strong>
            </p>
            <Steps items={[
              { title: 'أنشئ مصفوفة RACI', detail: 'اختر العملية أو المشروع الذي تريد تحديد المسؤوليات فيه', path: 'مصفوفة RACI ← جديد' },
              { title: 'أضف الأنشطة', detail: 'سرّد كل مهمة أو نشاط ضمن العملية' },
              { title: 'عيّن المشاركين', detail: 'لكل نشاط حدد دور كل موظف (R/A/C/I)' },
              { title: 'أرسل للاعتماد', detail: 'بعد الانتهاء أرسل المصفوفة لاعتماد المدير المختص' },
            ]} />
            <Impact items={[
              { level: 'high', text: 'RACI المعتمدة تُصبح مرجعاً رسمياً عند الخلاف على المسؤوليات' },
              { level: 'info', text: 'يمكن ربط RACI بسياسة أو إجراء لتوضيح من ينفّذه' },
            ]} />
          </Section>

          {/* ══════════════════════════════════════════════════════ مؤشرات الأداء */}
          <Section id="kpi" icon="📈" title="مؤشرات الأداء KPI">
            <p className="text-sm text-gray-600 mt-3">
              مؤشرات الأداء تقيس مدى تحقيق الأهداف المؤسسية بأرقام قابلة للقياس والمقارنة عبر الزمن.
            </p>

            <Scenario
              title="إعداد مؤشر لقياس الالتزام بالسياسات"
              steps={[
                { title: 'أنشئ المؤشر', detail: 'حدد الاسم، الوحدة (نسبة مئوية)، والقسم المرتبط', path: 'مؤشرات الأداء ← جديد' },
                { title: 'حدد القيم المستهدفة', detail: 'مثال: الهدف 100% التزام بالسياسات' },
                { title: 'أدخل القراءات الشهرية', detail: 'ادخل إلى المؤشر ← أضف قراءة جديدة', path: 'داخل المؤشر ← قراءة جديدة' },
                { title: 'اربطه بالمخاطر ذات الصلة', detail: 'إذا انخفض المؤشر ترتفع المخاطرة المرتبطة تلقائياً' },
              ]}
              result="يظهر المؤشر في لوحة التنفيذي بالألوان: أخضر (محقق) — أصفر (تحذير) — أحمر (حرج)."
            />

            <Impact items={[
              { level: 'high', text: 'المؤشرات تُغذّي لوحة التنفيذي وتقرير الأداء المؤسسي تلقائياً' },
              { level: 'medium', text: 'الانخفاض الحاد في مؤشر مرتبط بمخاطرة يُغيّر تصنيف المخاطرة' },
            ]} />
          </Section>

          {/* ══════════════════════════════════════════════════════ المهام */}
          <Section id="tasks" icon="✅" title="المهام والمتابعة">
            <p className="text-sm text-gray-600 mt-3">
              المهام أداة التنفيذ — كل إجراء يحتاج متابعة يُحوَّل لمهمة مع مسؤول وموعد نهائي.
            </p>
            <Steps items={[
              { title: 'أنشئ مهمة', detail: 'حدد العنوان والأولوية والمسؤول وتاريخ الاستحقاق', path: 'المهام ← مهمة جديدة' },
              { title: 'اربطها بمصدرها', detail: 'يمكن ربطها بمخاطرة أو حادثة أو سياسة' },
              { title: 'تابع التقدم', detail: 'المسؤول يُحدّث الحالة: معلقة ← قيد التنفيذ ← مكتملة' },
            ]} />
            <Impact items={[
              { level: 'high', text: 'المهام المتأخرة تُرسل إشعارات تلقائية للمسؤول والمدير' },
              { level: 'info', text: 'المهام المرتبطة بمخاطرة تُعيّن حالة المعالجة تلقائياً عند إكمالها' },
            ]} />
          </Section>

          {/* ═══════════════════════════════════════════ اختبارات الضوابط */}
          <Section id="controls" icon="🔍" title="اختبارات الضوابط">
            <p className="text-sm text-gray-600 mt-3">
              الضابط هو إجراء رقابي — اختباره يُثبت أنه يعمل بفاعلية. النتيجة: فعّال / ضعيف / فاشل.
            </p>

            <Scenario
              title="اختبار ضابط الوصول لقاعدة البيانات"
              steps={[
                { title: 'أنشئ اختبار ضابط', detail: 'حدد اسم الضابط والقسم والتكرار (شهري/سنوي)', path: 'اختبارات الضوابط ← جديد' },
                { title: 'سجّل الإجراء المُتبَع', detail: 'اكتب كيف تحققت من فاعلية الضابط' },
                { title: 'ارفع الأدلة', detail: 'ارفع لقطات شاشة أو تقارير كدليل على الاختبار' },
                { title: 'حدد النتيجة', detail: 'فعّال / ضعيف / فاشل — مع تفسير' },
              ]}
              result="الاختبار موثّق بأدلته وتاريخه، وجاهز للعرض على المراجع الخارجي."
            />

            <Impact items={[
              { level: 'high', text: 'الضوابط الفاشلة تُنشئ توصية تلقائية بمعالجة المخاطرة المرتبطة' },
              { level: 'high', text: 'نتائج الضوابط تُحسب في تقرير الامتثال ضمن مكتبة ISO/NESA' },
            ]} />
          </Section>

          {/* ══════════════════════════════════════════════ الاستبيانات */}
          <Section id="assessments" icon="📝" title="الاستبيانات والتقييم">
            <p className="text-sm text-gray-600 mt-3">
              الاستبيانات أداة لقياس مدى الامتثال أو جمع بيانات من الأقسام. دورة حياتها: مسودة ← منشور ← مغلق.
            </p>

            <Scenario
              title="قياس الالتزام بسياسة أمن المعلومات"
              steps={[
                { title: 'أنشئ الاستبيان', detail: 'اكتب الأسئلة — يدعم: نعم/لا، نص حر، تقييم رقمي', path: 'الاستبيانات ← جديد' },
                { title: 'انشر الاستبيان', detail: 'بعد إضافة الأسئلة اضغط "نشر" لتفعيله' },
                { title: 'الأقسام تُجيب', detail: 'كل قسم يفتح الاستبيان ويسجّل إجاباته' },
                { title: 'اغلق الاستبيان', detail: 'عند انتهاء الموعد اضغط "إغلاق" لوقف الإجابات' },
                { title: 'استعرض التقرير', detail: 'تقرير تلقائي يُلخّص الإجابات والنسب', path: 'داخل الاستبيان ← التقرير' },
              ]}
              result="تحصل على تقرير مرئي بنسبة الامتثال لكل قسم، جاهز لرفعه للإدارة العليا."
            />
          </Section>

          {/* ══════════════════════════════════════════════ الحوادث */}
          <Section id="incidents" icon="🚨" title="إدارة الحوادث">
            <p className="text-sm text-gray-600 mt-3">
              الحادثة = أي طارئ يُؤثر على العمليات. دورة حياتها: مفتوحة ← قيد المراجعة ← محلولة ← مغلقة.
            </p>

            <Scenario
              title="حادثة تسرّب بيانات"
              steps={[
                { title: 'سجّل الحادثة فوراً', detail: 'حدد الخطورة (حرجة) وتاريخ الاكتشاف والوصف', path: 'الحوادث ← حادثة جديدة' },
                { title: 'اربطها بالمخاطرة المرتبطة', detail: 'إذا كانت مخاطرة التسرّب موثّقة مسبقاً اربطها' },
                { title: 'أنشئ مهمة معالجة', detail: 'عيّن فريق الاستجابة وحدد موعد الحل' },
                { title: 'سجّل التحقيق', detail: 'وثّق الأسباب والتأثير والإجراءات المتخذة' },
                { title: 'أغلق الحادثة', detail: 'بعد المعالجة اضغط "حلّ الحادثة" مع ملاحظات الإغلاق' },
              ]}
              result="الحادثة موثّقة بالكامل مع سجل زمني، وجاهزة للمراجعة من جهات الرقابة."
            />

            <Impact items={[
              { level: 'high', text: 'الحوادث الحرجة تُرسل إشعاراً فورياً للمدراء' },
              { level: 'high', text: 'يمكن تصدير تقرير الحوادث بصيغة Excel للتحليل' },
              { level: 'medium', text: 'ربط الحادثة بمخاطرة يُحدّث تلقائياً سجل المخاطرة بتاريخ التحقق الفعلي' },
            ]} />
          </Section>

          {/* ══════════════════════════════════════════════ الموردون */}
          <Section id="vendors" icon="🤝" title="إدارة الموردين">
            <p className="text-sm text-gray-600 mt-3">
              سجّل الموردين وعقودهم وقيّم مخاطرهم بشكل دوري.
            </p>

            <Scenario
              title="إضافة مورد جديد وتقييم مخاطره"
              steps={[
                { title: 'أضف المورد', detail: 'سجّل الاسم والنوع ومعلومات التواصل وفترة العقد', path: 'الموردون ← جديد' },
                { title: 'سجّل الامتثال القانوني', detail: 'حدد ما إذا كان لديه: NDA، اتفاقية حماية بيانات، شهادة ISO' },
                { title: 'قيّم مستوى الخطر', detail: 'افتح صفحة المورد ← تقييم المخاطر ← حدد المستوى والدرجة', path: 'داخل المورد ← تقييم المخاطر' },
                { title: 'تابع تاريخ انتهاء العقد', detail: 'النظام يُنبّه تلقائياً قبل 30 يوماً من انتهاء العقد' },
              ]}
              result="سجل مورد كامل مع تتبع المخاطر والعقود — جاهز للمراجعة الدورية."
            />

            <Impact items={[
              { level: 'high', text: 'الموردون ذوو المخاطر الحرجة يظهرون بالأحمر في قائمة الموردين' },
              { level: 'medium', text: 'تنبيه تلقائي عند اقتراب انتهاء العقد أو انتهاء صلاحية الشهادات' },
            ]} />
          </Section>

          {/* ═══════════════════════════════════════════════ الاجتماعات */}
          <Section id="meetings" icon="🗓️" title="الاجتماعات">
            <p className="text-sm text-gray-600 mt-3">
              وثّق اجتماعات اللجان: جدول الأعمال، الحضور، المحضر، ونقاط العمل — كل شيء في مكان واحد.
            </p>

            <Scenario
              title="اجتماع لجنة حوكمة شهري"
              steps={[
                { title: 'جدوِل الاجتماع', detail: 'حدد التاريخ والوقت والموقع والمدعوين', path: 'الاجتماعات ← جديد' },
                { title: 'أضف جدول الأعمال', detail: 'اكتب بنود النقاش التي ستُطرح' },
                { title: 'ابدأ الاجتماع', detail: 'عند البدء اضغط "بدء الاجتماع" لتغيير الحالة إلى جارٍ' },
                { title: 'أكمل الاجتماع', detail: 'بعد الانتهاء: سجّل المحضر، حضور كل شخص، ونقاط العمل', path: 'داخل الاجتماع ← إكمال' },
                { title: 'تابع نقاط العمل', detail: 'كل نقطة عمل لها مسؤول وموعد نهائي ويمكن إغلاقها عند الإنجاز' },
              ]}
              result="الاجتماع موثّق بالكامل: محضر رسمي + حضور معتمد + نقاط عمل قابلة للمتابعة."
            />
          </Section>

          {/* ════════════════════════════════════════════ تقييم الأداء */}
          <Section id="performance" icon="👤" title="تقييم الأداء HPMS">
            <p className="text-sm text-gray-600 mt-3">
              دورة التقييم: المقيِّم يُنشئ التقييم ← يرفعه ← المدير يعتمده أو يرفضه.
            </p>

            <Scenario
              title="تقييم أداء موظف سنوي"
              steps={[
                { title: 'أنشئ التقييم', detail: 'حدد الموظف والمقيِّم والفترة (سنوي/ربعي)', path: 'تقييمات الأداء ← جديد' },
                { title: 'أضف الأهداف', detail: 'لكل هدف: العنوان، الوزن النسبي، القيمة المستهدفة' },
                { title: 'سجّل النتائج الفعلية', detail: 'بعد انتهاء الفترة أدخل القيم الفعلية وتقييم كل هدف' },
                { title: 'أضف التعليقات', detail: 'نقاط القوة، مجالات التطوير، التعليقات العامة' },
                { title: 'ارفع للاعتماد', detail: 'اضغط "رفع للاعتماد" لإرساله للمدير المختص' },
                { title: 'اعتماد أو رفض', detail: 'المدير يعتمد التقييم أو يرفضه مع ذكر السبب' },
              ]}
              result="التقييم معتمد رسمياً مع سجل كامل بالأهداف والنتائج والتعليقات."
            />

            <Impact items={[
              { level: 'high', text: 'التقييم المرجّح يحسب الدرجة الإجمالية تلقائياً من أوزان الأهداف' },
              { level: 'info', text: 'شريط التقدم لكل هدف يُظهر نسبة الإنجاز (الفعلي ÷ المستهدف)' },
            ]} />
          </Section>

          {/* ══════════════════════════════════════════════ PDPL */}
          <Section id="pdpl" icon="🛡️" title="حماية البيانات الشخصية PDPL">
            <p className="text-sm text-gray-600 mt-3">
              قانون PDPL الإماراتي يُلزم الجهات بتوثيق أنشطة معالجة البيانات والرد على طلبات الأفراد خلال <strong>30 يوماً</strong>.
            </p>

            <Scenario
              title="توثيق معالجة بيانات الموظفين"
              steps={[
                { title: 'أنشئ سجل معالجة', detail: 'اكتب الغرض من المعالجة وتصنيف البيانات والأساس القانوني', path: 'حماية البيانات ← سجل جديد' },
                { title: 'حدد فترة الاحتفاظ', detail: 'مثال: 5 سنوات من تاريخ انتهاء الخدمة' },
                { title: 'سجّل الضمانات الأمنية', detail: 'التشفير، التحكم في الوصول، النسخ الاحتياطي' },
                { title: 'وثّق المشاركة الخارجية', detail: 'إذا كانت البيانات تُشارَك مع جهات خارجية حددها' },
                { title: 'راجع السجل سنوياً', detail: 'اضغط "تسجيل مراجعة" كل سنة للتأكد من دقة البيانات' },
              ]}
            />

            <Note type="warning">
              عند تلقي أي طلب حذف أو تصحيح من موظف أو مواطن — افتح السجل ← أضف طلباً — يجب الرد خلال 30 يوماً وإلا يتحول الطلب إلى "متأخر" ويظهر بالأحمر.
            </Note>

            <Impact items={[
              { level: 'high', text: 'الطلبات المتأخرة تظهر بتحذير واضح — الجهة قد تتعرض لغرامة إذا تجاوزت المهلة' },
              { level: 'high', text: 'النقل العابر للحدود يتطلب توثيق الضمانات (قرار ملاءمة / SCCs) بموجب القانون' },
              { level: 'info', text: 'قسم "طلبات البيانات" يجمع كل الطلبات من كل السجلات في صفحة واحدة للمتابعة' },
            ]} />
          </Section>

          {/* ══════════════════════════════════════════ سير العمل */}
          <Section id="workflows" icon="🔄" title="سير العمل والاعتمادات">
            <p className="text-sm text-gray-600 mt-3">
              سير العمل يُرتّب مراحل الاعتماد — بدلاً من البريد الإلكتروني كل شيء داخل النظام.
            </p>
            <Steps items={[
              { title: 'عرّف سير العمل', detail: 'حدد الكيان (سياسة/مخاطرة/...) وعدد مراحل الاعتماد', path: 'سير العمل ← تعريف جديد' },
              { title: 'حدد المعتمدين', detail: 'لكل مرحلة حدد من يجب أن يعتمد' },
              { title: 'ابدأ دورة اعتماد', detail: 'من داخل أي سجل اضغط "إرسال للاعتماد"' },
              { title: 'تابع من بريد الاعتماد', detail: 'المعتمدون يجدون طلباتهم في "بريد الاعتماد"', path: 'بريد الاعتماد' },
            ]} />
            <Impact items={[
              { level: 'high', text: 'لا يمكن نشر سياسة أو اعتماد تقرير دون المرور بسير العمل المعرَّف' },
              { level: 'info', text: 'بريد الاعتماد يجمع كل الطلبات المعلقة للمعتمِد في مكان واحد' },
            ]} />
          </Section>

          {/* ══════════════════════════════════════════════ الأدوار */}
          <Section id="roles" icon="🔐" title="الأدوار والصلاحيات">
            <p className="text-sm text-gray-600 mt-3">
              النظام يعمل بنموذج RBAC — الصلاحيات تُمنح للدور لا للشخص مباشرة.
            </p>

            <div className="bg-gray-50 rounded-xl p-4 mt-3">
              <p className="text-xs font-bold text-gray-600 mb-2">هيكل الصلاحيات الموصى به</p>
              <div className="space-y-2">
                {[
                  { role: 'مسؤول النظام (Admin)',   perms: 'كل الصلاحيات — يُعطى بحذر لشخص واحد أو اثنين فقط' },
                  { role: 'مدير حوكمة',              perms: 'قراءة وتعديل كل الوحدات — لا حذف' },
                  { role: 'مشرف قسم',               perms: 'قراءة وتعديل سجلات قسمه فقط' },
                  { role: 'موظف',                   perms: 'قراءة، الرد على الاستبيانات، الإقرار بالسياسات' },
                  { role: 'مراجع داخلي',             perms: 'قراءة فقط لكل الوحدات + تقارير' },
                ].map(r => (
                  <div key={r.role} className="flex gap-3 text-xs">
                    <span className="font-semibold text-gray-700 w-32 flex-shrink-0">{r.role}</span>
                    <span className="text-gray-500">{r.perms}</span>
                  </div>
                ))}
              </div>
            </div>

            <Steps items={[
              { title: 'انتقل لصفحة الأدوار', path: 'الأدوار والصلاحيات' },
              { title: 'افتح الدور المطلوب تعديله', detail: 'مثال: دور "مشرف قسم"' },
              { title: 'عدّل الصلاحيات', detail: 'ضع علامة ✓ على كل صلاحية تريد منحها أو إلغاءها' },
              { title: 'احفظ التغييرات', detail: 'تُطبَّق فوراً على كل من يحمل هذا الدور' },
            ]} />

            <Note type="warning">
              تعديل صلاحيات الدور يؤثر فوراً على جميع المستخدمين المرتبطين به — تصرّف بحذر.
            </Note>
          </Section>

          {/* ══════════════════════════════════════════ التقارير */}
          <Section id="reports" icon="📊" title="التقارير والتحليلات">
            <p className="text-sm text-gray-600 mt-3">
              النظام يوفر عدة مستويات من التقارير:
            </p>
            <div className="space-y-3 mt-3">
              {[
                { title: 'لوحة الرئيسية', desc: 'ملخص سريع: حوادث مفتوحة، مخاطر حرجة، مهام متأخرة، مؤشرات أداء', path: 'الرئيسية' },
                { title: 'لوحة التنفيذي', desc: 'تقرير شامل للإدارة العليا: كل المؤشرات والمخاطر والامتثال في صفحة واحدة', path: 'لوحة التنفيذي' },
                { title: 'تقرير الأقسام (Scorecard)', desc: 'تقييم أداء كل قسم مقارنةً بمؤشراته', path: 'التقارير ← تقرير الأقسام' },
                { title: 'مكتبة الامتثال', desc: 'نسبة تغطية كل إطار عمل (ISO 27001 / NESA) بناءً على الضوابط المختبَرة', path: 'مكتبة الامتثال' },
                { title: 'تصدير Excel', desc: 'المخاطر والحوادث والمهام قابلة للتصدير كـ Excel للتحليل الخارجي' },
                { title: 'طباعة / PDF', desc: 'أي صفحة تقرير يمكن طباعتها أو حفظها كـ PDF من المتصفح', },
              ].map(r => (
                <div key={r.title} className="flex gap-3 p-3 bg-gray-50 rounded-xl">
                  <div className="flex-1">
                    <p className="text-sm font-semibold text-gray-800">{r.title}</p>
                    <p className="text-xs text-gray-500 mt-0.5">{r.desc}</p>
                  </div>
                  {r.path && (
                    <span className="text-xs text-blue-600 font-mono bg-blue-50 px-2 py-0.5 rounded self-start flex-shrink-0">
                      {r.path}
                    </span>
                  )}
                </div>
              ))}
            </div>
          </Section>

          {/* نهاية */}
          <div className="bg-emerald-50 border border-emerald-200 rounded-xl p-4 text-center">
            <p className="text-sm font-bold text-emerald-800">💡 نصيحة عامة</p>
            <p className="text-xs text-emerald-700 mt-1">
              ابدأ بالأقسام والمستخدمين ← ثم السياسات ← ثم المخاطر والمؤشرات ← ثم الباقي حسب الأولوية.
              <br />النظام مصمم ليكون مرجعاً يومياً للمدير — كلما وثّقت أكثر كان التقرير أدق.
            </p>
          </div>

        </div>
      </div>
    </div>
  )
}
