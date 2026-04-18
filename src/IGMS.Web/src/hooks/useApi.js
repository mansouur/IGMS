import { useState, useCallback } from 'react'
import { toast } from '../store/toastStore'
import useConfirmStore from '../store/confirmStore'

/**
 * useApi – hook موحّد لاستدعاء API.
 *
 * يوفر:
 *   loading  : boolean – هل الطلب جارٍ؟
 *   error    : string | null – رسالة الخطأ
 *   execute  : (apiCall, options?) => Promise<data | null>
 *
 * Options:
 *   successMsg : رسالة Toast عند النجاح (اختياري)
 *   errorMsg   : رسالة Toast عند الفشل (يُستخدم كـ fallback)
 *   silent     : true = لا تُظهر Toast للأخطاء
 *
 * المُرجَع من execute:
 *   - data من response.data إذا نجح
 *   - null إذا فشل
 */
export function useApi() {
  const [loading, setLoading] = useState(false)
  const [error,   setError]   = useState(null)

  const execute = useCallback(async (apiCall, options = {}) => {
    const { successMsg, errorMsg, silent = false } = options
    setLoading(true)
    setError(null)

    try {
      const response = await apiCall()
      const data = response?.data?.data ?? response?.data ?? null

      if (successMsg) toast.success(successMsg)
      return data

    } catch (err) {
      const serverError =
        err.response?.data?.errors?.[0] ??
        err.response?.data?.message     ??
        err.message                      ??
        'حدث خطأ غير متوقع. حاول مجدداً.'

      setError(serverError)

      if (!silent) toast.error(errorMsg ?? serverError)

      return null
    } finally {
      setLoading(false)
    }
  }, [])

  return { loading, error, execute }
}

/**
 * useConfirm – dialog تأكيد مخصص قبل حذف أو إجراء خطير.
 *
 * الاستخدام:
 *   const confirm = useConfirm()
 *   const ok = await confirm('رسالة التأكيد')                           // danger افتراضي
 *   const ok = await confirm({ title:'تأكيد', message:'...', variant:'warning' })
 */
export function useConfirm() {
  const show = useConfirmStore((s) => s.show)

  return useCallback((input) => {
    if (typeof input === 'string') {
      return show({ title: 'تأكيد الحذف', message: input, variant: 'danger' })
    }
    return show(input)
  }, [show])
}
